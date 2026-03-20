using System;
using System.Collections.Generic;
using System.IO;
using QFramework;
using UnityEngine;

namespace VampireSurvivorLike
{
    public enum StressState
    {
        Easy = 0,
        Flow = 1,
        Stressed = 2
    }

    public readonly struct DDAMetricsSnapshot
    {
        public readonly float HitsPerMinute;
        public readonly float HpRatio;
        public readonly float KillsPerMinute;
        public readonly int ActiveEnemyCount;

        public DDAMetricsSnapshot(float hitsPerMinute, float hpRatio, float killsPerMinute, int activeEnemyCount)
        {
            HitsPerMinute = hitsPerMinute;
            HpRatio = hpRatio;
            KillsPerMinute = killsPerMinute;
            ActiveEnemyCount = activeEnemyCount;
        }
    }

    public static class DDAMath
    {
        public static float NormalizeDamagePressure(float hitsPerMinute, DDAConfig config)
        {
            var cap = config ? config.DamageTakenPressureCapPerMinute : 20f;
            return Mathf.Clamp01(hitsPerMinute / Mathf.Max(0.01f, cap));
        }

        public static float NormalizeHpPressure(float hpRatio)
        {
            return 1f - Mathf.Clamp01(hpRatio);
        }

        public static float NormalizeKillPressure(float killsPerMinute, DDAConfig config)
        {
            var cap = config ? config.KillReliefCapPerMinute : 60f;
            return 1f - Mathf.Clamp01(killsPerMinute / Mathf.Max(0.01f, cap));
        }

        public static float NormalizeEnemyPressure(int activeEnemyCount, DDAConfig config)
        {
            var cap = config ? config.ActiveEnemyPressureCap : 150;
            return Mathf.Clamp01(activeEnemyCount / Mathf.Max(1f, cap));
        }

        public static float ComputePerformanceScore(in DDAMetricsSnapshot metrics, DDAConfig config)
        {
            var damagePressure = NormalizeDamagePressure(metrics.HitsPerMinute, config);
            var hpPressure = NormalizeHpPressure(metrics.HpRatio);
            var killPressure = NormalizeKillPressure(metrics.KillsPerMinute, config);
            var enemyPressure = NormalizeEnemyPressure(metrics.ActiveEnemyCount, config);
            return damagePressure * 0.35f + hpPressure * 0.25f + killPressure * 0.25f + enemyPressure * 0.15f;
        }

        public static StressState DetermineStressState(float performanceScore, DDAConfig config)
        {
            var lower = config ? config.FlowLowerThreshold : 0.30f;
            var upper = config ? config.FlowUpperThreshold : 0.65f;
            if (performanceScore < lower) return StressState.Easy;
            if (performanceScore > upper) return StressState.Stressed;
            return StressState.Flow;
        }
    }

    [Serializable]
    public sealed class DDAAdjustmentLogRecord
    {
        public float timestamp;
        public string stressState;
        public float performanceScore;
        public DDAAdjustmentLogSnapshot adjustments;
    }

    [Serializable]
    public sealed class DDAAdjustmentLogSnapshot
    {
        public float spawnIntervalMultiplier;
        public float enemyHpMultiplier;
        public float enemySpeedMultiplier;
        public float bossTimeOffsetSeconds;
        public float healDropBonusRate;
    }

    [Serializable]
    internal sealed class DDAAdjustmentLogWrapper
    {
        public List<DDAAdjustmentLogRecord> entries = new List<DDAAdjustmentLogRecord>();
    }

    public sealed class DDASystem : AbstractSystem
    {
        private const float CounterSampleIntervalSeconds = 0.25f;

        private readonly Queue<CounterSample> _counterSamples = new Queue<CounterSample>(256);
        private readonly DDAAdjustmentLogWrapper _logWrapper = new DDAAdjustmentLogWrapper();

        private DDAConfig _config;
        private CounterSample _windowStartSample;
        private float _lastSampleTime = float.NegativeInfinity;
        private float _lastAdjustTime = float.NegativeInfinity;
        private bool _lastDdaEnabled = true;
        private int _lastRunSessionId = -1;
        private string _logFilePath = string.Empty;

