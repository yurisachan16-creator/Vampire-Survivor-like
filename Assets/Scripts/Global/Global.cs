using System.Collections;
using System.Collections.Generic;
using QFramework;
using UnityEngine;

namespace VampireSurvivorLike
{
    public class Global : Architecture<Global>
    {
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
        public static BindableProperty<float> SimpleAbilityDamage = new BindableProperty<float>(1); //简单攻击伤害
        public static BindableProperty<float> SimpleAbilityDuration = new BindableProperty<float>(1);   //简单攻击间隔时间

        public static BindableProperty<float> ExpPercent = new BindableProperty<float>(0.3f); //经验值掉落概率
        public static BindableProperty<float> CoinPercent = new BindableProperty<float>(0.05f); //金币掉落概率

        #endregion

        [RuntimeInitializeOnLoadMethod]
        public static void AutoInit()
        {
            ResKit.Init();
            UIKit.Root.SetResolution(1920, 1080, 1);
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
        }
        public static void ResetData()
        {
            HP.Value = MaxHP.Value;
            Exp.Value = 0;
            Level.Value = 1;
            CurrentSeconds.Value = 0;
            SimpleAbilityDamage.Value = 1;
            SimpleAbilityDuration.Value = 1.5f;
            EnemyGenerator.EnemyCount.Value = 0;
            Interface.GetSystem<ExpUpgradeSystem>().ResetData();
        }

        /// <summary>
        /// 升级公式
        /// </summary>
        /// <returns></returns> <summary>
        public static int ExpToNextLevel()
        {
            return Global.Level.Value * 5;
        }

        public static void GeneratePowerUp(GameObject gameObject)
        {
            //根据概率生成经验值和金币
            var percent=Random.Range(0, 1f);

            if (percent < ExpPercent.Value)
            {
                //生成经验值
                PowerUpManager.Default.Exp.Instantiate()
                    .Position(gameObject.Position())
                    .Show();

                return;
            }

            percent=Random.Range(0, 1f);

            if (percent < CoinPercent.Value)
            {
                //生成金币
                PowerUpManager.Default.Coin.Instantiate()
                    .Position(gameObject.Position())
                    .Show();

                return;
            }

            percent=Random.Range(0, 1f);

            if(percent<0.1f)
            {
                //生成回血道具
                PowerUpManager.Default.RecoverHP.Instantiate()
                    .Position(gameObject.Position())
                    .Show();

                return;
            }

            percent=Random.Range(0, 1f);

            if(percent<0.1f)
            {
                //生成炸弹道具
                PowerUpManager.Default.Bomb.Instantiate()
                    .Position(gameObject.Position())
                    .Show();

                return;
            }

            percent=Random.Range(0, 1f);

            if(percent<0.1f)
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
        }
    }
}

