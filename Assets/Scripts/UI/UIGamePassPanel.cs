using UnityEngine;
using UnityEngine.UI;
using QFramework;
using UnityEngine.SceneManagement;

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

			ActionKit.OnUpdate.Register(()=>
            {
                if(Input.GetKeyDown(KeyCode.Space))
				{
					//重置数据
					Global.ResetData();
					//关闭当前面板
					this.CloseSelf();
					//重新加载场景
					SceneManager.LoadScene("Game");
				}
            }).UnRegisterWhenGameObjectDestroyed(gameObject);

			BtnBackToStart.onClick.AddListener(() =>
            {
                this.CloseSelf();
				SceneManager.LoadScene("GameStart");
            });

			//通关音效
			AudioKit.PlaySound("");
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
