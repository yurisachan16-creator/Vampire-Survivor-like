using NUnit.Framework;
using UnityEngine;

namespace VampireSurvivorLike.Tests
{
    public class DDASystemTests
    {
        private GameObject _enemyGeneratorGo;
        private EnemyGenerator _enemyGenerator;
        private DDASystem _system;

        [SetUp]
        public void SetUp()
        {
            PlayerPrefs.DeleteKey("DDAEnabled");
            GameSettings.EnableDDA = true;

            Global.ResetData();
            _system = Global.Interface.GetSystem<DDASystem>();
            _system.ResetRun();

            _enemyGeneratorGo = new GameObject("DDATestEnemyGenerator");
            _enemyGenerator = _enemyGeneratorGo.AddComponent<EnemyGenerator>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_enemyGeneratorGo)
            {
                Object.DestroyImmediate(_enemyGeneratorGo);
            }

            EnemyGenerator.BossEnemyCount.Value = 0;
            EnemyGenerator.EnemyCount.Value = 0;
            EnemyGenerator.SmallEnemyCount.Value = 0;
            PlayerPrefs.DeleteKey("DDAEnabled");
        }

        [Test]
        public void PerformanceScore_UsesExpectedWeights()
        {
            var config = DDAConfig.CreateDefaultInstance();
            var metrics = new DDAMetricsSnapshot(
                hitsPerMinute: 10f,
                hpRatio: 0.5f,
                killsPerMinute: 30f,
                activeEnemyCount: 75);

            var score = DDAMath.ComputePerformanceScore(metrics, config);

            Assert.That(score, Is.EqualTo(0.5f).Within(0.0001f));
        }

        [Test]
        public void StressState_UsesConfiguredThresholds()
        {
            var config = DDAConfig.CreateDefaultInstance();

            Assert.That(DDAMath.DetermineStressState(0.2f, config), Is.EqualTo(StressState.Easy));
            Assert.That(DDAMath.DetermineStressState(0.5f, config), Is.EqualTo(StressState.Flow));
            Assert.That(DDAMath.DetermineStressState(0.9f, config), Is.EqualTo(StressState.Stressed));
        }

        [Test]
        public void OpeningProtection_PreventsAdjustments()
        {
            PrimeMetrics(gameTime: 0f, hp: 100, maxHp: 100, hits: 0, kills: 0, activeEnemies: 0);
            PrimeMetrics(gameTime: 30f, hp: 10, maxHp: 100, hits: 20, kills: 0, activeEnemies: 150);

            Assert.That(_system.CurrentStressState, Is.EqualTo(StressState.Flow));
            Assert.That(_enemyGenerator.RuntimeSpawnIntervalMultiplier, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(_system.HealDropBonusRate, Is.EqualTo(0f).Within(0.0001f));
        }

        [Test]
        public void Cooldown_PreventsImmediateRepeatedAdjustment()
        {
            PrimeMetrics(gameTime: 0f, hp: 100, maxHp: 100, hits: 0, kills: 0, activeEnemies: 0);
            PrimeMetrics(gameTime: 61f, hp: 10, maxHp: 100, hits: 20, kills: 0, activeEnemies: 150);
            var firstSpawn = _enemyGenerator.RuntimeSpawnIntervalMultiplier;

            PrimeMetrics(gameTime: 65f, hp: 10, maxHp: 100, hits: 30, kills: 0, activeEnemies: 150);

            Assert.That(_enemyGenerator.RuntimeSpawnIntervalMultiplier, Is.EqualTo(firstSpawn).Within(0.0001f));
        }

        [Test]
        public void BossFreeze_PreventsFurtherAdjustments()
        {
            PrimeMetrics(gameTime: 0f, hp: 100, maxHp: 100, hits: 0, kills: 0, activeEnemies: 0);
            PrimeMetrics(gameTime: 61f, hp: 10, maxHp: 100, hits: 20, kills: 0, activeEnemies: 150);
            var beforeBoss = _enemyGenerator.RuntimeSpawnIntervalMultiplier;

            EnemyGenerator.BossEnemyCount.Value = 1;
            PrimeMetrics(gameTime: 90f, hp: 10, maxHp: 100, hits: 35, kills: 0, activeEnemies: 150);

            Assert.That(_enemyGenerator.RuntimeSpawnIntervalMultiplier, Is.EqualTo(beforeBoss).Within(0.0001f));
        }

        [Test]
        public void LowHpStress_EnablesHealBonus()
        {
            PrimeMetrics(gameTime: 0f, hp: 100, maxHp: 100, hits: 0, kills: 0, activeEnemies: 0);
            PrimeMetrics(gameTime: 61f, hp: 20, maxHp: 100, hits: 20, kills: 0, activeEnemies: 150);

            Assert.That(_system.CurrentStressState, Is.EqualTo(StressState.Stressed));
            Assert.That(_system.HealDropBonusRate, Is.EqualTo(DDAConfig.Instance.HealDropBonusRate).Within(0.0001f));
        }

        [Test]
        public void BossOffset_IsClampedByConfig()
        {
            PrimeMetrics(gameTime: 0f, hp: 100, maxHp: 100, hits: 0, kills: 0, activeEnemies: 0);

            for (var i = 0; i < 10; i++)
            {
                PrimeMetrics(gameTime: 61f + i * 25f, hp: 10, maxHp: 100, hits: 20 + i * 5, kills: 0, activeEnemies: 150);
            }

            Assert.That(_system.RuntimeBossTimeOffsetSeconds,
                Is.EqualTo(DDAConfig.Instance.BossMaxDelayMinutes * 60f).Within(0.0001f));
        }

        private void PrimeMetrics(float gameTime, int hp, int maxHp, int hits, int kills, int activeEnemies)
        {
            Global.CurrentSeconds.Value = gameTime;
            Global.MaxHP.Value = maxHp;
            Global.HP.Value = hp;
            Global.RunDamageTakenCount = hits;
            Global.RunKillCount = kills;
            EnemyGenerator.EnemyCount.Value = activeEnemies;

            _system.Tick(_enemyGenerator);
        }
    }
}
