using System;
using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace VampireSurvivorLike
{
	// Generate Id:ecfb6f3c-a4c3-45aa-bd40-ceaab088f942
	public partial class UIGamePanel
	{
		public const string Name = "UIGamePanel";
		
		[SerializeField]
		public UnityEngine.UI.Text HPText;
		[SerializeField]
		public UnityEngine.UI.Text ExpText;
		[SerializeField]
		public UnityEngine.UI.Text LevelText;
		[SerializeField]
		public UnityEngine.UI.Text TimeText;
		[SerializeField]
		public UnityEngine.UI.Text EnemyCountText;
		[SerializeField]
		public UnityEngine.UI.Text CoinText;
		[SerializeField]
		public RectTransform UpgradeRoot;
		[SerializeField]
		public UnityEngine.UI.Button BtnUpgrade;
		[SerializeField]
		public UnityEngine.UI.Button BtnSimpleDurationUpgrade;
		
		private UIGamePanelData mPrivateData = null;
		
		protected override void ClearUIComponents()
		{
			HPText = null;
			ExpText = null;
			LevelText = null;
			TimeText = null;
			EnemyCountText = null;
			CoinText = null;
			UpgradeRoot = null;
			BtnUpgrade = null;
			BtnSimpleDurationUpgrade = null;
			
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
