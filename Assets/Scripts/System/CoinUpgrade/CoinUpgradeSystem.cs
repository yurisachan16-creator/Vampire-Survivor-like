using System.Collections;
using System.Collections.Generic;
using QFramework;
using UnityEditor.Search;
using UnityEngine;

namespace VampireSurvivorLike
{
    public class CoinUpgradeSystem : AbstractSystem
    {
        public List<CoinUpgradeItem> Items{get;} = new List<CoinUpgradeItem>();
        protected override void OnInit()
        {
            Items.Add(new CoinUpgradeItem()
            .WithKey("coin_percent")
            .WithDescription("增加金币掉落概率")
            .WithPrice(5)
            .OnUpgrade((item)=>
            {
                Global.CoinPercent.Value += 0.1f;
				Global.Coin.Value -= item.Price;
            }));
        }

        public void Say()
        {
            Debug.Log("Hello,CoinUpgradeSystem");
        }
    }
}

