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
using TMPro;

namespace VampireSurvivorLike
{
	public partial class CoinUpgradePanel : UIElement,IController
	{
		private sealed class ItemView
		{
			public Button Button;
			public Text UguiLabel;
			public TMP_Text TmpLabel;
			public CoinUpgradeItem Item;
		}

		private readonly List<ItemView> _itemViews = new List<ItemView>();
	
		private void Awake()
		{
			LocalizationManager.PreloadTable("game");
			LocalizationManager.PreloadTable("upgrade");
			if (CoinUpgradeItemPrefab) CoinUpgradeItemPrefab.Hide();

			if (!BtnClose)
			{
				var byName = transform.Find("BtnClose") ? transform.Find("BtnClose").GetComponent<Button>() : null;
				if (!byName)
				{
					var buttons = GetComponentsInChildren<Button>(true);
					for (var i = 0; i < buttons.Length; i++)
					{
						var b = buttons[i];
						if (!b) continue;
						if (b.name.Contains("Close"))
						{
							byName = b;
							break;
						}
					}
				}

				if (byName) BtnClose = byName;
			}

			var closeUguiText = BtnClose ? BtnClose.GetComponentInChildren<Text>(true) : null;
			var closeTmpText = BtnClose ? BtnClose.GetComponentInChildren<TMP_Text>(true) : null;

			if (CoinText) FontManager.Register(CoinText);
			if (closeUguiText) FontManager.Register(closeUguiText);
			if (closeTmpText) FontManager.Register(closeTmpText);

			System.Action refreshTexts = () =>
			{
				if (!LocalizationManager.IsReady) return;

				if (CoinText) CoinText.text = LocalizationManager.Format("game.ui.coin", Global.Coin.Value);

				var closeLabel = LocalizationManager.T("coin_upgrade.ui.close");
				if (closeUguiText) closeUguiText.text = closeLabel;
				if (closeTmpText) closeTmpText.text = closeLabel;

				for (var i = 0; i < _itemViews.Count; i++)
				{
					var v = _itemViews[i];
					if (v == null || v.Item == null) continue;
					var text = LocalizationManager.Format("coin_upgrade.ui.item_price", v.Item.Description, LocaleFormat.Number(v.Item.Price));
					if (v.UguiLabel) v.UguiLabel.text = text;
					if (v.TmpLabel) v.TmpLabel.text = text;
				}
			};

			System.Action refreshItemStates = () =>
			{
				var coin = Global.Coin.Value;
				for (var i = 0; i < _itemViews.Count; i++)
				{
					var v = _itemViews[i];
					if (v == null || !v.Button || v.Item == null) continue;

					if (v.Item.ConditionCheck()) v.Button.Show();
					else v.Button.Hide();

					v.Button.interactable = coin >= v.Item.Price;
				}
			};

			if (CoinUpgradeItemRoot && CoinUpgradeItemPrefab)
			{
				var coinUpgradeSystem = this.GetSystem<CoinUpgradeSystem>();
				var items = coinUpgradeSystem != null ? coinUpgradeSystem.Items : null;
				if (items != null)
				{
					for (var i = 0; i < items.Count; i++)
					{
						var itemCache = items[i];
						if (itemCache == null) continue;

						var btn = CoinUpgradeItemPrefab.InstantiateWithParent(CoinUpgradeItemRoot);
						btn.gameObject.name = itemCache.Key;

						var view = new ItemView
						{
							Button = btn,
							UguiLabel = btn ? btn.GetComponentInChildren<Text>(true) : null,
							TmpLabel = btn ? btn.GetComponentInChildren<TMP_Text>(true) : null,
							Item = itemCache
						};
						_itemViews.Add(view);

						if (view.UguiLabel) FontManager.Register(view.UguiLabel);
						if (view.TmpLabel) FontManager.Register(view.TmpLabel);

						btn.onClick.RemoveAllListeners();
						btn.onClick.AddListener(() =>
						{
							if (Global.Coin.Value < itemCache.Price) return;
							itemCache.Upgrade();
							AudioKit.PlaySound("Retro Event UI 01");
							refreshItemStates();
							refreshTexts();
						});

						itemCache.OnChanged.Register(() =>
						{
							refreshItemStates();
							refreshTexts();
						}).UnRegisterWhenGameObjectDestroyed(gameObject);
					}
				}
			}

			Global.Coin.RegisterWithInitValue(_ =>
			{
				refreshItemStates();
				refreshTexts();
			}).UnRegisterWhenGameObjectDestroyed(gameObject);

			LocalizationManager.ReadyChanged.Register(refreshTexts).UnRegisterWhenGameObjectDestroyed(gameObject);
			LocalizationManager.CurrentLanguage.Register(_ => refreshTexts()).UnRegisterWhenGameObjectDestroyed(gameObject);

			if (BtnClose)
			{
				BtnClose.onClick.RemoveAllListeners();
				BtnClose.onClick.AddListener(() =>
				{
					AudioKit.PlaySound(Sfx.BUTTONCLICK);
					this.Hide();
				});
			}

			refreshItemStates();
			refreshTexts();
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
