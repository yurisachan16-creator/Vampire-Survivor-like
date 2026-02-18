using System.Collections;
using System.Collections.Generic;
using QFramework;
using Unity.VisualScripting;
using UnityEngine;

namespace VampireSurvivorLike
{

    public class Global : Architecture<Global>
    {
        #if UNITY_EDITOR
        [UnityEditor.MenuItem("Tool/Clear All Data")]
        public static void ClearAllData()
        {
            PlayerPrefs.DeleteAll();
            UnityEditor.EditorUtility.DisplayDialog("提示","已清除所有数据","确定");
        }
        #endif

        #region Model

        public static BindableProperty<int> HP = new BindableProperty<int>(3);
        public static BindableProperty<int> MaxHP = new BindableProperty<int>(3);
        /// <summary>
        /// 玩家经验值
        /// </summary>
        public static BindableProperty<int> Exp = new BindableProperty<int>(0);
        public static BindableProperty<int> Coin = new BindableProperty<int>(0);
        public static BindableProperty<int> Level = new BindableProperty<int>(1);
        public static BindableProperty<float> CurrentSeconds = new BindableProperty<float>(0);
        public static BindableProperty<bool> SimpleSwordUnlocked = new BindableProperty<bool>(false);   //简单剑是否解锁
        public static BindableProperty<bool> SimpleKnifeUnlocked = new BindableProperty<bool>(false);   //简单刀是否解锁
        public static BindableProperty<bool> RotateSwordUnlocked = new BindableProperty<bool>(false);   //旋转剑是否解锁
        public static BindableProperty<bool> BasketBallUnlocked = new BindableProperty<bool>(false);   //篮球是否解锁
        public static BindableProperty<bool> BombUnlocked = new BindableProperty<bool>(false);   //简单炸弹是否解锁
        public static BindableProperty<float> SimpleAbilityDamage = new BindableProperty<float>(1); //简单攻击伤害
        public static BindableProperty<float> SimpleAbilityDuration = new BindableProperty<float>(1.5f);   //简单攻击间隔时间

        public static BindableProperty<int> SimpleSwordCount = new BindableProperty<int>(Config.InitSimpleSwordCount);   //简单攻击数量
        public static BindableProperty<float> SimpleSwordRange = new BindableProperty<float>(Config.InitSimpleSwordRange);

        public static BindableProperty<float> SimpleKnifeDamage = new BindableProperty<float>(Config.InitSimpleKnifeDamage);
        public static BindableProperty<float> SimpleKnifeDuration = new BindableProperty<float>(Config.InitSimpleKnifeDuration);
        public static BindableProperty<int> SimpleKnifeCount = new BindableProperty<int>(Config.InitSimpleKnifeCount);
        public static BindableProperty<int> SimpleKnifeAttackCount = new BindableProperty<int>(Config.InitSimpleKnifeAttackCount); //穿透数量

        public static BindableProperty<float> RotateSwordDamage = new BindableProperty<float>(Config.InitRotateSwordDamage);
        public static BindableProperty<int> RotateSwordCount = new BindableProperty<int>(Config.InitRotateSwordCount);
        public static BindableProperty<float> RotateSwordSpeed = new BindableProperty<float>(Config.InitRotateSwordSpeed);
        public static BindableProperty<float> RotateSwordRange = new BindableProperty<float>(Config.InitRotateSwordRange);

        public static BindableProperty<float> BasketBallDamage = new BindableProperty<float>(Config.InitBasketBallDamage);
        public static BindableProperty<float> BasketBallSpeed = new BindableProperty<float>(Config.InitBasketBallSpeed);
        public static BindableProperty<int> BasketBallCount = new BindableProperty<int>(Config.InitBasketBallCount);

        public static BindableProperty<float> BombDamage = new BindableProperty<float>(Config.InitBombDamage);
        public static BindableProperty<float> BombPercent = new BindableProperty<float>(Config.InitBombPercent);

