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
	public partial class ExpUpgradePanel : UIElement,IController
	{
		private void Awake()
        {
            var expUpgradeSystem = this.GetSystem<ExpUpgradeSystem>();

			foreach(var expUpgradeItem in expUpgradeSystem.Items)
            {
                BtnExpUpgradeItemPrefab.InstantiateWithParent(UpgradeRoot)
                .Self(self =>
                {
					var itemCache = expUpgradeItem;
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
							if(expUpgradeSystem.Pairs.TryGetValue(itemCache.Key,out var pairedName))
							{
								var pairedItem = expUpgradeSystem.Dictionary[pairedName];
								if(pairedItem.CurrentLevel.Value > 0 && itemCache.CurrentLevel.Value == 0)
								{
									var pairedNameText = selfCache.transform.Find("PairedName");
									pairedNameText.GetComponent<Text>().text = "配对武器：" + pairedName;
								}
								else
								{
									selfCache.transform.Find("PairedName").Hide();
								}
								
							}
							else
							{
								selfCache.transform.Find("PairedName").Hide();
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
		}

        public IArchitecture GetArchitecture()
        {
            return Global.Interface;
        }
    }
}