        public StressState CurrentStressState { get; private set; } = StressState.Flow;
        public float CurrentPerformanceScore { get; private set; } = 0.5f;
        public float RuntimeSpawnIntervalMultiplier { get; private set; } = 1f;
        public float RuntimeEnemyHpMultiplier { get; private set; } = 1f;
        public float RuntimeEnemySpeedMultiplier { get; private set; } = 1f;
        public float RuntimeBossTimeOffsetSeconds { get; private set; }
        public float HealDropBonusRate { get; private set; }

        public static DDASystem TryGet()
        {
            return Global.Interface.GetSystem<DDASystem>();
        }

        public static float GetActiveHealDropBonusRate()
        {
            return TryGet()?.HealDropBonusRate ?? 0f;
        }

        public override string ToString()
        {
            return $"Stress={CurrentStressState} Score={CurrentPerformanceScore:0.000} Spawn={RuntimeSpawnIntervalMultiplier:0.00} Hp={RuntimeEnemyHpMultiplier:0.00} Speed={RuntimeEnemySpeedMultiplier:0.00} BossOffset={RuntimeBossTimeOffsetSeconds:0}s Heal={HealDropBonusRate:0.00}";
        }

        protected override void OnInit()
        {
            _config = DDAConfig.Instance;
            ResetRun();
        }

        public void ResetRun()
        {
            EnsureConfig();
            ResetEvaluationState(clearLogs: true);
            _lastRunSessionId = Global.RunSessionId;
            _lastDdaEnabled = GameSettings.EnableDDA;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            _logFilePath = Path.Combine(Application.persistentDataPath, $"dda_log_{Global.RunSessionId}.json");
            WriteLogFile();
#endif
        }

        public void Tick(EnemyGenerator generator)
        {
            if (!generator) return;

            EnsureConfig();

            if (_lastRunSessionId != Global.RunSessionId)
            {
                ResetRun();
            }

            var ddaEnabled = GameSettings.EnableDDA;
            if (!ddaEnabled)
            {
                if (_lastDdaEnabled)
                {
                    ResetEvaluationState(clearLogs: false);
                }

                _lastDdaEnabled = false;
                ApplyRuntimeModifiers(generator);
                return;
            }

            if (!_lastDdaEnabled)
            {
                ResetEvaluationState(clearLogs: false);
                _lastRunSessionId = Global.RunSessionId;
            }

            _lastDdaEnabled = true;

            var gameTime = Mathf.Max(0f, Global.CurrentSeconds.Value);
            RecordCounterSample(gameTime);

            var hpRatio = Global.MaxHP.Value <= 0 ? 1f : Global.HP.Value / (float)Mathf.Max(1, Global.MaxHP.Value);
            var hitsPerMinute = GetHitsPerMinute(gameTime);
            var killsPerMinute = GetKillsPerMinute(gameTime);
            var activeEnemyCount = generator.ActiveEnemyCountInCamera >= 0
                ? generator.ActiveEnemyCountInCamera
                : EnemyGenerator.EnemyCount.Value;

            var metrics = new DDAMetricsSnapshot(hitsPerMinute, hpRatio, killsPerMinute, activeEnemyCount);
            CurrentPerformanceScore = DDAMath.ComputePerformanceScore(metrics, _config);
            CurrentStressState = DDAMath.DetermineStressState(CurrentPerformanceScore, _config);

            if (gameTime < _config.OpeningProtectionSeconds)
            {
                CurrentStressState = StressState.Flow;
                HealDropBonusRate = 0f;
                ApplyRuntimeModifiers(generator);
                return;
            }

            if (generator.BossAliveCount > 0)
            {
                ApplyRuntimeModifiers(generator);
                return;
            }

            if (CurrentStressState == StressState.Flow)
            {
                HealDropBonusRate = 0f;
                ApplyRuntimeModifiers(generator);
                return;
            }

            if (_lastAdjustTime > float.NegativeInfinity &&
                gameTime - _lastAdjustTime < _config.CooldownSeconds)
            {
                ApplyRuntimeModifiers(generator);
                return;
            }

            ApplyAdjustment(gameTime, hpRatio);
            ApplyRuntimeModifiers(generator);
        }

        private void EnsureConfig()
        {
            _config = DDAConfig.Instance;
        }