        public static BindableProperty<float> CriticalRate = new BindableProperty<float>(Config.InitCriticalRate); //暴击率
        public static BindableProperty<float> DamageRate = new BindableProperty<float>(Config.InitDamageRate); //伤害倍率
        public static BindableProperty<int> AdditionalFlyThingCount = new BindableProperty<int>(0); //额外飞行物数量
        public static BindableProperty<float> MovementSpeedRate = new BindableProperty<float>(Config.InitMovementSpeedRate); //移动速度倍率

        public static BindableProperty<float> CollectableAreaRadius = new BindableProperty<float>(Config.InitCollectableAreaRadius); //可收集物品范围半径
        public static BindableProperty<float> AdditionalExpPercent = new BindableProperty<float>(Config.InitAdditionalExpPercent); //额外经验值获取百分比

        public static BindableProperty<bool> SuperKnife = new (false); //超级简单刀
        public static BindableProperty<bool> SuperSword = new (false); //超级简单剑
        public static BindableProperty<bool> SuperRotateSword = new (false); //超级旋转剑
        public static BindableProperty<bool> SuperBasketBall = new (false); //超级篮球
        public static BindableProperty<bool> SuperBomb = new (false); //超级炸弹
        public static BindableProperty<float> ExpPercent = new BindableProperty<float>(0.3f); //经验值掉落概率
        public static BindableProperty<float> CoinPercent = new BindableProperty<float>(0.3f); //金币掉落概率

        public static BindableProperty<bool> IsGameOver = new BindableProperty<bool>(false);
        public static EasyEvent RequestHPUIRefresh = new EasyEvent();

        public struct PlayerDeathReport
        {
            public string BossId;
            public string DamageSource;
            public int DeathFrame;
        }

        public static PlayerDeathReport LastPlayerDeathReport;

        #endregion

