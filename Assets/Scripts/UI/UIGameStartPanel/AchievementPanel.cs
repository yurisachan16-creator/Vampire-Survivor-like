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

		private void Awake()
		{
			AchievementItemPrefab.Hide();

			var iconAtlas = _mResLoader.LoadSync<SpriteAtlas>("icon");

			//加载成就列表
			//没完成的放前面
			foreach (var achievementItem in this.GetSystem<AchievementSystem>().Items
							.OrderByDescending(item => item.Unlocked))
			{
				AchievementItemPrefab.InstantiateWithParent(AchievementItemRoot)
					.Self(s =>
					{
						s.GetComponentInChildren<Text>().text = "<b>" + achievementItem.Name +
							(achievementItem.Unlocked ?
							 "<color=green>【已完成】</color>" : "") +
							"</b>\n" + achievementItem.Description;
						var sprite = iconAtlas.GetSprite(achievementItem.IconName);
						s.transform.Find("Icon").GetComponent<Image>().sprite = sprite;
					})
					.Show();
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
			_mResLoader.Recycle2Cache();
			_mResLoader = null;
		}

		public IArchitecture GetArchitecture()
		{
			return Global.Interface;
		}
	}
}
