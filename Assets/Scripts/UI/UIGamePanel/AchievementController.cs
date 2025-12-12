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
	public partial class AchievementController : UIElement
	{
		private ResLoader _mResLoader = ResLoader.Allocate();
		private void Awake()
		{
			var originLocalPosY = AchievementItem.LocalPositionY();

			var iconAtlas = _mResLoader.LoadSync<SpriteAtlas>("icon");

			AchievementSystem.OnAchievementUnlocked.Register(item =>
			{
				Title.text = $"<b>成就{item.Name} 达成!</b>";
				Description.text = item.Description;
				var sprite = iconAtlas.GetSprite(item.IconName);
				Icon.sprite = sprite;
				AchievementItem.Show();

				AchievementItem.LocalPositionY(-200);

				AudioKit.PlaySound("Achievement");

				ActionKit.Sequence()
					.Lerp(-200,originLocalPosY,0.3f,(y)=>
					{
						AchievementItem.LocalPositionY(y);
					})
					.Delay(2f)
					.Lerp(originLocalPosY,-200,0.3f,(y)=>
					{
						AchievementItem.LocalPositionY(y);
					}, () =>
					{
						AchievementItem.Hide();
					})
					.Start(this);
			}).UnRegisterWhenGameObjectDestroyed(this);
		}

		protected override void OnBeforeDestroy()
		{
			_mResLoader.Recycle2Cache();
			_mResLoader = null;
		}
	}
}