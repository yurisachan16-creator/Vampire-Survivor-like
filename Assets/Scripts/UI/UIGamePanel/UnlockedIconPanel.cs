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
	public partial class UnlockedIconPanel : UIElement,IController
	{
		private Dictionary<string,System.Tuple<ExpUpgradeItem,Image>> _mUnlockedKeys =
			new Dictionary<string, System.Tuple<ExpUpgradeItem, Image>>();

		ResLoader _mResLoader = ResLoader.Allocate();
		private UITooltipView _mTooltipView;
		private void Awake()
        {
			LocalizationManager.PreloadTable("upgrade");
            UnlockedIconPrefab.Hide();

			var iconAtlas = _mResLoader.LoadSync<UnityEngine.U2D.SpriteAtlas>("icon");
			_mTooltipView = UnityEngine.Object.FindObjectOfType<UITooltipView>(true);

			foreach(var expUpgradeItem in this.GetSystem<ExpUpgradeSystem>().Items)
            {
                var cachedItem = expUpgradeItem;
				//监听当前等级变化
                expUpgradeItem.CurrentLevel.RegisterWithInitValue(level=>
				{
                    if(level > 0)
                    {
                        if(_mUnlockedKeys.ContainsKey(cachedItem.Key))
                        {
							var image = _mUnlockedKeys[cachedItem.Key].Item2;
							RefreshTooltip(image, cachedItem, level);
                        }
                        else
                        {
                            UnlockedIconPrefab.InstantiateWithParent(UnlockedIconRoot)
							.Self(self =>
                            {
                                self.sprite = iconAtlas.GetSprite(cachedItem.IconName);
								_mUnlockedKeys.Add(cachedItem.Key,
									new System.Tuple<ExpUpgradeItem, Image>(cachedItem,self));

								RefreshTooltip(self, cachedItem, level);
                            })
							.Show();
                        }
                    }
                }).UnRegisterWhenGameObjectDestroyed(this.gameObject);
            }

            Global.SuperKnife.Register(unlocked =>
            {
                if(unlocked)
				{
                    if (_mUnlockedKeys.ContainsKey("simple_knife"))
                    {
                        var item = _mUnlockedKeys["simple_knife"].Item1;
						var sprite = iconAtlas.GetSprite(item.PairedIconName);
						var image = _mUnlockedKeys["simple_knife"].Item2;
						image.sprite = sprite;
						RefreshTooltip(image, item, item.CurrentLevel.Value);
					}
				}	
            }).UnRegisterWhenGameObjectDestroyed(this.gameObject);

			Global.SuperRotateSword.Register(unlocked =>
            {
                if(unlocked)
				{
                    if (_mUnlockedKeys.ContainsKey("rotate_sword"))
					{
						var item = _mUnlockedKeys["rotate_sword"].Item1;
						var sprite = iconAtlas.GetSprite(item.PairedIconName);
						var image = _mUnlockedKeys["rotate_sword"].Item2;
						image.sprite = sprite;
						RefreshTooltip(image, item, item.CurrentLevel.Value);
					}
				}	
            }).UnRegisterWhenGameObjectDestroyed(this.gameObject);

			Global.SuperBasketBall.Register(unlocked =>
            {
                if(unlocked)
				{
                    if (_mUnlockedKeys.ContainsKey("basket_ball"))
                    {
                        var item = _mUnlockedKeys["basket_ball"].Item1;
						var sprite = iconAtlas.GetSprite(item.PairedIconName);
						var image = _mUnlockedKeys["basket_ball"].Item2;
						image.sprite = sprite;
						RefreshTooltip(image, item, item.CurrentLevel.Value);
					}
				}	
            }).UnRegisterWhenGameObjectDestroyed(this.gameObject);

			Global.SuperBomb.Register(unlocked =>
            {
                if(unlocked)
				{
                    if (_mUnlockedKeys.ContainsKey("simple_bomb"))
                    {
                        var item = _mUnlockedKeys["simple_bomb"].Item1;
						var sprite = iconAtlas.GetSprite(item.PairedIconName);
						var image = _mUnlockedKeys["simple_bomb"].Item2;
						image.sprite = sprite;
						RefreshTooltip(image, item, item.CurrentLevel.Value);
					}
				}	
            }).UnRegisterWhenGameObjectDestroyed(this.gameObject);

			Global.SuperSword.Register(unlocked =>
            {
                if(unlocked)
				{
                    if (_mUnlockedKeys.ContainsKey("simple_sword"))
                    {
                        var item = _mUnlockedKeys["simple_sword"].Item1;
						var sprite = iconAtlas.GetSprite(item.PairedIconName);
						var image = _mUnlockedKeys["simple_sword"].Item2;
						image.sprite = sprite;
						RefreshTooltip(image, item, item.CurrentLevel.Value);
					}
				}	
            }).UnRegisterWhenGameObjectDestroyed(this.gameObject);

			Global.SuperAxe.Register(unlocked =>
			{
				if(unlocked)
				{
					if (_mUnlockedKeys.ContainsKey("simple_axe"))
					{
						var item = _mUnlockedKeys["simple_axe"].Item1;
						var sprite = iconAtlas.GetSprite(item.PairedIconName);
						var image = _mUnlockedKeys["simple_axe"].Item2;
						image.sprite = sprite;
						RefreshTooltip(image, item, item.CurrentLevel.Value);
					}
				}
			}).UnRegisterWhenGameObjectDestroyed(this.gameObject);

			LocalizationManager.CurrentLanguage.Register(_ =>
			{
				foreach (var kv in _mUnlockedKeys)
				{
					var item = kv.Value.Item1;
					var image = kv.Value.Item2;
					if (item != null && image != null)
					{
						RefreshTooltip(image, item, item.CurrentLevel.Value);
					}
				}
			}).UnRegisterWhenGameObjectDestroyed(this.gameObject);
        }

		private void RefreshTooltip(Image image, ExpUpgradeItem item, int level)
		{
			if (!image || item == null) return;

			var trigger = image.GetComponent<TooltipTrigger>();
			if (!trigger) trigger = image.gameObject.AddComponent<TooltipTrigger>();

			trigger.SetTooltipView(_mTooltipView);
			trigger.SetTarget(image.transform as RectTransform);

			var usePaired = IsPairedUnlocked(item.Key);
			var title = usePaired && !string.IsNullOrEmpty(item.PairedName)
				? item.PairedName
				: $"{item.Name} Lv.{Mathf.Max(1, level)}";
			var desc = usePaired && !string.IsNullOrEmpty(item.PairedDescription)
				? item.PairedDescription
				: item.CurrentDescription;

			trigger.SetContent(title, desc);
		}

		private bool IsPairedUnlocked(string key)
		{
			switch (key)
			{
				case "simple_knife": return Global.SuperKnife.Value;
				case "rotate_sword": return Global.SuperRotateSword.Value;
				case "basket_ball": return Global.SuperBasketBall.Value;
				case "simple_bomb": return Global.SuperBomb.Value;
				case "simple_sword": return Global.SuperSword.Value;
				case "simple_axe": return Global.SuperAxe.Value;
				default: return false;
			}
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
