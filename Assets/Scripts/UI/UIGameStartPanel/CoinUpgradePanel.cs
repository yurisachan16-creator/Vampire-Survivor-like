/****************************************************************************
 * 2025.12 DESKTOP-JJUC8BO
 ****************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using QFramework;
using System.Linq;
using QAssetBundle;

namespace VampireSurvivorLike
{
	public partial class CoinUpgradePanel : UIElement,IController
	{
	
		private void Awake()
        {
			LocalizationManager.PreloadTable("game");
			LocalizationManager.PreloadTable("upgrade");
			CoinUpgradeItemPrefab.Hide();

			

			

			Global.Coin.RegisterWithInitValue((coin)=>
			{
				CoinText.text = LocalizationManager.Format("game.ui.coin", coin);
			}).UnRegisterWhenGameObjectDestroyed(gameObject);

			LocalizationManager.CurrentLanguage.Register(_ =>
			{
				CoinText.text = LocalizationManager.Format("game.ui.coin", Global.Coin.Value);
			}).UnRegisterWhenGameObjectDestroyed(gameObject);
			foreach(var CoinUpgradeItem in this.GetSystem<CoinUpgradeSystem>().Items.Where(item=>item.ConditionCheck()))
            {
                CoinUpgradeItemPrefab.InstantiateWithParent(CoinUpgradeItemRoot)
                .Self(self =>
                {
					var itemCache = CoinUpgradeItem;
					var label = self.GetComponentInChildren<Text>();
					if (label) label.text = LocalizationManager.Format("coin_upgrade.ui.item_price", itemCache.Description, LocaleFormat.Number(itemCache.Price));
					LocalizationManager.CurrentLanguage.Register(_ =>
					{
						if (label) label.text = LocalizationManager.Format("coin_upgrade.ui.item_price", itemCache.Description, LocaleFormat.Number(itemCache.Price));
					}).UnRegisterWhenGameObjectDestroyed(self);
					self.onClick.AddListener(() =>
					{
						CoinUpgradeItem.Upgrade();
						//TODO:播放升级音效
						AudioKit.PlaySound("Retro Event UI 01");
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
				//播放音效
				AudioKit.PlaySound(Sfx.BUTTONCLICK);
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
