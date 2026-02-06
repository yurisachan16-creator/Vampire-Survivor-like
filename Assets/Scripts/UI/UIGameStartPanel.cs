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

			var startLabel = BtnStartGame ? BtnStartGame.GetComponentInChildren<Text>(true) : null;
			if (startLabel) FontManager.Register(startLabel);
			var settingsLabel = BtnSettingsGame ? BtnSettingsGame.GetComponentInChildren<Text>(true) : null;
			if (settingsLabel) FontManager.Register(settingsLabel);
			var achievementLabel = BtnAchievement ? BtnAchievement.GetComponentInChildren<Text>(true) : null;
			if (achievementLabel) FontManager.Register(achievementLabel);
			var coinUpgradeLabel = BtnCoinUpgrade ? BtnCoinUpgrade.GetComponentInChildren<Text>(true) : null;
			if (coinUpgradeLabel) FontManager.Register(coinUpgradeLabel);

			System.Action refreshUiText = () =>
			{
				if (!LocalizationManager.IsReady) return;
				if (startLabel) startLabel.text = LocalizationManager.T("ui.start.start_game");
				if (settingsLabel) settingsLabel.text = LocalizationManager.T("ui.start.settings");
				if (achievementLabel) achievementLabel.text = LocalizationManager.T("ui.start.achievement");
				if (coinUpgradeLabel) coinUpgradeLabel.text = LocalizationManager.T("ui.start.coin_upgrade");
			};
			LocalizationManager.ReadyChanged.Register(() => refreshUiText()).UnRegisterWhenGameObjectDestroyed(gameObject);
			refreshUiText();

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
