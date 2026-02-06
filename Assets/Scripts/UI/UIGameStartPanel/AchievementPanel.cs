/****************************************************************************
 * 2025.12 DESKTOP-JJUC8BO
 ****************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using QFramework;
using UnityEngine.U2D;
using System.Linq;
using QAssetBundle;

namespace VampireSurvivorLike
{
	public partial class AchievementPanel : UIElement, IController
	{
		private ResLoader _mResLoader = ResLoader.Allocate();
		private SpriteAtlas _mIconAtlas;

		private void Awake()
		{
			AchievementItemPrefab.Hide();

			_mIconAtlas = _mResLoader.LoadSync<SpriteAtlas>("icon");

			LocalizationManager.ReadyChanged.Register(RefreshList).UnRegisterWhenGameObjectDestroyed(gameObject);
			RefreshList();

			BtnClose.onClick.AddListener(() =>
			{
				//播放音效
				AudioKit.PlaySound(Sfx.BUTTONCLICK);
				this.Hide();
			});
		}

		private void RefreshList()
		{
			if (!LocalizationManager.IsReady) return;

			for (var i = AchievementItemRoot.childCount - 1; i >= 0; i--)
			{
				var child = AchievementItemRoot.GetChild(i);
				if (!child) continue;
				if (child.gameObject == AchievementItemPrefab.gameObject) continue;
				Destroy(child.gameObject);
			}

			var completedSuffix = LocalizationManager.T("ui.achievement.completed");

			foreach (var achievementItem in this.GetSystem<AchievementSystem>().Items
							.OrderByDescending(item => item.Unlocked))
			{
				AchievementItemPrefab.InstantiateWithParent(AchievementItemRoot)
					.Self(s =>
					{
						var label = s.GetComponentInChildren<Text>(true);
						if (label) FontManager.Register(label);
						if (label)
						{
							label.text = "<b>" + achievementItem.DisplayName +
										(achievementItem.Unlocked ? "<color=green>" + completedSuffix + "</color>" : "") +
										"</b>\n" + achievementItem.DisplayDescription;
						}
						var sprite = _mIconAtlas ? _mIconAtlas.GetSprite(achievementItem.IconName) : null;
						var icon = s.transform.Find("Icon")?.GetComponent<Image>();
						if (icon) icon.sprite = sprite;
					})
					.Show();
			}
		}

		protected override void OnBeforeDestroy()
		{
			_mResLoader.Recycle2Cache();
			_mResLoader = null;
			_mIconAtlas = null;
		}

		public IArchitecture GetArchitecture()
		{
			return Global.Interface;
		}
	}
}
