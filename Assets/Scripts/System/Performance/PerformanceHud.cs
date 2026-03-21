using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Unity.Profiling;
using UnityEngine;

namespace VampireSurvivorLike
{
    [DisallowMultipleComponent]
    public sealed class PerformanceHud : MonoBehaviour
    {
        private static PerformanceHud _instance;

        private const int DefaultCapacity = 2400;
        private const float DefaultSampleIntervalSeconds = 0.25f;

        private readonly List<PerformanceSample> _samples = new List<PerformanceSample>(DefaultCapacity);
        private readonly StringBuilder _sb = new StringBuilder(1024);

        private float _nextSampleTime;
        private float _smoothedDeltaTime = 0.016f;
        private bool _visible = true;
        private int _audioSourceCount;
        private int _audioManagerChildCount;

        private ProfilerRecorder _gcAllocInFrame;
        private ProfilerRecorder _monoUsedSize;
        private ProfilerRecorder _totalUsedMemory;
        private ProfilerRecorder _totalReservedMemory;
        private ProfilerRecorder _batches;
        private ProfilerRecorder _setPass;
        private ProfilerRecorder _triangles;
        private ProfilerRecorder _mainThreadTime;
        private ProfilerRecorder _renderThreadTime;

        public static void ApplyStartup()
        {
            if (!Debug.isDebugBuild && !Application.isEditor) return;

            if (GameSettings.EnablePerformanceHud) Ensure();
            else DestroyIfExists();
        }

        public static void Ensure()
        {
            if (_instance) return;

            var go = new GameObject("PerformanceHud");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<PerformanceHud>();
        }

        private static void DestroyIfExists()
        {
            if (!_instance) return;
            Destroy(_instance.gameObject);
            _instance = null;
        }

        private void Awake()
        {
            if (!Debug.isDebugBuild && !Application.isEditor)
            {
                Destroy(gameObject);
                return;
            }
        }

        private void OnEnable()
        {
            TryStartRecorder(ref _gcAllocInFrame, ProfilerCategory.Memory, "GC Allocated In Frame");
            TryStartRecorder(ref _monoUsedSize, ProfilerCategory.Memory, "Mono Used Size");
            TryStartRecorder(ref _totalUsedMemory, ProfilerCategory.Memory, "Total Used Memory");
            TryStartRecorder(ref _totalReservedMemory, ProfilerCategory.Memory, "Total Reserved Memory");

            TryStartRecorder(ref _batches, ProfilerCategory.Render, "Batches Count");
            TryStartRecorder(ref _setPass, ProfilerCategory.Render, "SetPass Calls Count");
            TryStartRecorder(ref _triangles, ProfilerCategory.Render, "Triangles Count");

            TryStartRecorder(ref _mainThreadTime, ProfilerCategory.Internal, "Main Thread");
            TryStartRecorder(ref _renderThreadTime, ProfilerCategory.Internal, "Render Thread");
        }

