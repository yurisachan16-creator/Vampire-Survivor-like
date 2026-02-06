using UnityEngine;
using UnityEngine.UI;
using QFramework;
using UnityEngine.SceneManagement;
using QAssetBundle;

namespace VampireSurvivorLike
{
	public class UIGameOverPanelData : UIPanelData
	{
	}
	public partial class UIGameOverPanel : UIPanel
	{
		protected override void OnInit(IUIData uiData = null)
		{
			mData = uiData as UIGameOverPanelData ?? new UIGameOverPanelData();
			// please add init code here

			var titleText = transform.Find("Title")?.GetComponent<Text>();
			if (titleText) FontManager.Register(titleText);
			var backLabel = BtnBackToStart ? BtnBackToStart.GetComponentInChildren<Text>(true) : null;
			if (backLabel) FontManager.Register(backLabel);

			System.Action refreshUiText = () =>
			{
				if (!LocalizationManager.IsReady) return;
				if (titleText) titleText.text = LocalizationManager.T("ui.gameover.title");
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
				//恢复时间缩放
				Time.timeScale = 1f;
				//重置游戏数据（保留金币）
				Global.ResetData();
				this.CloseSelf();
				SceneManager.LoadScene("GameStart");
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
