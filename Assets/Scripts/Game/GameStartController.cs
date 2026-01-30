using UnityEngine;
using QFramework;
using System.Collections;

namespace VampireSurvivorLike
{
	public partial class GameStartController : ViewController
	{
        
		private IEnumerator Start() 
		{
			#if UNITY_WEBGL && !UNITY_EDITOR
			// WebGL 实际运行时：初始化 ResKit 并预加载所有资源
			yield return ResKit.InitAsync();
			yield return WebGLPreloader.PreloadAllAssets();
			#endif
			
			// 预加载完成后，OpenPanel 可以从缓存中同步获取资源
			UIKit.OpenPanel<UIGameStartPanel>();
			yield break;
		}

		private void Update()
		{
			// ESC 键或 Settings 按钮打开/关闭设置面板
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				ToggleSettingsPanel(false);
			}
		}

		/// <summary>
		/// 切换设置面板显示状态
		/// </summary>
		/// <param name="isFromGame">是否从游戏中打开（需要暂停）</param>
		public static void ToggleSettingsPanel(bool isFromGame)
		{
			var settingsPanel = UIKit.GetPanel<UIGameSettingsPanel>();
			if (settingsPanel != null && settingsPanel.State == PanelState.Opening)
			{
				UIKit.ClosePanel<UIGameSettingsPanel>();
			}
			else
			{
				UIKit.OpenPanel<UIGameSettingsPanel>(new UIGameSettingsPanelData { IsFromGame = isFromGame });
			}
		}
    }
}