        private void OnDisable()
        {
            DisposeRecorder(ref _gcAllocInFrame);
            DisposeRecorder(ref _monoUsedSize);
            DisposeRecorder(ref _totalUsedMemory);
            DisposeRecorder(ref _totalReservedMemory);
            DisposeRecorder(ref _batches);
            DisposeRecorder(ref _setPass);
            DisposeRecorder(ref _triangles);
            DisposeRecorder(ref _mainThreadTime);
            DisposeRecorder(ref _renderThreadTime);
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        private void Update()
        {
            if (!Debug.isDebugBuild && !Application.isEditor) return;

            if (Input.GetKeyDown(KeyCode.F10)) _visible = !_visible;
            if (Input.GetKeyDown(KeyCode.F9) && (Application.platform == RuntimePlatform.WindowsPlayer || Application.isEditor))
            {
                GameSettings.EnablePcInstancedEnemyRenderer = !GameSettings.EnablePcInstancedEnemyRenderer;
            }

            _smoothedDeltaTime = Mathf.Lerp(_smoothedDeltaTime, Time.unscaledDeltaTime, 0.1f);

            if (Time.unscaledTime >= _nextSampleTime)
            {
                _nextSampleTime = Time.unscaledTime + DefaultSampleIntervalSeconds;
                AddSample();
            }
        }

        private void AddSample()
        {
            if (_samples.Count >= DefaultCapacity) _samples.RemoveAt(0);

            _samples.Add(new PerformanceSample
            {
                Time = Time.realtimeSinceStartupAsDouble,
                Fps = 1.0 / Math.Max(0.0001, _smoothedDeltaTime),
                FrameMs = _smoothedDeltaTime * 1000.0,
                GcAllocBytes = ReadRecorderValue(_gcAllocInFrame),
                MonoUsedBytes = ReadRecorderValue(_monoUsedSize),
                TotalUsedBytes = ReadRecorderValue(_totalUsedMemory),
                TotalReservedBytes = ReadRecorderValue(_totalReservedMemory),
                Batches = (int)ReadRecorderValue(_batches),
                SetPass = (int)ReadRecorderValue(_setPass),
                Triangles = ReadRecorderValue(_triangles),
                MainThreadMs = ReadRecorderMs(_mainThreadTime),
                RenderThreadMs = ReadRecorderMs(_renderThreadTime),
                ManagedNearEnemies = EnemySimulationManager.LastManagedNearEnemyCount,
                PhysicsActiveEnemies = EnemySimulationManager.LastPhysicsActiveEnemyCount,
                ManualMeleeHits = EnemySimulationManager.LastManualMeleeHitCount
            });

            _audioSourceCount = CountAudioSources();
            _audioManagerChildCount = CountAudioManagerChildren();
        }

        private void OnGUI()
        {
            if (!_visible) return;
            if (!Debug.isDebugBuild && !Application.isEditor) return;

            const float w = 520f;
            const float h = 420f;

            var rect = new Rect(12f, 12f, w, h);
            GUI.Box(rect, GUIContent.none);

            var y = rect.y + 10f;
            var x = rect.x + 10f;

            if (GUI.Button(new Rect(x, y, 96f, 24f), "Export"))
            {
                ExportSnapshot();
            }
            if (GUI.Button(new Rect(x + 104f, y, 96f, 24f), "Clear"))
            {
                _samples.Clear();
            }
            GUI.Label(new Rect(x + 210f, y + 4f, w - 220f, 24f), "F10 Toggle");
            y += 30f;

            var latest = _samples.Count > 0 ? _samples[_samples.Count - 1] : default;
            _sb.Length = 0;
            _sb.Append("FPS: ").Append(latest.Fps.ToString("0", CultureInfo.InvariantCulture))
                .Append("  Frame: ").Append(latest.FrameMs.ToString("0.00", CultureInfo.InvariantCulture)).Append("ms\n");

            AppendBytesLine("GC Alloc/frame", latest.GcAllocBytes);
            AppendBytesLine("Mono Used", latest.MonoUsedBytes);
            AppendBytesLine("Total Used", latest.TotalUsedBytes);
            AppendBytesLine("Total Reserved", latest.TotalReservedBytes);

            _sb.Append("Batches: ").Append(latest.Batches)
                .Append("  SetPass: ").Append(latest.SetPass)
                .Append("  Tris: ").Append(FormatNumber(latest.Triangles)).Append('\n');

            if (latest.MainThreadMs > 0) _sb.Append("Main Thread: ").Append(latest.MainThreadMs.ToString("0.00", CultureInfo.InvariantCulture)).Append("ms  ");
            if (latest.RenderThreadMs > 0) _sb.Append("Render Thread: ").Append(latest.RenderThreadMs.ToString("0.00", CultureInfo.InvariantCulture)).Append("ms");
            _sb.Append('\n');

            _sb.Append("SRP Batcher: ").Append(UnityEngine.Rendering.GraphicsSettings.useScriptableRenderPipelineBatching ? "On" : "Off").Append('\n');
            _sb.Append("PC Instanced Enemy: ").Append(PcInstancedEnemyRenderer.Enabled ? "On" : "Off").Append('\n');
            _sb.Append("Adaptive Perf: ").Append(MobileAdaptivePerformanceController.CurrentModeLabel).Append('\n');

            _sb.Append("── Game State ──\n");
            _sb.Append("Enemies: ").Append(EnemyGenerator.SmallEnemyCount.Value)
                .Append(" small + ").Append(EnemyGenerator.BossEnemyCount.Value)
                .Append(" boss (total ").Append(EnemyGenerator.EnemyCount.Value).Append(")\n");
            _sb.Append("Managed Near: ").Append(latest.ManagedNearEnemies)
                .Append("  Physics Active: ").Append(latest.PhysicsActiveEnemies)
                .Append("  Manual Melee Hits: ").Append(latest.ManualMeleeHits)
                .Append("  Total: ").Append(EnemySimulationManager.TotalManualMeleeHitCount).Append('\n');
            _sb.Append("Drops: ").Append(PowerUpRegistry.ExpCount).Append(" exp / ")
                .Append(PowerUpRegistry.CoinCount).Append(" coin")
                .Append("  CoinMerge: ").Append(PowerUpMergeSystem.CoinMergeTriggerCount).Append('\n');
            _sb.Append("Minute: ").Append(EnemyGenerator.CurrentMinute.Value)
                .Append("/30  Remaining: ").Append(EnemyGenerator.GameRemainingTime.Value.ToString("0")).Append("s\n");
            _sb.Append("Channels: ").Append(EnemyGenerator.ActiveChannelCount.Value).Append(" active\n");
            var chNames = EnemyGenerator.ActiveChannelNames.Value;
            if (!string.IsNullOrEmpty(chNames))
                _sb.Append("  ").Append(chNames).Append('\n');
            _sb.Append("GameTime: ").Append(Global.CurrentSeconds.Value.ToString("0.0")).Append("s\n");
            _sb.Append("MaxEnemy: ").Append(GameSettings.GetMaxSmallEnemyCountForCurrentPlatform()).Append('\n');
            _sb.Append("AudioSources: ").Append(_audioSourceCount)
                .Append("  AudioManagerChildren: ").Append(_audioManagerChildCount)
                .Append("  SfxDropped: ").Append(SfxThrottle.DroppedCount).Append('\n');

            _sb.Append("Samples: ").Append(_samples.Count).Append('\n');

            GUI.Label(new Rect(x, y, w - 20f, h - 60f), _sb.ToString());
        }

        private void AppendBytesLine(string label, long bytes)
        {
            _sb.Append(label).Append(": ").Append(FormatBytes(bytes)).Append('\n');
        }

        private void ExportSnapshot()
        {
            if (_samples.Count == 0) return;

            var utcNow = DateTime.UtcNow;
            var fileBase = $"perf_{utcNow:yyyyMMdd_HHmmss}_{Application.platform}";

            var csv = BuildCsv();
            var json = BuildJson();

            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                PlayerPrefs.SetString(fileBase + ".csv", csv);
                PlayerPrefs.SetString(fileBase + ".json", json);
                PlayerPrefs.Save();
                Debug.Log($"[PerformanceHud] Exported to PlayerPrefs: {fileBase}.csv / {fileBase}.json");
                return;
            }

            var dir = Path.Combine(Application.persistentDataPath, "benchmarks");
            Directory.CreateDirectory(dir);

            File.WriteAllText(Path.Combine(dir, fileBase + ".csv"), csv, Encoding.UTF8);
            File.WriteAllText(Path.Combine(dir, fileBase + ".json"), json, Encoding.UTF8);

            Debug.Log($"[PerformanceHud] Exported: {dir}\\{fileBase}.csv");
        }

