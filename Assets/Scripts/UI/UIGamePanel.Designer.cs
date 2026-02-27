using System;
using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace VampireSurvivorLike
{
	// Generate Id:af85bc5a-0834-4e6a-bd54-20c19d193987
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
		public UnityEngine.UI.Text EnemyCountNextTimeText;
		[SerializeField]
		public UnityEngine.UI.Text EnemyWaveCountText;
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
			EnemyCountNextTimeText = null;
			EnemyWaveCountText = null;
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
