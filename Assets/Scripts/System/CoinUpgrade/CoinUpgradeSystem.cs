using System.Collections;
using System.Collections.Generic;
using QFramework;
using UnityEditor.Search;
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
            var coinPercentLv1 = Add(new CoinUpgradeItem()
            .WithKey("coin_percent_Lv1")
            .WithDescription("金币掉落概率提升 Lv1")
            .WithPrice(5)
            .OnUpgrade((item)=>
            {
                Global.CoinPercent.Value += 0.1f;
				Global.Coin.Value -= item.Price;
            }));

            var coinPercentLv2 = Add(new CoinUpgradeItem()
            .WithKey("coin_percent_Lv2")
            .WithDescription("金币掉落概率提升 Lv2")
            .WithPrice(10)
            .Condition((_)=>coinPercentLv1.UpgradeFinish)
            .OnUpgrade((item)=>
            {
                Global.CoinPercent.Value += 0.1f;
				Global.Coin.Value -= item.Price;
            }));

            var coinPercentLv3 = Add(new CoinUpgradeItem()
            .WithKey("coin_percent_Lv3")
            .WithDescription("金币掉落概率提升 Lv3")
            .WithPrice(15)
            .Condition((_)=>coinPercentLv2.UpgradeFinish)
            .OnUpgrade((item)=>
            {
                Global.CoinPercent.Value += 0.1f;
				Global.Coin.Value -= item.Price;
            }));

            

            Items.Add(new CoinUpgradeItem()
            .WithKey("exp_percent")
            .WithDescription("增加经验掉落概率")
            .WithPrice(5)
            .OnUpgrade((item)=>
            {
                Global.ExpPercent.Value += 0.1f;
				Global.Coin.Value -= item.Price;
            }));

            Items.Add(new CoinUpgradeItem()
            .WithKey("player_max_hp")
            .WithDescription("玩家最大生命值+1")
            .WithPrice(30)
            .OnUpgrade((item)=>
            {
                Global.MaxHP.Value++;
				Global.Coin.Value -= item.Price;
            }));

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

