using UnityEngine;
using QFramework;
using QAssetBundle;

namespace VampireSurvivorLike
{
	// Generate Id:9b7c6d24-da13-4a2f-afcf-a8f69033dcbe
	public partial class UIGameLocalLeaderboardPanel
	{
		public const string Name = "UIGameLocalLeaderboardPanel";
		
		[SerializeField]
		public UnityEngine.UI.Image RankingTemplate;
		[SerializeField]
		public UnityEngine.UI.Button BtnClose;
		[SerializeField]
		public UnityEngine.UI.Button BtnReturnToMainMenu;
		[SerializeField]
		public UnityEngine.UI.Button BtnQuit;
		
		private UIGameLocalLeaderboardPanelData mPrivateData = null;
		
		protected override void ClearUIComponents()
		{
			RankingTemplate = null;
			BtnClose = null;
			BtnReturnToMainMenu = null;
			BtnQuit = null;
			mData = null;
		}
		
		public UIGameLocalLeaderboardPanelData Data
		{
			get
			{
				return mData;
			}
		}
		
		UIGameLocalLeaderboardPanelData mData
		{
			get
			{
				return mPrivateData ?? (mPrivateData = new UIGameLocalLeaderboardPanelData());
			}
			set
			{
				mUIData = value;
				mPrivateData = value;
			}
		}
	}
}
