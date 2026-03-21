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
    public sealed class BenchmarkHarness : MonoBehaviour
    {
        private const float WarmupSeconds = 2f;
        private const float DurationSeconds = 12f;
        private const float SampleIntervalSeconds = 0.25f;

        private static BenchmarkHarness _instance;

        private readonly List<double> _frameMs = new List<double>(4096);
        private readonly List<GameObject> _spawned = new List<GameObject>(16384);

        private ProfilerRecorder _gcAllocInFrame;
        private ProfilerRecorder _monoUsedSize;
        private ProfilerRecorder _totalUsedMemory;
        private ProfilerRecorder _batches;
        private ProfilerRecorder _setPass;
        private ProfilerRecorder _triangles;

        private bool _visible;
        private bool _running;
        private int _targetCount = 1000;
        private int _spawnBatchSize = 250;
        private int _spawnedCount;
        private float _startTime;
        private float _nextSampleTime;
        private float _smoothedDeltaTime = 0.016f;
        private string _status = "";
        private string _lastExportPath = "";
        private bool _pcInstancedEnemyRenderer;
        private bool _pcInstancedEnemyRendererAtRunStart;
        private double _sumManagedNearEnemies;
        private double _sumPhysicsActiveEnemies;
        private int _manualMeleeHitCountAtRunStart;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (!Debug.isDebugBuild && !Application.isEditor) return;
            if (_instance) return;

            var go = new GameObject("BenchmarkHarness");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<BenchmarkHarness>();
        }

        public static bool IsRunning => _instance && _instance._running;
        public static string Status => _instance ? _instance._status : string.Empty;
        public static string LastExportPath => _instance ? _instance._lastExportPath : string.Empty;

        public static bool TryStart(int targetCount, int spawnBatchSize, bool pcInstancedEnemyRenderer)
        {
            if (!Debug.isDebugBuild && !Application.isEditor) return false;
            if (!_instance) return false;
            if (_instance._running) return false;

            _instance._targetCount = Mathf.Max(0, targetCount);
            _instance._spawnBatchSize = Mathf.Max(1, spawnBatchSize);
            _instance._pcInstancedEnemyRenderer = pcInstancedEnemyRenderer;
            _instance._visible = false;
            _instance.StartRun();
            return true;
        }

        private void OnEnable()
        {
            TryStartRecorder(ref _gcAllocInFrame, ProfilerCategory.Memory, "GC Allocated In Frame");
            TryStartRecorder(ref _monoUsedSize, ProfilerCategory.Memory, "Mono Used Size");
            TryStartRecorder(ref _totalUsedMemory, ProfilerCategory.Memory, "Total Used Memory");
            TryStartRecorder(ref _batches, ProfilerCategory.Render, "Batches Count");
            TryStartRecorder(ref _setPass, ProfilerCategory.Render, "SetPass Calls Count");
            TryStartRecorder(ref _triangles, ProfilerCategory.Render, "Triangles Count");
        }

        private void OnDisable()
        {
            DisposeRecorder(ref _gcAllocInFrame);
            DisposeRecorder(ref _monoUsedSize);
            DisposeRecorder(ref _totalUsedMemory);
            DisposeRecorder(ref _batches);
            DisposeRecorder(ref _setPass);
            DisposeRecorder(ref _triangles);
        }

        private void Update()
        {
            if (!Debug.isDebugBuild && !Application.isEditor) return;

            if (Input.GetKeyDown(KeyCode.F8)) _visible = !_visible;

            if (!_running) return;

            _smoothedDeltaTime = Mathf.Lerp(_smoothedDeltaTime, Time.unscaledDeltaTime, 0.1f);

            if (_spawnedCount < _targetCount)
            {
                SpawnStep();
            }

            if (Time.unscaledTime >= _nextSampleTime)
            {
                _nextSampleTime = Time.unscaledTime + SampleIntervalSeconds;
                Sample();
            }

            var elapsed = Time.unscaledTime - _startTime;
            if (elapsed >= WarmupSeconds + DurationSeconds)
            {
                Finish();
            }
        }

        private void SpawnStep()
        {
            var prefab = TryGetDefaultEnemyPrefab();
            if (!prefab)
            {
                _status = "No EnemyGenerator/Config prefab found";
                _running = false;
                return;
            }

            var player = Player.Default ? Player.Default.transform : null;
            var cam = Camera.main;
            if (!player || !cam)
            {
                _status = "No Player/Camera found";
                _running = false;
                return;
            }

            var orthoSize = cam.orthographic ? cam.orthographicSize : 10f;
            var halfW = orthoSize * cam.aspect;
            var halfH = orthoSize;
            var basePos = (Vector2)player.position;

            var batch = Mathf.Min(_spawnBatchSize, _targetCount - _spawnedCount);
            for (var i = 0; i < batch; i++)
            {
                var angle = UnityEngine.Random.value * Mathf.PI * 2f;
                var radius = UnityEngine.Random.Range(Mathf.Max(halfW, halfH) * 1.1f, Mathf.Max(halfW, halfH) * 1.6f);
                var pos = basePos + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                var go = Instantiate(prefab, pos, Quaternion.identity);
                _spawned.Add(go);
                _spawnedCount++;
            }
        }

        private void Sample()
        {
            var elapsed = Time.unscaledTime - _startTime;
            if (elapsed < WarmupSeconds) return;

            var frameMs = _smoothedDeltaTime * 1000.0;
            _frameMs.Add(frameMs);
            _sumManagedNearEnemies += EnemySimulationManager.LastManagedNearEnemyCount;
            _sumPhysicsActiveEnemies += EnemySimulationManager.LastPhysicsActiveEnemyCount;
        }

        private void Finish()
        {
            _running = false;

            if (_frameMs.Count == 0)
            {
                _status = "No samples collected";
                return;
            }

            var result = ComputeResult();
            _lastExportPath = ExportResult(result);
            _status = $"Done. P95 {result.P95FrameMs:0.00}ms, Avg {result.AvgFrameMs:0.00}ms";
        }

        private BenchmarkResult ComputeResult()
        {
            var data = new List<double>(_frameMs);
            data.Sort();

            var sum = 0.0;
            for (var i = 0; i < data.Count; i++) sum += data[i];
            var avg = sum / Math.Max(1, data.Count);

            var p95Index = Mathf.Clamp((int)Math.Floor((data.Count - 1) * 0.95), 0, data.Count - 1);
            var p95 = data[p95Index];

            return new BenchmarkResult
            {
                Platform = Application.platform.ToString(),
                Unity = Application.unityVersion,
                TargetEnemyCount = _targetCount,
                PcInstancedEnemyRenderer = _pcInstancedEnemyRendererAtRunStart,
                SampleCount = data.Count,
                AvgFrameMs = avg,
                P95FrameMs = p95,
                AvgManagedNearEnemies = _sumManagedNearEnemies / Math.Max(1, data.Count),
                AvgPhysicsActiveEnemies = _sumPhysicsActiveEnemies / Math.Max(1, data.Count),
                ManualMeleeHits = Math.Max(0, EnemySimulationManager.TotalManualMeleeHitCount - _manualMeleeHitCountAtRunStart),
                GcAllocBytes = ReadRecorderValue(_gcAllocInFrame),
                MonoUsedBytes = ReadRecorderValue(_monoUsedSize),
                TotalUsedBytes = ReadRecorderValue(_totalUsedMemory),
                Batches = (int)ReadRecorderValue(_batches),
                SetPass = (int)ReadRecorderValue(_setPass),
                Triangles = ReadRecorderValue(_triangles)
            };
        }

        private string ExportResult(BenchmarkResult r)
        {
            var utcNow = DateTime.UtcNow;
            var fileBase = $"benchmark_{utcNow:yyyyMMdd_HHmmss}_{Application.platform}_{r.TargetEnemyCount}_inst{(r.PcInstancedEnemyRenderer ? 1 : 0)}";

            var csv = BuildCsv(r);
            var json = BuildJson(r);

            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                PlayerPrefs.SetString(fileBase + ".csv", csv);
                PlayerPrefs.SetString(fileBase + ".json", json);
                PlayerPrefs.Save();
                Debug.Log($"[BenchmarkHarness] Exported to PlayerPrefs: {fileBase}.csv / {fileBase}.json");
                return fileBase;
            }

            var dir = Path.Combine(Application.persistentDataPath, "benchmarks");
            Directory.CreateDirectory(dir);
            var csvPath = Path.Combine(dir, fileBase + ".csv");
            var jsonPath = Path.Combine(dir, fileBase + ".json");
            File.WriteAllText(csvPath, csv, Encoding.UTF8);
            File.WriteAllText(jsonPath, json, Encoding.UTF8);
            Debug.Log($"[BenchmarkHarness] Exported: {csvPath}");
            return csvPath;
        }

        private string BuildCsv(BenchmarkResult r)
        {
            var sb = new StringBuilder(512);
            sb.Append("platform,unity,targetEnemyCount,pcInstancedEnemyRenderer,sampleCount,avgFrameMs,p95FrameMs,avgManagedNearEnemies,avgPhysicsActiveEnemies,manualMeleeHits,gcAllocBytes,monoUsedBytes,totalUsedBytes,batches,setPass,triangles\n");
            sb.Append(r.Platform).Append(',')
                .Append(r.Unity).Append(',')
                .Append(r.TargetEnemyCount).Append(',')
                .Append(r.PcInstancedEnemyRenderer ? 1 : 0).Append(',')
                .Append(r.SampleCount).Append(',')
                .Append(r.AvgFrameMs.ToString("0.000", CultureInfo.InvariantCulture)).Append(',')
                .Append(r.P95FrameMs.ToString("0.000", CultureInfo.InvariantCulture)).Append(',')
                .Append(r.AvgManagedNearEnemies.ToString("0.000", CultureInfo.InvariantCulture)).Append(',')
                .Append(r.AvgPhysicsActiveEnemies.ToString("0.000", CultureInfo.InvariantCulture)).Append(',')
                .Append(r.ManualMeleeHits).Append(',')
                .Append(r.GcAllocBytes).Append(',')
                .Append(r.MonoUsedBytes).Append(',')
                .Append(r.TotalUsedBytes).Append(',')
                .Append(r.Batches).Append(',')
                .Append(r.SetPass).Append(',')
                .Append(r.Triangles).Append('\n');
            return sb.ToString();
        }

        private string BuildJson(BenchmarkResult r)
        {
            var sb = new StringBuilder(512);
            sb.Append('{');
            sb.Append("\"platform\":\"").Append(r.Platform).Append("\",");
            sb.Append("\"unity\":\"").Append(r.Unity).Append("\",");
            sb.Append("\"targetEnemyCount\":").Append(r.TargetEnemyCount).Append(',');
            sb.Append("\"pcInstancedEnemyRenderer\":").Append(r.PcInstancedEnemyRenderer ? "true" : "false").Append(',');
            sb.Append("\"sampleCount\":").Append(r.SampleCount).Append(',');
            sb.Append("\"avgFrameMs\":").Append(r.AvgFrameMs.ToString("0.000", CultureInfo.InvariantCulture)).Append(',');
            sb.Append("\"p95FrameMs\":").Append(r.P95FrameMs.ToString("0.000", CultureInfo.InvariantCulture)).Append(',');
            sb.Append("\"avgManagedNearEnemies\":").Append(r.AvgManagedNearEnemies.ToString("0.000", CultureInfo.InvariantCulture)).Append(',');
            sb.Append("\"avgPhysicsActiveEnemies\":").Append(r.AvgPhysicsActiveEnemies.ToString("0.000", CultureInfo.InvariantCulture)).Append(',');
            sb.Append("\"manualMeleeHits\":").Append(r.ManualMeleeHits).Append(',');
            sb.Append("\"gcAllocBytes\":").Append(r.GcAllocBytes).Append(',');
            sb.Append("\"monoUsedBytes\":").Append(r.MonoUsedBytes).Append(',');
            sb.Append("\"totalUsedBytes\":").Append(r.TotalUsedBytes).Append(',');
            sb.Append("\"batches\":").Append(r.Batches).Append(',');
            sb.Append("\"setPass\":").Append(r.SetPass).Append(',');
            sb.Append("\"triangles\":").Append(r.Triangles);
            sb.Append('}');
            return sb.ToString();
        }

        private GameObject TryGetDefaultEnemyPrefab()
        {
            var gen = FindObjectOfType<EnemyGenerator>();
            if (!gen || !gen.PrefabMapping) return null;

            var enemies = gen.PrefabMapping.Enemies;
            if (enemies == null || enemies.Count == 0) return null;

            // 返回第一个有效的敌人预制体
            for (var i = 0; i < enemies.Count; i++)
            {
                var entry = enemies[i];
                if (entry != null && entry.Prefab != null)
                {
                    return entry.Prefab;
                }
            }

            return null;
        }

        private void ClearSpawned()
        {
            for (var i = 0; i < _spawned.Count; i++)
            {
                var go = _spawned[i];
                if (go) Destroy(go);
            }
            _spawned.Clear();
            _spawnedCount = 0;
        }

        private void StartRun()
        {
            ClearSpawned();
            _frameMs.Clear();

            GameSettings.EnablePcInstancedEnemyRenderer = _pcInstancedEnemyRenderer;
            _pcInstancedEnemyRendererAtRunStart = PcInstancedEnemyRenderer.Enabled;

            _running = true;
            _startTime = Time.unscaledTime;
            _nextSampleTime = Time.unscaledTime + SampleIntervalSeconds;
            _status = "Running...";
            _lastExportPath = "";
            _sumManagedNearEnemies = 0.0;
            _sumPhysicsActiveEnemies = 0.0;
            _manualMeleeHitCountAtRunStart = EnemySimulationManager.TotalManualMeleeHitCount;
        }

        private void OnGUI()
        {
            if (!_visible) return;
            if (!Debug.isDebugBuild && !Application.isEditor) return;

            const float w = 520f;
            const float h = 220f;
            var rect = new Rect(12f, 270f, w, h);
            GUI.Box(rect, GUIContent.none);

            var x = rect.x + 10f;
            var y = rect.y + 10f;

            GUI.Label(new Rect(x, y, w - 20f, 22f), "Benchmark (F8 Toggle)");
            y += 26f;

            GUI.Label(new Rect(x, y, 140f, 22f), "Enemy Count");
            _targetCount = DrawIntField(new Rect(x + 150f, y, 120f, 22f), _targetCount);
            y += 26f;

            GUI.Label(new Rect(x, y, 140f, 22f), "Spawn Batch");
            _spawnBatchSize = DrawIntField(new Rect(x + 150f, y, 120f, 22f), _spawnBatchSize);
            y += 30f;

            _pcInstancedEnemyRenderer = GUI.Toggle(new Rect(x, y, w - 20f, 22f), _pcInstancedEnemyRenderer, "PC Instanced Enemy (F9 in HUD)");
            y += 30f;

            GUI.enabled = !_running;
            if (GUI.Button(new Rect(x, y, 120f, 26f), "Start"))
            {
                StartRun();
            }
            GUI.enabled = true;

            if (GUI.Button(new Rect(x + 130f, y, 120f, 26f), "Clear Enemies"))
            {
                ClearSpawned();
            }
            y += 34f;

            if (!string.IsNullOrEmpty(_status))
            {
                GUI.Label(new Rect(x, y, w - 20f, 44f), _status);
                y += 22f;
            }

            if (!string.IsNullOrEmpty(_lastExportPath))
            {
                GUI.Label(new Rect(x, y, w - 20f, 44f), _lastExportPath);
            }
        }

        private static int DrawIntField(Rect rect, int value)
        {
            var text = GUI.TextField(rect, value.ToString(CultureInfo.InvariantCulture));
            if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)) return Mathf.Max(0, parsed);
            return value;
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

        private struct BenchmarkResult
        {
            public string Platform;
            public string Unity;
            public int TargetEnemyCount;
            public bool PcInstancedEnemyRenderer;
            public int SampleCount;
            public double AvgFrameMs;
            public double P95FrameMs;
            public double AvgManagedNearEnemies;
            public double AvgPhysicsActiveEnemies;
            public int ManualMeleeHits;
            public long GcAllocBytes;
            public long MonoUsedBytes;
            public long TotalUsedBytes;
            public int Batches;
            public int SetPass;
            public long Triangles;
        }
    }
}
