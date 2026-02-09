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
			LocalizationManager.PreloadTable("upgrade");
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
					var iconTransform = self.transform.Find("Icon");
					var iconImage = iconTransform ? iconTransform.GetComponent<Image>() : null;
					if (iconImage && iconAtlas)
					{
						iconImage.sprite = iconAtlas.GetSprite(itemCache.IconName);
					}

					self.onClick.AddListener(() =>
					{
						//恢复游戏
						Time.timeScale = 1f;
						itemCache.Upgrade();
						this.Hide();
						//TODO:播放升级音效
						AudioKit.PlaySound("Retro Event Acute 08");
					});
					var selfCache=self;
					var label = self.GetComponentInChildren<Text>();
					if (label) FontManager.Register(label);

					System.Action refreshTexts = () =>
					{
						if (label) label.text = itemCache.Description;
						var pairedUpgradeName = selfCache.transform.Find("PairedUpgradeName");
						if (pairedUpgradeName && expUpgradeSystem.Pairs.TryGetValue(itemCache.Key, out var pairedName))
						{
							if (expUpgradeSystem.Dictionary.TryGetValue(pairedName, out var pairedItem) && pairedItem != null)
							{
								if (pairedItem.CurrentLevel.Value > 0 && itemCache.CurrentLevel.Value == 0)
								{
									var pairedLabel = pairedUpgradeName.GetComponent<Text>();
									if (pairedLabel)
									{
										FontManager.Register(pairedLabel);
										pairedLabel.text = LocalizationManager.Format("exp_upgrade.ui.paired_weapon", pairedItem.Name);
									}
									pairedUpgradeName.Show();
									var pairedIconTransform = pairedUpgradeName.Find("Icon");
									var pairedIconImage = pairedIconTransform ? pairedIconTransform.GetComponent<Image>() : null;
									if (pairedIconImage && iconAtlas)
									{
										pairedIconImage.sprite = iconAtlas.GetSprite(pairedItem.IconName);
									}
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
							if (pairedUpgradeName) pairedUpgradeName.Hide();
						}
					};

                    itemCache.Visible.RegisterWithInitValue(visible =>
					{
						if (!selfCache) return;
                        if(visible)
                        {
							refreshTexts();
                            selfCache.Show();
                        }
                        else
                        {
                            selfCache.Hide();
                        }
                    }).UnRegisterWhenGameObjectDestroyed(gameObject);

					itemCache.CurrentLevel.Register((lv) =>
                    {
						refreshTexts();
                    }).UnRegisterWhenGameObjectDestroyed(gameObject);

					LocalizationManager.CurrentLanguage.Register(_ =>
					{
						if (!selfCache) return;
						if (selfCache.gameObject.activeInHierarchy) refreshTexts();
					}).UnRegisterWhenGameObjectDestroyed(gameObject);

					LocalizationManager.ReadyChanged.Register(() =>
					{
						if (!selfCache) return;
						if (selfCache.gameObject.activeInHierarchy) refreshTexts();
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
