/****************************************************************************
 * 2025.12 DESKTOP-JJUC8BO
 ****************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using QFramework;
using UnityEngine.U2D;

namespace VampireSurvivorLike
{
	public partial class ExpUpgradePanel : UIElement,IController
	{
		private ResLoader _mResLoader;

		private void Awake()
        {
			_mResLoader = ResLoader.Allocate();
			var iconAtlas = _mResLoader.LoadSync<SpriteAtlas>("icon");
            var expUpgradeSystem = this.GetSystem<ExpUpgradeSystem>();

			foreach(var expUpgradeItem in expUpgradeSystem.Items)
            {
                BtnExpUpgradeItemPrefab.InstantiateWithParent(UpgradeRoot)
                .Self(self =>
                {
					var itemCache = expUpgradeItem;
					//动态加载图标
					self.transform.Find("Icon").GetComponent<Image>().sprite = 
						iconAtlas.GetSprite(expUpgradeItem.IconName);

					self.onClick.AddListener(() =>
					{
						//恢复游戏
						Time.timeScale = 1f;
						expUpgradeItem.Upgrade();
						this.Hide();
						//TODO:播放升级音效
						AudioKit.PlaySound("Retro Event Acute 08");
					});
					var selfCache=self;
					

                    itemCache.Visible.RegisterWithInitValue(visible =>
					{
                        if(visible)
                        {
							self.GetComponentInChildren<Text>().text = expUpgradeItem.Description;
                            selfCache.Show();
							var pairedUpgradeName = selfCache.transform.Find("PairedUpgradeName");
							if(expUpgradeSystem.Pairs.TryGetValue(itemCache.Key,out var pairedName))
							{
								var pairedItem = expUpgradeSystem.Dictionary[pairedName];
								if(pairedItem.CurrentLevel.Value > 0 && itemCache.CurrentLevel.Value == 0)
								{
									
									pairedUpgradeName.GetComponent<Text>().text = "配对武器：" + pairedItem.Key;
									pairedUpgradeName.Show();
									pairedUpgradeName.Find("Icon").GetComponent<Image>().sprite = 
										iconAtlas.GetSprite(pairedItem.IconName);
								}
								else
								{
									pairedUpgradeName.Hide();
								}
								
							}
							else
							{
								pairedUpgradeName.Hide();
							}
                        }
                        else
                        {
                            selfCache.Hide();
                        }
                    }).UnRegisterWhenGameObjectDestroyed(selfCache);

					itemCache.CurrentLevel.Register((lv) =>
                    {
                        selfCache.GetComponentInChildren<Text>().text = expUpgradeItem.Description;
                    }).UnRegisterWhenGameObjectDestroyed(gameObject);
                });
            }

			

			// //简单攻击间隔时间升级按钮点击事件
			// BtnSimpleDurationUpgrade.onClick.AddListener(()=>
			// {
			// 	//恢复游戏
			// 	Time.timeScale = 1f;
			// 	//缩短简单攻击间隔时间
			// 	Global.SimpleAbilityDuration.Value *= 0.8f;
			// 	//隐藏升级按钮
			// 	UpgradeRoot.Hide();
			// 	//TODO:播放升级音效
			// 	AudioKit.PlaySound("");
			// });
        }

		protected override void OnBeforeDestroy()
		{
			_mResLoader.Recycle2Cache();
			_mResLoader = null;
		}

        public IArchitecture GetArchitecture()
        {
            return Global.Interface;
        }
    }
}