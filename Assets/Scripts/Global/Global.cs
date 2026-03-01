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

        public static BindableProperty<int> HP = new BindableProperty<int>(Config.PlayerBaseMaxHP);
        public static BindableProperty<int> MaxHP = new BindableProperty<int>(Config.PlayerBaseMaxHP);
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
        public static BindableProperty<bool> SimpleAxeUnlocked = new BindableProperty<bool>(false);   //斧头（simple_axe）是否解锁
        public static BindableProperty<bool> MagicWandUnlocked = new BindableProperty<bool>(false);   //魔杖（magic_wand）是否解锁
        public static BindableProperty<bool> SimpleBowUnlocked = new BindableProperty<bool>(false);   //弓箭（simple_bow）是否解锁
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
        public static BindableProperty<float> SimpleAxeDamage = new BindableProperty<float>(Config.InitSimpleAxeDamage);
        public static BindableProperty<float> SimpleAxeDuration = new BindableProperty<float>(Config.InitSimpleAxeDuration);
        public static BindableProperty<int> SimpleAxeCount = new BindableProperty<int>(Config.InitSimpleAxeCount);
        public static BindableProperty<int> SimpleAxePierce = new BindableProperty<int>(Config.InitSimpleAxePierce);

        public static BindableProperty<float> MagicWandDamage = new BindableProperty<float>(Config.InitMagicWandDamage);
        public static BindableProperty<float> MagicWandDuration = new BindableProperty<float>(Config.InitMagicWandDuration);
        public static BindableProperty<int> MagicWandCount = new BindableProperty<int>(Config.InitMagicWandCount);

        public static BindableProperty<float> SimpleBowDamage = new BindableProperty<float>(Config.InitSimpleBowDamage);
        public static BindableProperty<float> SimpleBowDuration = new BindableProperty<float>(Config.InitSimpleBowDuration);
        public static BindableProperty<int> SimpleBowCount = new BindableProperty<int>(Config.InitSimpleBowCount);
        public static BindableProperty<int> SimpleBowPierce = new BindableProperty<int>(Config.InitSimpleBowPierce);
        public static BindableProperty<bool> BoomerangUnlocked = new BindableProperty<bool>(false);   // 飞镖（boomerang）是否解锁
        public static BindableProperty<float> BoomerangDamage = new BindableProperty<float>(Config.InitBoomerangDamage);
        public static BindableProperty<float> BoomerangDuration = new BindableProperty<float>(Config.InitBoomerangDuration);
        public static BindableProperty<int> BoomerangCount = new BindableProperty<int>(Config.InitBoomerangCount);
        public static BindableProperty<int> BoomerangMaxHits = new BindableProperty<int>(Config.InitBoomerangMaxHits);
        public static BindableProperty<int> BoomerangReturnCount = new BindableProperty<int>(Config.InitBoomerangReturnCount);
        public static BindableProperty<bool> HolyWaterUnlocked = new BindableProperty<bool>(false);   // 圣水（holy_water）是否解锁
        public static BindableProperty<float> HolyWaterDamage = new BindableProperty<float>(Config.InitHolyWaterDamage);
        public static BindableProperty<float> HolyWaterDuration = new BindableProperty<float>(Config.InitHolyWaterDuration);
        public static BindableProperty<float> HolyWaterTickInterval = new BindableProperty<float>(Config.InitHolyWaterTickInterval);
        public static BindableProperty<float> HolyWaterSlowMultiplier = new BindableProperty<float>(Config.InitHolyWaterSlowMultiplier);
        public static BindableProperty<float> HolyWaterSlowDuration = new BindableProperty<float>(Config.InitHolyWaterSlowDuration);

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
        public static BindableProperty<bool> SuperAxe = new (false); //超级斧头
        public static BindableProperty<bool> SuperMagicWand = new(false); //超级魔杖
        public static BindableProperty<bool> SuperBow = new(false); //超级弓箭
        public static BindableProperty<bool> SuperBoomerang = new(false); //超级飞镖
        public static BindableProperty<bool> SuperHolyWater = new(false); //超级圣水
        public static BindableProperty<float> ExpPercent = new BindableProperty<float>(0.3f); //经验值掉落概率
        public static BindableProperty<float> CoinPercent = new BindableProperty<float>(0.3f); //金币掉落概率
        public static BindableProperty<int> ArmorValue = new BindableProperty<int>(0); //护甲
        public static BindableProperty<float> AreaMultiplier = new BindableProperty<float>(1f); //范围倍率
        public static BindableProperty<float> CooldownReduction = new BindableProperty<float>(0f); //冷却缩减
        public static BindableProperty<float> LuckValue = new BindableProperty<float>(0f); //幸运
        public static BindableProperty<float> LemonDamageBuffBonus = new BindableProperty<float>(0f); //柠檬伤害增益

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
            //移动端在12帧窗口内忽略重复音效，桌面端保持10帧
            AudioKit.PlaySoundMode = AudioKit.PlaySoundModes.IgnoreSameSoundInGlobalFrames;
            AudioKit.GlobalFrameCountForIgnoreSameSound = Application.isMobilePlatform ? 12 : 10;
            SfxThrottle.Reset();
            
            // WebGL 实际运行时需要异步初始化，在场景启动时完成
            // 编辑器中即使目标是 WebGL，也使用同步初始化（模拟模式）
