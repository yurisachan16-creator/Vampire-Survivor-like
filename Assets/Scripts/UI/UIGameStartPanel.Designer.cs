using System;
using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace VampireSurvivorLike
{
	// Generate Id:57b3237b-ee72-4deb-8247-2ffbfed36d44
	public partial class UIGameStartPanel
	{
		public const string Name = "UIGameStartPanel";
		
		[SerializeField]
		public UnityEngine.UI.Button BtnStartGame;
		[SerializeField]
		public UnityEngine.UI.Button BtnSettingsGame;
		[SerializeField]
		public UnityEngine.UI.Button BtnCoinUpgrade;
		[SerializeField]
		public UnityEngine.UI.Button BtnAchievement;
		[SerializeField]
		public CoinUpgradePanel CoinUpgradePanel;
		[SerializeField]
		public AchievementPanel AchievementPanel;
		[SerializeField]
		public UnityEngine.UI.Button BtnClose;
		[SerializeField]
		public UnityEngine.UI.Button AchievementItemPrefab;
		
		private UIGameStartPanelData mPrivateData = null;
		
		protected override void ClearUIComponents()
		{
			BtnStartGame = null;
			BtnSettingsGame = null;
			BtnCoinUpgrade = null;
			BtnAchievement = null;
			CoinUpgradePanel = null;
			AchievementPanel = null;
			BtnClose = null;
			AchievementItemPrefab = null;
			
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
