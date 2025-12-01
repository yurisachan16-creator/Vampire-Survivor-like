using UnityEngine;
using UnityEngine.UI;
using QFramework;
using UnityEngine.SceneManagement;

namespace VampireSurvivorLike
{
	public class UIGameStartPanelData : UIPanelData
	{
	}
	public partial class UIGameStartPanel : UIPanel
	{
		protected override void OnInit(IUIData uiData = null)
		{
			mData = uiData as UIGameStartPanelData ?? new UIGameStartPanelData();
			// please add init code here
			Time.timeScale = 1.0f;

			BtnStartGame.onClick.AddListener(() =>
			{
				//开始游戏
				Global.ResetData();
				this.CloseSelf();
				SceneManager.LoadScene("Game");

			});
			
			BtnCoinPercentUpgrade.onClick.AddListener(() =>
			{
				CoinUpgradePanel.Show();
			});

			Global.CoinPercent.RegisterWithInitValue((Coin) =>
			{
				CoinText.text="金币"+Coin;
                if (Coin >= 5)
                {
                    BtnCoinUpgrade.Show();
					BtnExpPercentUpgrade.Show();
                }
				else
				{
					BtnCoinUpgrade.Hide();
					BtnExpPercentUpgrade.Hide();
				}
			}).UnRegisterWhenGameObjectDestroyed(gameObject);

			
			BtnCoinPercentUpgrade.onClick.AddListener(() =>
            {
                Global.CoinPercent.Value += 0.1f;
				Global.Coin.Value -= 5;
            });

			
			BtnExpPercentUpgrade.onClick.AddListener(() =>
			{
				Global.ExpPercent.Value += 0.1f;
				Global.Coin.Value -= 5;
			});

			BtnClose.onClick.AddListener(() =>
			{
				CoinUpgradePanel.Hide();
			});
		}
		
		protected override void OnOpen(IUIData uiData = null)
		{
		}
		
		protected override void OnShow()
		{
		}
		
		protected override void OnHide()
		{
		}
		
		protected override void OnClose()
		{
		}
	}
}
