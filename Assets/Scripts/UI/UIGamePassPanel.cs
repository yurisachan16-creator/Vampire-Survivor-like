using UnityEngine;
using UnityEngine.UI;
using QFramework;
using UnityEngine.SceneManagement;
using QAssetBundle;

namespace VampireSurvivorLike
{
	public class UIGamePassPanelData : UIPanelData
	{
	}
	public partial class UIGamePassPanel : UIPanel
	{
		protected override void OnInit(IUIData uiData = null)
		{
			mData = uiData as UIGamePassPanelData ?? new UIGamePassPanelData();
			//暂停游戏
			Time.timeScale = 0f;
			// please add init code here

			var titleText = transform.Find("Title")?.GetComponent<Text>();
			if (titleText) FontManager.Register(titleText);
			var backLabel = BtnBackToStart ? BtnBackToStart.GetComponentInChildren<Text>(true) : null;
			if (backLabel) FontManager.Register(backLabel);

			System.Action refreshUiText = () =>
			{
				if (!LocalizationManager.IsReady) return;
				if (titleText) titleText.text = LocalizationManager.T("ui.gamepass.title");
				if (backLabel) backLabel.text = LocalizationManager.T("ui.settings.return_main_menu");
			};
			LocalizationManager.ReadyChanged.Register(() => refreshUiText()).UnRegisterWhenGameObjectDestroyed(gameObject);
			refreshUiText();

			//移除按空格键重玩功能
			// ActionKit.OnUpdate.Register(()=>
            // {
            //     if(Input.GetKeyDown(KeyCode.Space))
			// 	{
			// 		//重置数据
			// 		Global.ResetData();
			// 		//关闭当前面板
			// 		this.CloseSelf();
			// 		//重新加载场景
			// 		SceneManager.LoadScene("Game");
			// 	}
            // }).UnRegisterWhenGameObjectDestroyed(gameObject);

			BtnBackToStart.onClick.AddListener(() =>
            {
				//播放音效
				AudioKit.PlaySound(Sfx.BUTTONCLICK);
                this.CloseSelf();
				SceneManager.LoadScene("GameStart");
            });

			//通关音效
			AudioKit.PlaySound("Retro Event Acute 11");
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
			//恢复游戏
			Time.timeScale = 1f;
        }
	}
}
