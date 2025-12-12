using System;
using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace VampireSurvivorLike
{
	// Generate Id:09238675-fb2b-4a03-a313-0d7c0213f9f2
	public partial class UIGamePanel
	{
		public const string Name = "UIGamePanel";
		
		[SerializeField]
		public UnlockedIconPanel UnlockedIconPanel;
		[SerializeField]
		public UnityEngine.UI.Text LevelText;
		[SerializeField]
		public UnityEngine.UI.Text TimeText;
		[SerializeField]
		public UnityEngine.UI.Text EnemyCountText;
		[SerializeField]
		public UnityEngine.UI.Text CoinText;
		[SerializeField]
		public ExpUpgradePanel ExpUpgradePanel;
		[SerializeField]
		public UnityEngine.UI.Image ExpValue;
		[SerializeField]
		public UnityEngine.UI.Image ScreenColor;
		[SerializeField]
		public TreasureChestPanel TreasureChestPanel;
		[SerializeField]
		public AchievementController AchievementController;
		
		private UIGamePanelData mPrivateData = null;
		
		protected override void ClearUIComponents()
		{
			UnlockedIconPanel = null;
			LevelText = null;
			TimeText = null;
			EnemyCountText = null;
			CoinText = null;
			ExpUpgradePanel = null;
			ExpValue = null;
			ScreenColor = null;
			TreasureChestPanel = null;
			AchievementController = null;
			
			mData = null;
		}
		
		public UIGamePanelData Data
		{
			get
			{
				return mData;
			}
		}
		
		UIGamePanelData mData
		{
			get
			{
				return mPrivateData ?? (mPrivateData = new UIGamePanelData());
			}
			set
			{
				mUIData = value;
				mPrivateData = value;
			}
		}
	}
}
