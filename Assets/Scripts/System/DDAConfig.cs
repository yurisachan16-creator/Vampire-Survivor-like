using UnityEngine;

namespace VampireSurvivorLike
{
    [CreateAssetMenu(fileName = "DDAConfig", menuName = "VampireSurvivorLike/DDA Config")]
    public sealed class DDAConfig : ScriptableObject
    {
        [TextArea(2, 4)]
        public string Description = "Dynamic difficulty adjustment runtime tuning.";

        [Header("Evaluation")]
        public float EvaluationWindowSeconds = 15f;
        public float CooldownSeconds = 20f;
        public float OpeningProtectionSeconds = 60f;
        public float FlowLowerThreshold = 0.30f;
        public float FlowUpperThreshold = 0.65f;

        [Header("Runtime Limits")]
        public float SpawnIntervalMultiplierMin = 0.60f;
        public float SpawnIntervalMultiplierMax = 1.50f;
        public float EnemyHpMultiplierMin = 0.70f;
        public float EnemyHpMultiplierMax = 1.40f;
        public float EnemySpeedMultiplierMin = 0.80f;
        public float EnemySpeedMultiplierMax = 1.25f;
        public int BossMaxAdvanceMinutes = 3;
        public int BossMaxDelayMinutes = 5;

        [Header("Pressure Thresholds")]
        public float HealDropBonusRate = 0.15f;
        public float CriticalHpThreshold = 0.35f;
        public float DamageTakenPressureCapPerMinute = 20f;
        public float KillReliefCapPerMinute = 60f;
        public int ActiveEnemyPressureCap = 150;

        [Header("Adjustment Steps")]
        public float EasySpawnIntervalStep = -0.05f;
        public float StressedSpawnIntervalStep = 0.10f;
        public float EasyEnemyHpStep = 0.05f;
        public float StressedEnemyHpStep = -0.08f;
        public float EasyEnemySpeedStep = 0.03f;
        public float StressedEnemySpeedStep = -0.05f;
        public float BossTimeOffsetStepSeconds = 60f;

        [Header("Enemy Count Sampling")]
        public float NearScreenPadding = 2f;

        private static DDAConfig _instance;

        public static DDAConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<DDAConfig>("DDAConfig") ?? CreateDefaultInstance();
                }

                return _instance;
            }
        }

        private void OnValidate()
        {
            EvaluationWindowSeconds = Mathf.Max(1f, EvaluationWindowSeconds);
            CooldownSeconds = Mathf.Max(0f, CooldownSeconds);
            OpeningProtectionSeconds = Mathf.Max(0f, OpeningProtectionSeconds);
            FlowLowerThreshold = Mathf.Clamp01(FlowLowerThreshold);
            FlowUpperThreshold = Mathf.Clamp(FlowUpperThreshold, FlowLowerThreshold, 1f);
            SpawnIntervalMultiplierMin = Mathf.Max(0.05f, SpawnIntervalMultiplierMin);
            SpawnIntervalMultiplierMax = Mathf.Max(SpawnIntervalMultiplierMin, SpawnIntervalMultiplierMax);
            EnemyHpMultiplierMin = Mathf.Max(0.05f, EnemyHpMultiplierMin);
            EnemyHpMultiplierMax = Mathf.Max(EnemyHpMultiplierMin, EnemyHpMultiplierMax);
            EnemySpeedMultiplierMin = Mathf.Max(0.05f, EnemySpeedMultiplierMin);
            EnemySpeedMultiplierMax = Mathf.Max(EnemySpeedMultiplierMin, EnemySpeedMultiplierMax);
            BossMaxAdvanceMinutes = Mathf.Max(0, BossMaxAdvanceMinutes);
            BossMaxDelayMinutes = Mathf.Max(0, BossMaxDelayMinutes);
            HealDropBonusRate = Mathf.Clamp01(HealDropBonusRate);
            CriticalHpThreshold = Mathf.Clamp01(CriticalHpThreshold);
            DamageTakenPressureCapPerMinute = Mathf.Max(0.01f, DamageTakenPressureCapPerMinute);
            KillReliefCapPerMinute = Mathf.Max(0.01f, KillReliefCapPerMinute);
            ActiveEnemyPressureCap = Mathf.Max(1, ActiveEnemyPressureCap);
            BossTimeOffsetStepSeconds = Mathf.Max(1f, BossTimeOffsetStepSeconds);
            NearScreenPadding = Mathf.Max(0f, NearScreenPadding);
        }

        public static DDAConfig CreateDefaultInstance()
        {
            var instance = CreateInstance<DDAConfig>();
            instance.Description = "Auto-created fallback DDA config.";
            instance.EvaluationWindowSeconds = 15f;
            instance.CooldownSeconds = 20f;
            instance.OpeningProtectionSeconds = 60f;
            instance.FlowLowerThreshold = 0.30f;
            instance.FlowUpperThreshold = 0.65f;
            instance.SpawnIntervalMultiplierMin = 0.60f;
            instance.SpawnIntervalMultiplierMax = 1.50f;
            instance.EnemyHpMultiplierMin = 0.70f;
            instance.EnemyHpMultiplierMax = 1.40f;
            instance.EnemySpeedMultiplierMin = 0.80f;
            instance.EnemySpeedMultiplierMax = 1.25f;
            instance.BossMaxAdvanceMinutes = 3;
            instance.BossMaxDelayMinutes = 5;
            instance.HealDropBonusRate = 0.15f;
            instance.CriticalHpThreshold = 0.35f;
            instance.DamageTakenPressureCapPerMinute = 20f;
            instance.KillReliefCapPerMinute = 60f;
            instance.ActiveEnemyPressureCap = 150;
            instance.EasySpawnIntervalStep = -0.05f;
            instance.StressedSpawnIntervalStep = 0.10f;
            instance.EasyEnemyHpStep = 0.05f;
            instance.StressedEnemyHpStep = -0.08f;
            instance.EasyEnemySpeedStep = 0.03f;
            instance.StressedEnemySpeedStep = -0.05f;
            instance.BossTimeOffsetStepSeconds = 60f;
            instance.NearScreenPadding = 2f;
            return instance;
        }
    }
}
