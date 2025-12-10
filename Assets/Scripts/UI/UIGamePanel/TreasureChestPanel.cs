/****************************************************************************
 * 2025.12 DESKTOP-JJUC8BO
 ****************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using QFramework;
using System.Linq;

namespace VampireSurvivorLike
{
	public partial class TreasureChestPanel : UIElement,IController
	{
		private void Awake()
        {
            BtnSure.onClick.AddListener(()=>
            {
                Time.timeScale = 1f;
				this.Hide();
            });
        }

        private void OnEnable()
        {
            //超级武器的匹配逻辑类似与游戏《星穹铁道》的活动“银河球棒侠传说”
            //其中一个武器的等级达到7级后，并且另一个配对的武器解锁后，可以进行合成升级
            //1.判断是否有匹配的 没合成的
            //2.判断是否有没升级完成的
            var expUpgradeSystem = this.GetSystem<ExpUpgradeSystem>();

            var matchedPairedItems = expUpgradeSystem.Items.Where(item =>
            {
                if (item.CurrentLevel.Value >= 7)
                {
                    var containsInPair = expUpgradeSystem.Pairs.ContainsKey(item.Key);
                    var pairedItemKey = expUpgradeSystem.Pairs[item.Key];
                    var pairedItemStartUpgrade = expUpgradeSystem.Dictionary[pairedItemKey].CurrentLevel.Value > 0;
                    var pairedUnlocked = expUpgradeSystem.PairedProperties[pairedItemKey].Value;

                    return containsInPair && pairedItemStartUpgrade && pairedUnlocked;
                }

                return false;
            });

            if(matchedPairedItems.Any())
            {
                var item = matchedPairedItems.ToList().GetRandomItem();
                Content.text = "<b>" + "合成后的" + item.Key + "</b>\n";

                //如果是超级武器，直接升级到满级
                while(!item.UpgradeFinish)
                {
                    item.Upgrade();
                }

                expUpgradeSystem.PairedProperties[item.Key].Value = true;
            }
            else
            {
                var upgradeItems = expUpgradeSystem.Items.Where(item=>item.CurrentLevel.Value > 0 &&!item.UpgradeFinish).ToList();

                if (upgradeItems.Any())
                {
                    var item = upgradeItems.GetRandomItem();
                    Content.text = item.Description;
                    item.Upgrade();
                }
                else
                {
                    if(Global.HP.Value < Global.MaxHP.Value)
                    {
                        if (UnityEngine.Random.Range(0, 1.0f) < 0.2f)
                        {
                            Content.text = "恢复1点生命值";
                            AudioKit.PlaySound("Retro Event Acute 08");
                            Global.HP.Value += 1;
                            return;
                        }
                        
                    }

                    Content.text = "增加50点金币";
                    Global.Coin.Value += 50;
                }
            }

            //TODO:

			
        }

        protected override void OnBeforeDestroy()
		{
		}

        public IArchitecture GetArchitecture()
        {
            return Global.Interface;
        }
    }
}