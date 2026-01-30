using UnityEngine;
using QFramework;
using System.Collections;

namespace VampireSurvivorLike
{
	public partial class GameUIController : ViewController
	{
		private IEnumerator Start()
		{
			#if UNITY_WEBGL
			// WebGL 平台（包括编辑器）：如果还没预加载，先进行预加载
			if (!WebGLPreloader.IsPreloaded)
			{
				yield return ResKit.InitAsync();
				yield return WebGLPreloader.PreloadAllAssets();
			}
			#endif
			
			UIKit.OpenPanel<UIGamePanel>();
			yield break;
		}

		private void Update()
		{
			// ESC 键打开/关闭设置面板（游戏中需要暂停）
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				ToggleSettingsPanel();
			}
		}

		/// <summary>
		/// 切换设置面板显示状态（游戏中打开会暂停游戏）
		/// </summary>
		private void ToggleSettingsPanel()
		{
			var settingsPanel = UIKit.GetPanel<UIGameSettingsPanel>();
			if (settingsPanel != null && settingsPanel.State == PanelState.Opening)
			{
				UIKit.ClosePanel<UIGameSettingsPanel>();
			}
			else
			{
				UIKit.OpenPanel<UIGameSettingsPanel>(new UIGameSettingsPanelData { IsFromGame = true });
			}
		}

        void OnDestroy()
        {
			UIKit.ClosePanel<UIGamePanel>();
        }
    }
}