        [RuntimeInitializeOnLoadMethod]
        public static void AutoInit()
        {
            //设置音频播放模式，避免同一音效在短时间内重复播放
            //相同的音效在10帧内只播放一次
            AudioKit.PlaySoundMode = AudioKit.PlaySoundModes.IgnoreSameSoundInGlobalFrames;
            
            // WebGL 实际运行时需要异步初始化，在场景启动时完成
            // 编辑器中即使目标是 WebGL，也使用同步初始化（模拟模式）
#if UNITY_WEBGL && !UNITY_EDITOR
            // 异步初始化在 GameStartController/GameUIController 中完成
#else
            ResKit.Init();
#endif
            UIKit.Root.SetResolution(1920, 1080, 0);
            Global.Coin.Value=PlayerPrefs.GetInt(nameof(Global.Coin),0);
            
            Global.HP.Value=PlayerPrefs.GetInt(nameof(Global.MaxHP),3);
            HP.Value=MaxHP.Value;

            Global.ExpPercent.Value=PlayerPrefs.GetFloat(nameof(Global.ExpPercent),0.3f);
            Global.CoinPercent.Value=PlayerPrefs.GetFloat(nameof(Global.CoinPercent),0.3f);

			Global.Coin.Register((coin) =>
			{
				PlayerPrefs.SetInt(nameof(Global.Coin), coin);

			});

			Global.ExpPercent.Register((expPercent) =>
			{
				PlayerPrefs.SetFloat(nameof(Global.ExpPercent), expPercent);

			});

			Global.CoinPercent.Register((coinPercent) =>
			{
				PlayerPrefs.SetFloat(nameof(Global.CoinPercent), coinPercent);

			});

            Global.MaxHP.Register((maxHp) =>
            {
                PlayerPrefs.SetInt(nameof(Global.MaxHP), maxHp);
            });

            var _ = Interface;
        }
        public static void ResetData()
        {
            IsGameOver.Value = false;
            HP.Value = MaxHP.Value;
            Exp.Value = 0;
            Level.Value = 1;
            CurrentSeconds.Value = 0;

            SimpleSwordUnlocked.Value = false;
            SimpleKnifeUnlocked.Value = false;
            RotateSwordUnlocked.Value = false;
            BasketBallUnlocked.Value = false;
            BombUnlocked.Value = false;

            SimpleAbilityDamage.Value = Config.InitSimpleSwordDamage;
            SimpleAbilityDuration.Value = Config.InitSimpleSwordDuration;

            SimpleSwordCount.Value = Config.InitSimpleSwordCount;
            SimpleSwordRange.Value = Config.InitSimpleSwordRange;

            SimpleKnifeDamage.Value = Config.InitSimpleKnifeDamage;
            SimpleKnifeDuration.Value = Config.InitSimpleKnifeDuration;
            SimpleKnifeCount.Value = Config.InitSimpleKnifeCount;
            SimpleKnifeAttackCount.Value = Config.InitSimpleKnifeAttackCount;

            RotateSwordDamage.Value = Config.InitRotateSwordDamage;
            RotateSwordCount.Value = Config.InitRotateSwordCount;
            RotateSwordSpeed.Value = Config.InitRotateSwordSpeed;
            RotateSwordRange.Value = Config.InitRotateSwordRange;

            BasketBallDamage.Value = Config.InitBasketBallDamage;
            BasketBallSpeed.Value = Config.InitBasketBallSpeed;
            BasketBallCount.Value = Config.InitBasketBallCount;

            BombDamage.Value = Config.InitBombDamage;
            BombPercent.Value = Config.InitBombPercent;

            CriticalRate.Value = Config.InitCriticalRate;

            DamageRate.Value = Config.InitDamageRate;

            MovementSpeedRate.Value = Config.InitMovementSpeedRate;

            AdditionalFlyThingCount.Value = 0;

            CollectableAreaRadius.Value = Config.InitCollectableAreaRadius;

            AdditionalExpPercent.Value = Config.InitAdditionalExpPercent;

            SuperKnife.Value = false;
            SuperSword.Value = false;
            SuperRotateSword.Value = false;
            SuperBasketBall.Value = false;
            SuperBomb.Value = false;
            
            EnemyGenerator.EnemyCount.Value = 0;
            EnemyGenerator.SmallEnemyCount.Value = 0;
            EnemyGenerator.BossEnemyCount.Value = 0;
            EnemyGenerator.CurrentMinute.Value = 0;
            EnemyGenerator.ActiveChannelNames.Value = "";
            EnemyGenerator.GameRemainingTime.Value = 0;
            EnemyGenerator.ActiveChannelCount.Value = 0;
            EnemyGenerator.TotalWaveCount.Value = 30;
            EnemyRegistry.Clear();
            ObjectPoolSystem.ClearAll();
            PowerUpRegistry.Clear();
            Interface.GetSystem<ExpUpgradeSystem>().ResetData();

            // 从配置文件加载技能属性（如果已加载）
            ApplyAbilityConfig();
        }

        public static void ReportPlayerDeath(string bossId, string damageSource)
        {
            LastPlayerDeathReport = new PlayerDeathReport
            {
                BossId = bossId ?? string.Empty,
                DamageSource = damageSource ?? string.Empty,
                DeathFrame = Time.frameCount
            };
            Debug.Log($"[BattleReport] PlayerDead BossId={LastPlayerDeathReport.BossId} Source={LastPlayerDeathReport.DamageSource} Frame={LastPlayerDeathReport.DeathFrame}");
        }