        private void ResetEvaluationState(bool clearLogs)
        {
            CurrentStressState = StressState.Flow;
            CurrentPerformanceScore = 0.5f;
            RuntimeSpawnIntervalMultiplier = 1f;
            RuntimeEnemyHpMultiplier = 1f;
            RuntimeEnemySpeedMultiplier = 1f;
            RuntimeBossTimeOffsetSeconds = 0f;
            HealDropBonusRate = 0f;
            _lastAdjustTime = float.NegativeInfinity;
            _lastSampleTime = float.NegativeInfinity;
            _counterSamples.Clear();
            _windowStartSample = default;

            if (!clearLogs) return;

            _logWrapper.entries.Clear();
        }

        private void ApplyAdjustment(float gameTime, float hpRatio)
        {
            if (CurrentStressState == StressState.Easy)
            {
                RuntimeSpawnIntervalMultiplier = Mathf.Clamp(
                    RuntimeSpawnIntervalMultiplier + _config.EasySpawnIntervalStep,
                    _config.SpawnIntervalMultiplierMin,
                    _config.SpawnIntervalMultiplierMax);
                RuntimeEnemyHpMultiplier = Mathf.Clamp(
                    RuntimeEnemyHpMultiplier + _config.EasyEnemyHpStep,
                    _config.EnemyHpMultiplierMin,
                    _config.EnemyHpMultiplierMax);
                RuntimeEnemySpeedMultiplier = Mathf.Clamp(
                    RuntimeEnemySpeedMultiplier + _config.EasyEnemySpeedStep,
                    _config.EnemySpeedMultiplierMin,
                    _config.EnemySpeedMultiplierMax);
                RuntimeBossTimeOffsetSeconds = Mathf.Clamp(
                    RuntimeBossTimeOffsetSeconds - _config.BossTimeOffsetStepSeconds,
                    -_config.BossMaxAdvanceMinutes * 60f,
                    _config.BossMaxDelayMinutes * 60f);
                HealDropBonusRate = 0f;
            }
            else if (CurrentStressState == StressState.Stressed)
            {
                RuntimeSpawnIntervalMultiplier = Mathf.Clamp(
                    RuntimeSpawnIntervalMultiplier + _config.StressedSpawnIntervalStep,
                    _config.SpawnIntervalMultiplierMin,
                    _config.SpawnIntervalMultiplierMax);
                RuntimeEnemyHpMultiplier = Mathf.Clamp(
                    RuntimeEnemyHpMultiplier + _config.StressedEnemyHpStep,
                    _config.EnemyHpMultiplierMin,
                    _config.EnemyHpMultiplierMax);
                RuntimeEnemySpeedMultiplier = Mathf.Clamp(
                    RuntimeEnemySpeedMultiplier + _config.StressedEnemySpeedStep,
                    _config.EnemySpeedMultiplierMin,
                    _config.EnemySpeedMultiplierMax);
                RuntimeBossTimeOffsetSeconds = Mathf.Clamp(
                    RuntimeBossTimeOffsetSeconds + _config.BossTimeOffsetStepSeconds,
                    -_config.BossMaxAdvanceMinutes * 60f,
                    _config.BossMaxDelayMinutes * 60f);
                HealDropBonusRate = hpRatio < _config.CriticalHpThreshold ? _config.HealDropBonusRate : 0f;
            }

            _lastAdjustTime = gameTime;
            AppendLogRecord(gameTime);
        }

        private void RecordCounterSample(float gameTime)
        {
            if (_counterSamples.Count == 0 || gameTime - _lastSampleTime >= CounterSampleIntervalSeconds)
            {
                var sample = new CounterSample(gameTime, Global.RunDamageTakenCount, Global.RunKillCount);
                _counterSamples.Enqueue(sample);
                if (_counterSamples.Count == 1)
                {
                    _windowStartSample = sample;
                }

                _lastSampleTime = gameTime;
            }

            var windowStartTime = Mathf.Max(0f, gameTime - _config.EvaluationWindowSeconds);
            while (_counterSamples.Count > 1 && _counterSamples.Peek().Time <= windowStartTime)
            {
                _windowStartSample = _counterSamples.Dequeue();
            }
        }

        private float GetHitsPerMinute(float gameTime)
        {
            var windowDuration = Mathf.Min(_config.EvaluationWindowSeconds, Mathf.Max(0.01f, gameTime));
            var hitsThisWindow = Mathf.Max(0, Global.RunDamageTakenCount - _windowStartSample.DamageTakenCount);
            return hitsThisWindow * (60f / Mathf.Max(0.01f, windowDuration));
        }

