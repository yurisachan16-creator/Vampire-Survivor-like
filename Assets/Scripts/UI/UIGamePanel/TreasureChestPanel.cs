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
            var expUpgradeSystem = this.GetSystem<ExpUpgradeSystem>();
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

        protected override void OnBeforeDestroy()
		{
		}

        public IArchitecture GetArchitecture()
        {
            return Global.Interface;
        }
    }
}