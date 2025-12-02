/****************************************************************************
 * 2025.12 DESKTOP-JJUC8BO
 ****************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace VampireSurvivorLike
{
	public partial class CoinUpgradePanel : UIElement,IController
	{
		private void Awake()
        {
			CoinUpgradeItemPrefab.Hide();

			foreach(var CoinUpgradeItem in this.GetSystem<CoinUpgradeSystem>().Items)
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
                })
				.Show();
            }

			BtnCoinPercentUpgrade.Hide();
			BtnExpPercentUpgrade.Hide();
			BtnPlayerMaxHpUpgrade.Hide();

            BtnCoinPercentUpgrade.onClick.AddListener(() =>
			{
				this.Show();
			});

			Global.Coin.RegisterWithInitValue((Coin) =>
			{
				CoinText.text="金币"+Coin;
                if (Coin >= 5)
                {
                    BtnCoinPercentUpgrade.Show();
					BtnExpPercentUpgrade.Show();
                }
				else
				{
					BtnCoinPercentUpgrade.Hide();
					BtnExpPercentUpgrade.Hide();
				}
			}).UnRegisterWhenGameObjectDestroyed(gameObject);

			
			BtnCoinPercentUpgrade.onClick.AddListener(() =>
            {
                
				
            });

			
			BtnExpPercentUpgrade.onClick.AddListener(() =>
			{
				Global.ExpPercent.Value += 0.1f;
				Global.Coin.Value -= 5;
				//TODO:播放升级音效
				AudioKit.PlaySound("");
			});

			BtnPlayerMaxHpUpgrade.onClick.AddListener(() =>
			{
				Global.MaxHP.Value++;
				Global.Coin.Value -= 5;
				//TODO:播放升级音效
				AudioKit.PlaySound("");
			});

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