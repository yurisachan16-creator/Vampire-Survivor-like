using System;
using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace VampireSurvivorLike
{
	// Generate Id:748233b7-8f38-4221-876b-69bb9964d427
	public partial class UIGameStartPanel
	{
		public const string Name = "UIGameStartPanel";
		
		[SerializeField]
		public UnityEngine.UI.Button BtnStartGame;
		[SerializeField]
		public UnityEngine.UI.Button BtnCoinUpgrade;
		[SerializeField]
		public UnityEngine.UI.Button BtnAchievement;
		[SerializeField]
		public CoinUpgradePanel CoinUpgradePanel;
		[SerializeField]
		public AchievementPanel AchievementPanel;
		[SerializeField]
		public UnityEngine.UI.Button AchievementItemPrefab;
		[SerializeField]
		public UnityEngine.UI.Button BtnClose;
		[SerializeField]
		public RectTransform AchievementItemRoot;
		
		private UIGameStartPanelData mPrivateData = null;
		
		protected override void ClearUIComponents()
		{
			BtnStartGame = null;
			BtnCoinUpgrade = null;
			BtnAchievement = null;
			CoinUpgradePanel = null;
			AchievementPanel = null;
			AchievementItemPrefab = null;
			BtnClose = null;
			AchievementItemRoot = null;
			
			mData = null;
		}
		
		public UIGameStartPanelData Data
		{
			get
			{
				return mData;
			}
		}
		
		UIGameStartPanelData mData
		{
			get
			{
				return mPrivateData ?? (mPrivateData = new UIGameStartPanelData());
			}
			set
			{
				mUIData = value;
				mPrivateData = value;
			}
		}
	}
}