        private string BuildCsv()
        {
            var sb = new StringBuilder(_samples.Count * 80);
            sb.Append("time,fps,frameMs,gcAllocBytes,monoUsedBytes,totalUsedBytes,totalReservedBytes,batches,setPass,triangles,mainThreadMs,renderThreadMs,managedNearEnemies,physicsActiveEnemies,manualMeleeHits\n");
            for (var i = 0; i < _samples.Count; i++)
            {
                var s = _samples[i];
                sb.Append(s.Time.ToString("0.000", CultureInfo.InvariantCulture)).Append(',')
                    .Append(s.Fps.ToString("0.00", CultureInfo.InvariantCulture)).Append(',')
                    .Append(s.FrameMs.ToString("0.000", CultureInfo.InvariantCulture)).Append(',')
                    .Append(s.GcAllocBytes).Append(',')
                    .Append(s.MonoUsedBytes).Append(',')
                    .Append(s.TotalUsedBytes).Append(',')
                    .Append(s.TotalReservedBytes).Append(',')
                    .Append(s.Batches).Append(',')
                    .Append(s.SetPass).Append(',')
                    .Append(s.Triangles).Append(',')
                    .Append(s.MainThreadMs.ToString("0.000", CultureInfo.InvariantCulture)).Append(',')
                    .Append(s.RenderThreadMs.ToString("0.000", CultureInfo.InvariantCulture)).Append(',')
                    .Append(s.ManagedNearEnemies).Append(',')
                    .Append(s.PhysicsActiveEnemies).Append(',')
                    .Append(s.ManualMeleeHits).Append('\n');
            }
            return sb.ToString();
        }