#if UNITY_WEBGL && !UNITY_EDITOR
            // 异步初始化在 GameStartController/GameUIController 中完成
#else
            ResKit.Init();
#endif
            ApplyMaxHpBalanceMigration();
            UIKit.Root.SetResolution(1920, 1080, 0);
            Global.Coin.Value=PlayerPrefs.GetInt(nameof(Global.Coin),0);

            MaxHP.Value = Mathf.Clamp(
                PlayerPrefs.GetInt(nameof(Global.MaxHP), Config.PlayerBaseMaxHP),
                Config.PlayerBaseMaxHP,
                Config.PlayerMaxHPCap);
            Global.HP.Value = MaxHP.Value;

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
                var clamped = Mathf.Clamp(maxHp, Config.PlayerBaseMaxHP, Config.PlayerMaxHPCap);
                if (MaxHP.Value != clamped)
                {
                    MaxHP.Value = clamped;
                    return;
                }
                PlayerPrefs.SetInt(nameof(Global.MaxHP), clamped);
            });

            var _ = Interface;
        }
        public static void ResetData()
        {
            IsGameOver.Value = false;
            MaxHP.Value = Mathf.Clamp(MaxHP.Value, Config.PlayerBaseMaxHP, Config.PlayerMaxHPCap);
            HP.Value = MaxHP.Value;
            Exp.Value = 0;
            Level.Value = 1;
            CurrentSeconds.Value = 0;

            SimpleSwordUnlocked.Value = false;
            SimpleKnifeUnlocked.Value = false;
            RotateSwordUnlocked.Value = false;
            BasketBallUnlocked.Value = false;
            BombUnlocked.Value = false;
            SimpleAxeUnlocked.Value = false;
            MagicWandUnlocked.Value = false;
            SimpleBowUnlocked.Value = false;
            BoomerangUnlocked.Value = false;
            HolyWaterUnlocked.Value = false;

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
            SimpleAxeDamage.Value = Config.InitSimpleAxeDamage;
            SimpleAxeDuration.Value = Config.InitSimpleAxeDuration;
            SimpleAxeCount.Value = Config.InitSimpleAxeCount;
            SimpleAxePierce.Value = Config.InitSimpleAxePierce;
            MagicWandDamage.Value = Config.InitMagicWandDamage;
            MagicWandDuration.Value = Config.InitMagicWandDuration;
            MagicWandCount.Value = Config.InitMagicWandCount;
            SimpleBowDamage.Value = Config.InitSimpleBowDamage;
            SimpleBowDuration.Value = Config.InitSimpleBowDuration;
            SimpleBowCount.Value = Config.InitSimpleBowCount;
            SimpleBowPierce.Value = Config.InitSimpleBowPierce;
            BoomerangDamage.Value = Config.InitBoomerangDamage;
            BoomerangDuration.Value = Config.InitBoomerangDuration;
            BoomerangCount.Value = Config.InitBoomerangCount;
            BoomerangMaxHits.Value = Config.InitBoomerangMaxHits;
            BoomerangReturnCount.Value = Config.InitBoomerangReturnCount;
            HolyWaterDamage.Value = Config.InitHolyWaterDamage;
            HolyWaterDuration.Value = Config.InitHolyWaterDuration;
            HolyWaterTickInterval.Value = Config.InitHolyWaterTickInterval;
            HolyWaterSlowMultiplier.Value = Config.InitHolyWaterSlowMultiplier;
            HolyWaterSlowDuration.Value = Config.InitHolyWaterSlowDuration;

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
            SuperAxe.Value = false;
            SuperMagicWand.Value = false;
            SuperBow.Value = false;
            SuperBoomerang.Value = false;
            SuperHolyWater.Value = false;
            ArmorValue.Value = 0;
            AreaMultiplier.Value = 1f;
            CooldownReduction.Value = 0f;
            LuckValue.Value = 0f;
            LemonDamageBuffBonus.Value = 0f;
            
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
            PowerUpMergeSystem.ResetStats();
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

            // 斧头（simple_axe）
            var axeConfig = AbilityConfigLoader.GetConfig("simple_axe");
            if (axeConfig != null)
            {
                SimpleAxeDamage.Value = axeConfig.Damage;
                SimpleAxeDuration.Value = axeConfig.Duration;
                SimpleAxeCount.Value = axeConfig.Count;
                SimpleAxePierce.Value = axeConfig.AttackCount > 0 ? axeConfig.AttackCount : Config.InitSimpleAxePierce;
            }

            // 魔杖（magic_wand）
            var magicWandConfig = AbilityConfigLoader.GetConfig("magic_wand");
            if (magicWandConfig != null)
            {
                MagicWandDamage.Value = magicWandConfig.Damage;
                MagicWandDuration.Value = magicWandConfig.Duration;
                MagicWandCount.Value = magicWandConfig.Count > 0 ? magicWandConfig.Count : Config.InitMagicWandCount;
            }

            // 弓箭（simple_bow）
            var simpleBowConfig = AbilityConfigLoader.GetConfig("simple_bow");
            if (simpleBowConfig != null)
            {
                SimpleBowDamage.Value = simpleBowConfig.Damage;
                SimpleBowDuration.Value = simpleBowConfig.Duration;
                SimpleBowCount.Value = simpleBowConfig.Count > 0 ? simpleBowConfig.Count : Config.InitSimpleBowCount;
                SimpleBowPierce.Value = simpleBowConfig.AttackCount > 0 ? simpleBowConfig.AttackCount : Config.InitSimpleBowPierce;
            }

            var boomerangConfig = AbilityConfigLoader.GetConfig("boomerang");
            if (boomerangConfig != null)
            {
                BoomerangDamage.Value = boomerangConfig.Damage;
                BoomerangDuration.Value = boomerangConfig.Duration;
                BoomerangCount.Value = boomerangConfig.Count > 0 ? boomerangConfig.Count : Config.InitBoomerangCount;
                BoomerangMaxHits.Value = boomerangConfig.AttackCount > 0 ? boomerangConfig.AttackCount : Config.InitBoomerangMaxHits;
            }

            var holyWaterConfig = AbilityConfigLoader.GetConfig("holy_water");
            if (holyWaterConfig != null)
            {
                HolyWaterDamage.Value = holyWaterConfig.Damage;
                HolyWaterDuration.Value = holyWaterConfig.Duration > 0 ? holyWaterConfig.Duration : Config.InitHolyWaterDuration;
                HolyWaterTickInterval.Value = holyWaterConfig.Speed > 0 ? holyWaterConfig.Speed : Config.InitHolyWaterTickInterval;
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
            var manager = PowerUpManager.Default;
            if (!manager) return;
            var difficultyProfile = GameSettings.GetActiveRunProfile();

            if(genTreasureChest)
            {
                manager.TreasureChest
                    .Instantiate()
                    .Position(gameObject.Position())
                    .Show();
                return;
            }

            var luck = Mathf.Max(0f, LuckValue.Value);
            var qualityDropMultiplier = 1f + luck * 1.2f;
            var effectiveExpDropRate = Mathf.Clamp01((expDropRate + AdditionalExpPercent.Value) * difficultyProfile.ExpDropRateMultiplier);
            var effectiveCoinDropRate = Mathf.Clamp01(coinDropRate * difficultyProfile.CoinDropRateMultiplier);
            var effectiveHpDropRate = Mathf.Clamp01(hpDropRate * difficultyProfile.HpDropRateMultiplier);
            var effectiveBombDropRate = Mathf.Clamp01(bombDropRate * difficultyProfile.BombDropRateMultiplier);

            //根据概率生成经验值和金币
            var percent=Random.Range(0, 1f);

            if (percent < effectiveExpDropRate)
            {
                var dropPos = gameObject.Position();
                if (PowerUpRegistry.ExpCount >= Config.MaxActiveExpCount)
                {
                    var mergePos = Player.Default ? Player.Default.transform.position : (Vector3)dropPos;
                    PowerUpMergeSystem.TryMergeExpNow(mergePos);
                }

                var expGo = ObjectPoolSystem.Spawn(manager.Exp.gameObject, null, true);
                if (expGo)
                {
                    expGo.transform.position = dropPos;
                    var exp = expGo.GetComponent<Exp>();
                    if (exp)
                    {
                        var scaledValue = Mathf.Max(1, Mathf.RoundToInt(Mathf.Max(1, exp.ExpValue) * difficultyProfile.ExpValueMultiplier));
                        exp.SetExpValue(scaledValue);
                    }
                }

                return;
            }

            percent=Random.Range(0, 1f);

            if (percent < effectiveCoinDropRate)
            {
                var dropPos = gameObject.Position();
                if (PowerUpRegistry.CoinCount >= Config.MaxActiveCoinCountSoft)
                {
                    var mergePos = Player.Default ? Player.Default.transform.position : (Vector3)dropPos;
                    PowerUpMergeSystem.TryMergeCoinNow(mergePos);
                }

                var coinGo = ObjectPoolSystem.Spawn(manager.Coin.gameObject, null, true);
                if (coinGo)
                {
                    coinGo.transform.position = dropPos;
                    var coin = coinGo.GetComponent<Coin>();
                    if (coin)
                    {
                        var scaledValue = Mathf.Max(1, Mathf.RoundToInt(Mathf.Max(1, coin.CoinValue) * difficultyProfile.CoinValueMultiplier));
                        coin.SetCoinValue(scaledValue);
                    }
                }

                return;
            }

            percent=Random.Range(0, 1f);

            var recoverHpDropRate = Mathf.Clamp01(effectiveHpDropRate * Config.RecoverHpDropRateScaleWhenMulti * (1f + luck * 0.4f));
            if(percent < recoverHpDropRate)
            {
                //生成回血道具
                manager.RecoverHP.Instantiate()
                    .Position(gameObject.Position())
                    .Show();

                return;
            }

            if (PowerUpRegistry.ActiveWineCount < Config.MaxActiveWineCount)
            {
                percent = Random.Range(0, 1f);
                if (percent < Mathf.Clamp01(Config.WineDropRate * qualityDropMultiplier))
                {
                    manager.SpawnWine(gameObject.Position());
                    return;
                }
            }

            if (PowerUpRegistry.ActiveLemonBuffCount < Config.MaxActiveLemonBuffCount)
            {
                percent = Random.Range(0, 1f);
                if (percent < Mathf.Clamp01(Config.LemonBuffDropRate * qualityDropMultiplier))
                {
                    manager.SpawnLemonBuff(gameObject.Position());
                    return;
                }
            }

            if (BombUnlocked.Value && PowerUpRegistry.ActiveBombCount == 0)
            {
                percent=Random.Range(0, 1f);

                if(percent < effectiveBombDropRate)
                {
                    //生成炸弹道具
                    manager.Bomb.Instantiate()
                        .Position(gameObject.Position())
                        .Show();

                    return;
                }
            }

            percent=Random.Range(0, 1f);

            if(percent < Mathf.Clamp01(Config.GetAllExpDropRateWhenMulti * qualityDropMultiplier))
            {
                //生成经验吸附道具
                manager.GetAllExp.Instantiate()
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

        private static void ApplyMaxHpBalanceMigration()
        {
            if (PlayerPrefs.GetInt(Config.MaxHpBalanceResetKey, 0) == 1)
            {
                return;
            }

            var hpUpgradeKeys = new[]
            {
                "player_max_hp",
                "player_max_hp1",
                "player_max_hp2",
                "player_max_hp3",
                "player_max_hp4",
                "player_max_hp5",
                "player_max_hp6",
                "player_max_hp7",
                "player_max_hp8",
                "player_max_hp9"
            };

            for (var i = 0; i < hpUpgradeKeys.Length; i++)
            {
                PlayerPrefs.DeleteKey(hpUpgradeKeys[i]);
            }

            PlayerPrefs.SetInt(nameof(MaxHP), Config.PlayerBaseMaxHP);
            PlayerPrefs.SetInt(Config.MaxHpBalanceResetKey, 1);
            PlayerPrefs.Save();
        }
    }
}