        private float GetKillsPerMinute(float gameTime)
        {
            var windowDuration = Mathf.Min(_config.EvaluationWindowSeconds, Mathf.Max(0.01f, gameTime));
            var killsThisWindow = Mathf.Max(0, Global.RunKillCount - _windowStartSample.KillCount);
            return killsThisWindow * (60f / Mathf.Max(0.01f, windowDuration));
        }

        private void ApplyRuntimeModifiers(EnemyGenerator generator)
        {
            generator.ApplyDdaRuntimeModifiers(
                RuntimeSpawnIntervalMultiplier,
                RuntimeEnemyHpMultiplier,
                RuntimeEnemySpeedMultiplier,
                RuntimeBossTimeOffsetSeconds);
        }

        private void AppendLogRecord(float gameTime)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            _logWrapper.entries.Add(new DDAAdjustmentLogRecord
            {
                timestamp = gameTime,
                stressState = CurrentStressState.ToString(),
                performanceScore = CurrentPerformanceScore,
                adjustments = new DDAAdjustmentLogSnapshot
                {
                    spawnIntervalMultiplier = RuntimeSpawnIntervalMultiplier,
                    enemyHpMultiplier = RuntimeEnemyHpMultiplier,
                    enemySpeedMultiplier = RuntimeEnemySpeedMultiplier,
                    bossTimeOffsetSeconds = RuntimeBossTimeOffsetSeconds,
                    healDropBonusRate = HealDropBonusRate
                }
            });
            WriteLogFile();
#endif
        }

        private void WriteLogFile()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (string.IsNullOrEmpty(_logFilePath)) return;

            try
            {
                var directory = Path.GetDirectoryName(_logFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(_logFilePath, JsonUtility.ToJson(_logWrapper, true));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[DDASystem] Failed writing log file: {e.Message}");
            }
#endif
        }

        private readonly struct CounterSample
        {
            public readonly float Time;
            public readonly int DamageTakenCount;
            public readonly int KillCount;

            public CounterSample(float time, int damageTakenCount, int killCount)
            {
                Time = time;
                DamageTakenCount = damageTakenCount;
                KillCount = killCount;
            }
        }
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    [DisallowMultipleComponent]
    public sealed class DDADebugHud : MonoBehaviour
    {
        private static DDADebugHud _instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            Ensure();
        }

        public static void Ensure()
        {
            if (_instance) return;

            var go = new GameObject(nameof(DDADebugHud));
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<DDADebugHud>();
        }

        private void Awake()
        {
            if (_instance && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        private void OnGUI()
        {
            var dda = DDASystem.TryGet();
            if (dda == null) return;

            const float width = 330f;
            const float height = 154f;
            var rect = new Rect(Screen.width - width - 18f, 18f, width, height);
            GUI.color = new Color(1f, 1f, 1f, 0.92f);
            GUI.Box(rect, "DDA Debug");
            GUI.color = Color.white;

            var inner = new Rect(rect.x + 12f, rect.y + 28f, rect.width - 24f, rect.height - 36f);
            var enabledLabel = GameSettings.EnableDDA ? "On" : "Off";
            GUI.Label(new Rect(inner.x, inner.y, inner.width, 18f), $"Enabled: {enabledLabel}");
            GUI.Label(new Rect(inner.x, inner.y + 20f, inner.width, 18f), $"Stress: {dda.CurrentStressState}");
            GUI.Label(new Rect(inner.x, inner.y + 40f, inner.width, 18f), $"Score: {dda.CurrentPerformanceScore:0.000}");
            GUI.Label(new Rect(inner.x, inner.y + 60f, inner.width, 18f), $"Spawn: {dda.RuntimeSpawnIntervalMultiplier:0.00}   HP: {dda.RuntimeEnemyHpMultiplier:0.00}");
            GUI.Label(new Rect(inner.x, inner.y + 80f, inner.width, 18f), $"Speed: {dda.RuntimeEnemySpeedMultiplier:0.00}   Heal: {dda.HealDropBonusRate:0.00}");
            GUI.Label(new Rect(inner.x, inner.y + 100f, inner.width, 18f), $"Boss Offset: {dda.RuntimeBossTimeOffsetSeconds:0}s");
        }
    }
#endif
}
