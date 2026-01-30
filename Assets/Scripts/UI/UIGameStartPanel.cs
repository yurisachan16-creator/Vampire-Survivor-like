using UnityEngine;
using UnityEngine.UI;
using QFramework;
using UnityEngine.SceneManagement;
using QAssetBundle;

namespace VampireSurvivorLike
{
    public class UIGameStartPanelData : UIPanelData
    {
        
    }
    public partial class UIGameStartPanel : UIPanel,IController
	{
		protected override void OnInit(IUIData uiData = null)
		{
			mData = uiData as UIGameStartPanelData ?? new UIGameStartPanelData();
			// please add init code here
			Time.timeScale = 1.0f;

			BtnStartGame.onClick.AddListener(() =>
			{
				//播放音效
				AudioKit.PlaySound(Sfx.BUTTONCLICK);
				//开始游戏
				Global.ResetData();
				this.CloseSelf();
				SceneManager.LoadScene("Game");

			});

			BtnCoinUpgrade.onClick.AddListener(() =>
			{
				//播放音效
				AudioKit.PlaySound(Sfx.BUTTONCLICK);
				//打开金币升级面板
				CoinUpgradePanel.Show();
			});

			BtnAchievement.onClick.AddListener(() =>
			{
				//播放音效
				AudioKit.PlaySound(Sfx.BUTTONCLICK);
				//打开成就面板
				AchievementPanel.Show();
			});

			BtnSettingsGame.onClick.AddListener(() =>
			{
				//播放音效
				AudioKit.PlaySound(Sfx.BUTTONCLICK);
				//打开设置面板
				UIKit.OpenPanel<UIGameSettingsPanel>(new UIGameSettingsPanelData { IsFromGame = false });
			});

			this.GetSystem<CoinUpgradeSystem>().Say();
		}
	
		private void Update()
		{
			// ESC 键打开设置面板
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				AudioKit.PlaySound(Sfx.BUTTONCLICK);
				UIKit.OpenPanel<UIGameSettingsPanel>(new UIGameSettingsPanelData { IsFromGame = false });
			}
		}
			
		protected override void OnHide()
		{
		}
		
		protected override void OnClose()
		{
		}

		public IArchitecture GetArchitecture()
		{
			return Global.Interface;
		}
	}
}