        /// <summary>
        /// 从配置文件应用技能属性
        /// </summary>
        public static void ApplyAbilityConfig()
        {
            if (!AbilityConfigLoader.IsLoaded) return;

            // 剑
            var swordConfig = AbilityConfigLoader.GetConfig("simple_sword");
            if (swordConfig != null)
            {
                SimpleAbilityDamage.Value = swordConfig.Damage;
                SimpleAbilityDuration.Value = swordConfig.Duration;
                SimpleSwordCount.Value = swordConfig.Count;
                SimpleSwordRange.Value = swordConfig.Range;
            }

            // 飞刀
            var knifeConfig = AbilityConfigLoader.GetConfig("simple_knife");
            if (knifeConfig != null)
            {
                SimpleKnifeDamage.Value = knifeConfig.Damage;
                SimpleKnifeDuration.Value = knifeConfig.Duration;
                SimpleKnifeCount.Value = knifeConfig.Count;
                SimpleKnifeAttackCount.Value = knifeConfig.AttackCount;
            }

            // 旋转剑
            var rotateSwordConfig = AbilityConfigLoader.GetConfig("rotate_sword");
            if (rotateSwordConfig != null)
            {
                RotateSwordDamage.Value = rotateSwordConfig.Damage;
                RotateSwordCount.Value = rotateSwordConfig.Count;
                RotateSwordSpeed.Value = rotateSwordConfig.Speed;
                RotateSwordRange.Value = rotateSwordConfig.Range;
            }

            // 篮球
            var basketBallConfig = AbilityConfigLoader.GetConfig("basket_ball");
            if (basketBallConfig != null)
            {
                BasketBallDamage.Value = basketBallConfig.Damage;
                BasketBallCount.Value = basketBallConfig.Count;
                BasketBallSpeed.Value = basketBallConfig.Speed;
            }

            // 炸弹
            var bombConfig = AbilityConfigLoader.GetConfig("bomb");
            if (bombConfig != null)
            {
                BombDamage.Value = bombConfig.Damage;
            }
        }

        /// <summary>
        /// 升级公式
        /// </summary>
        /// <returns></returns> <summary>
        public static int ExpToNextLevel()
        {
            return Global.Level.Value * 5;
        }

        public static void GeneratePowerUp(GameObject gameObject,bool genTreasureChest)
        {
            GeneratePowerUpWithRates(gameObject, genTreasureChest, ExpPercent.Value, CoinPercent.Value, 0.1f, BombPercent.Value);
        }

        /// <summary>
        /// 使用自定义掉落率生成掉落物
        /// </summary>
        public static void GeneratePowerUpWithRates(GameObject gameObject, bool genTreasureChest, 
            float expDropRate, float coinDropRate, float hpDropRate, float bombDropRate)
        {
            if(genTreasureChest)
            {
                PowerUpManager.Default.TreasureChest
                    .Instantiate()
                    .Position(gameObject.Position())
                    .Show();
                return;
            }
            //根据概率生成经验值和金币
            var percent=Random.Range(0, 1f);

            if (percent < expDropRate + AdditionalExpPercent.Value)
            {
                //生成经验值
                PowerUpManager.Default.Exp.Instantiate()
                    .Position(gameObject.Position())
                    .Show();

                return;
            }

            percent=Random.Range(0, 1f);

            if (percent < coinDropRate)
            {
                //生成金币
                PowerUpManager.Default.Coin.Instantiate()
                    .Position(gameObject.Position())
                    .Show();

                return;
            }

            percent=Random.Range(0, 1f);

            if(percent < hpDropRate && PowerUpRegistry.ActiveRecoverHPCount == 0)
            {
                //生成回血道具
                PowerUpManager.Default.RecoverHP.Instantiate()
                    .Position(gameObject.Position())
                    .Show();

                return;
            }

            if (BombUnlocked.Value && PowerUpRegistry.ActiveBombCount == 0)
            {
                percent=Random.Range(0, 1f);

                if(percent < bombDropRate)
                {
                    //生成炸弹道具
                    PowerUpManager.Default.Bomb.Instantiate()
                        .Position(gameObject.Position())
                        .Show();

                    return;
                }
            }

            percent=Random.Range(0, 1f);

            if(percent<0.1f && PowerUpRegistry.ActiveGetAllExpCount == 0)
            {
                //生成经验吸附道具
                PowerUpManager.Default.GetAllExp.Instantiate()
                    .Position(gameObject.Position())
                    .Show();

                return;
            }
        }

        protected override void Init()
        {
            //注册模块的操作
            // XXX Model
            this.RegisterSystem(new SaveSystem());
            this.RegisterSystem(new CoinUpgradeSystem());
            this.RegisterSystem(new ExpUpgradeSystem());
            this.RegisterSystem(new AchievementSystem());
        }
    }
}