        private string BuildJson()
        {
            var sb = new StringBuilder(_samples.Count * 120);
            sb.Append('{');
            sb.Append("\"platform\":\"").Append(Application.platform).Append("\",");
            sb.Append("\"unity\":\"").Append(Application.unityVersion).Append("\",");
            sb.Append("\"samples\":[");

            for (var i = 0; i < _samples.Count; i++)
            {
                if (i > 0) sb.Append(',');
                var s = _samples[i];
                sb.Append('{');
                sb.Append("\"time\":").Append(s.Time.ToString("0.000", CultureInfo.InvariantCulture)).Append(',');
                sb.Append("\"fps\":").Append(s.Fps.ToString("0.00", CultureInfo.InvariantCulture)).Append(',');
                sb.Append("\"frameMs\":").Append(s.FrameMs.ToString("0.000", CultureInfo.InvariantCulture)).Append(',');
                sb.Append("\"gcAllocBytes\":").Append(s.GcAllocBytes).Append(',');
                sb.Append("\"monoUsedBytes\":").Append(s.MonoUsedBytes).Append(',');
                sb.Append("\"totalUsedBytes\":").Append(s.TotalUsedBytes).Append(',');
                sb.Append("\"totalReservedBytes\":").Append(s.TotalReservedBytes).Append(',');
                sb.Append("\"batches\":").Append(s.Batches).Append(',');
                sb.Append("\"setPass\":").Append(s.SetPass).Append(',');
                sb.Append("\"triangles\":").Append(s.Triangles).Append(',');
                sb.Append("\"mainThreadMs\":").Append(s.MainThreadMs.ToString("0.000", CultureInfo.InvariantCulture)).Append(',');
                sb.Append("\"renderThreadMs\":").Append(s.RenderThreadMs.ToString("0.000", CultureInfo.InvariantCulture)).Append(',');
                sb.Append("\"managedNearEnemies\":").Append(s.ManagedNearEnemies).Append(',');
                sb.Append("\"physicsActiveEnemies\":").Append(s.PhysicsActiveEnemies).Append(',');
                sb.Append("\"manualMeleeHits\":").Append(s.ManualMeleeHits);
                sb.Append('}');
            }

            sb.Append("]}");
            return sb.ToString();
        }

        private static void TryStartRecorder(ref ProfilerRecorder recorder, ProfilerCategory category, string statName)
        {
            try
            {
                recorder = ProfilerRecorder.StartNew(category, statName, 15);
            }
            catch
            {
                recorder = default;
            }
        }

        private static void DisposeRecorder(ref ProfilerRecorder recorder)
        {
            if (!recorder.Valid) return;
            recorder.Dispose();
            recorder = default;
        }

        private static long ReadRecorderValue(ProfilerRecorder recorder)
        {
            if (!recorder.Valid) return 0;
            return recorder.LastValue;
        }

        private static double ReadRecorderMs(ProfilerRecorder recorder)
        {
            if (!recorder.Valid) return 0;
            return recorder.LastValue / 1000000.0;
        }

        private static string FormatBytes(long bytes)
        {
            if (bytes <= 0) return "0 B";
            const double k = 1024.0;
            var b = (double)bytes;
            if (b < k) return $"{bytes} B";
            if (b < k * k) return $"{(b / k):0.00} KB";
            if (b < k * k * k) return $"{(b / (k * k)):0.00} MB";
            return $"{(b / (k * k * k)):0.00} GB";
        }

        private static string FormatNumber(long value)
        {
            if (value < 1000) return value.ToString(CultureInfo.InvariantCulture);
            if (value < 1000_000) return (value / 1000.0).ToString("0.0", CultureInfo.InvariantCulture) + "k";
            if (value < 1000_000_000) return (value / 1000_000.0).ToString("0.0", CultureInfo.InvariantCulture) + "m";
            return (value / 1000_000_000.0).ToString("0.0", CultureInfo.InvariantCulture) + "b";
        }

        private static int CountAudioSources()
        {
            return FindObjectsOfType<AudioSource>(true).Length;
        }

        private static int CountAudioManagerChildren()
        {
            var audioManager = GameObject.Find("QFramework/AudioKit/AudioManager");
            return audioManager ? audioManager.transform.childCount : 0;
        }

        private struct PerformanceSample
        {
            public double Time;
            public double Fps;
            public double FrameMs;
            public long GcAllocBytes;
            public long MonoUsedBytes;
            public long TotalUsedBytes;
            public long TotalReservedBytes;
            public int Batches;
            public int SetPass;
            public long Triangles;
            public double MainThreadMs;
            public double RenderThreadMs;
            public int ManagedNearEnemies;
            public int PhysicsActiveEnemies;
            public int ManualMeleeHits;
        }
    }
}
