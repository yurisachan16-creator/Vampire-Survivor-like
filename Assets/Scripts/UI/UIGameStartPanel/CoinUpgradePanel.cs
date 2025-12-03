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
	public partial class CoinUpgradePanel : UIElement,IController
	{
	
		private void Awake()
        {
			CoinUpgradeItemPrefab.Hide();

			

			

			Global.Coin.RegisterWithInitValue((coin)=>
			{
				CoinText.text = "金币:" + coin;
			}).UnRegisterWhenGameObjectDestroyed(gameObject);

			foreach(var CoinUpgradeItem in this.GetSystem<CoinUpgradeSystem>().Items.Where(item=>!item.ConditionCheck()))
            {
                CoinUpgradeItemPrefab.InstantiateWithParent(CoinUpgradeItemRoot)
                .Self(self =>
                {
					var itemCache = CoinUpgradeItem;
                    self.GetComponentInChildren<Text>().text = CoinUpgradeItem.Description+$" (价格:{CoinUpgradeItem.Price}金币)";
					self.onClick.AddListener(() =>
					{
						CoinUpgradeItem.Upgrade();
						//TODO:播放升级音效
						AudioKit.PlaySound("");
					});
					var SelfCache=self;
					CoinUpgradeItem.OnChanged.Register(()=>
					{
						if(itemCache.ConditionCheck())
						{
							SelfCache.Show();
						}
                        else
                        {
							SelfCache.Hide();
                        }
					}).UnRegisterWhenGameObjectDestroyed(SelfCache);

					if(itemCache.ConditionCheck())
					{
						SelfCache.Show();
					}
                    else
                    {
						SelfCache.Hide();
                    }

					Global.Coin.RegisterWithInitValue((Coin) =>
					{
                        if (Coin >= itemCache.Price)
                        {
                            SelfCache.interactable = true;
                        }
						else
						{
							SelfCache.interactable = false;
						}
						

					}).UnRegisterWhenGameObjectDestroyed(self);
                });
				
            }

			BtnClose.onClick.AddListener(() =>
			{
				this.Hide();
			});
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