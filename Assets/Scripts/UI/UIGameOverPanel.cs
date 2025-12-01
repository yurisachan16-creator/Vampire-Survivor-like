using UnityEngine;
using UnityEngine.UI;
using QFramework;
using UnityEditor.SearchService;
using UnityEngine.SceneManagement;

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
