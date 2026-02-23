using System.Collections;
using System.Collections.Generic;
using QFramework;
using UnityEngine;

namespace VampireSurvivorLike
{
    public class CoinUpgradeSystem : AbstractSystem,ICanSave
    {
        public static EasyEvent OnCoinUpgradeSystemChanged = new EasyEvent();   ///系统变化事件
        public List<CoinUpgradeItem> Items{get;} = new List<CoinUpgradeItem>();

        public CoinUpgradeItem Add(CoinUpgradeItem item)
        {
            Items.Add(item);
            return item;
        }
        protected override void OnInit()
        {
            LocalizationManager.PreloadTable("upgrade");
            Add(new CoinUpgradeItem()
                .WithKey("coin_percent_Lv1")
                .WithDescriptionKey("coin_upgrade.coin_percent_Lv1.desc")
                .WithPrice(100)
                .OnUpgrade((item)=>
                {
                    Global.CoinPercent.Value += 0.1f;
                    Global.Coin.Value -= item.Price;
                }))
            .Next(Add(new CoinUpgradeItem()
                .WithKey("coin_percent_Lv2")
                .WithDescriptionKey("coin_upgrade.coin_percent_Lv2.desc")
                .WithPrice(500)      
                .OnUpgrade((item)=>
                {
                    Global.CoinPercent.Value += 0.1f;
                    Global.Coin.Value -= item.Price;
                })))
            .Next(Add(new CoinUpgradeItem()
                .WithKey("coin_percent_Lv3")
                .WithDescriptionKey("coin_upgrade.coin_percent_Lv3.desc")
                .WithPrice(2000)
                .OnUpgrade((item)=>
                {
                    Global.CoinPercent.Value += 0.1f;
                    Global.Coin.Value -= item.Price;
                })))
            .Next(Add(new CoinUpgradeItem()
                .WithKey("coin_percent_Lv4")
                .WithDescriptionKey("coin_upgrade.coin_percent_Lv4.desc")
                .WithPrice(5000)
                .OnUpgrade((item)=>
                {
                    Global.CoinPercent.Value += 0.1f;
                    Global.Coin.Value -= item.Price;
                })));
            
            Items.Add(new CoinUpgradeItem()
                .WithKey("exp_percent")
                .WithDescriptionKey("coin_upgrade.exp_percent.desc")
                .WithPrice(5)
                .OnUpgrade((item)=>
                {
                    Global.ExpPercent.Value += 0.1f;
                    Global.Coin.Value -= item.Price;
                }));

            Add(new CoinUpgradeItem()
                .WithKey("player_max_hp")
                .WithDescriptionKey("coin_upgrade.player_max_hp.desc")
                .WithPrice(1000)
                .OnUpgrade((item)=>
                {
                    Global.MaxHP.Value = Mathf.Min(Config.PlayerMaxHPCap, Global.MaxHP.Value + 1);
                    Global.Coin.Value -= item.Price;
                }))
            .Next(Add(new CoinUpgradeItem()
                .WithKey("player_max_hp1")
                .WithDescriptionKey("coin_upgrade.player_max_hp1.desc")
                .WithPrice(2000)
                .OnUpgrade((item)=>
                {
                    Global.MaxHP.Value = Mathf.Min(Config.PlayerMaxHPCap, Global.MaxHP.Value + 1);
                    Global.Coin.Value -= item.Price;
                })))
            .Next(Add(new CoinUpgradeItem()
                .WithKey("player_max_hp2")
                .WithDescriptionKey("coin_upgrade.player_max_hp2.desc")
                .WithPrice(3000)
                .OnUpgrade((item)=>
                {
                    Global.MaxHP.Value = Mathf.Min(Config.PlayerMaxHPCap, Global.MaxHP.Value + 1);
                    Global.Coin.Value -= item.Price;
                })))
            .Next(Add(new CoinUpgradeItem()
                .WithKey("player_max_hp3")
                .WithDescriptionKey("coin_upgrade.player_max_hp3.desc")
                .WithPrice(4000)
                .OnUpgrade((item)=>
                {
                    Global.MaxHP.Value = Mathf.Min(Config.PlayerMaxHPCap, Global.MaxHP.Value + 1);
                    Global.Coin.Value -= item.Price;
                })))
            .Next(Add(new CoinUpgradeItem()
                .WithKey("player_max_hp4")
                .WithDescriptionKey("coin_upgrade.player_max_hp4.desc")
                .WithPrice(5000)
                .OnUpgrade((item)=>
                {
                    Global.MaxHP.Value = Mathf.Min(Config.PlayerMaxHPCap, Global.MaxHP.Value + 1);
                    Global.Coin.Value -= item.Price;
                })));


            Load();

            OnCoinUpgradeSystemChanged.Register(()=>
            {
                Save();
            });
        }

            

        public void Say()
        {
            Debug.Log("Hello,CoinUpgradeSystem");
        }

        public void Save()
        {
            var saveSystem = this.GetSystem<SaveSystem>();
            foreach (var coinUpgradeItem in Items)
            {
                saveSystem.SaveBool(coinUpgradeItem.Key, coinUpgradeItem.UpgradeFinish);
            }
            
        }

        public void Load()
        {
            var saveSystem = this.GetSystem<SaveSystem>();
            foreach (var coinUpgradeItem in Items)
            {
                coinUpgradeItem.UpgradeFinish = saveSystem.LoadBool(coinUpgradeItem.Key, false);
            }
        }
    }
}

