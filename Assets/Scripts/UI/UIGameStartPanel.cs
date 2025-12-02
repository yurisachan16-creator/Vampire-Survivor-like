using UnityEngine;
using UnityEngine.UI;
using QFramework;
using UnityEngine.SceneManagement;

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
				//开始游戏
				Global.ResetData();
				this.CloseSelf();
				SceneManager.LoadScene("Game");

			});
			
			

			this.GetSystem<CoinUpgradeSystem>().Say();
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

		public IArchitecture GetArchitecture()
        {
            return Global.Interface;
        }
	}
}
