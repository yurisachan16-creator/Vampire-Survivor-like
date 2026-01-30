using UnityEngine;
using QFramework;
using System.Collections;

namespace VampireSurvivorLike
{
	public partial class GameUIController : ViewController
	{
		private IEnumerator Start()
		{
			#if UNITY_WEBGL && !UNITY_EDITOR
			// WebGL 平台：如果还没预加载，先进行预加载
			if (!WebGLPreloader.IsPreloaded)
			{
				yield return ResKit.InitAsync();
				yield return WebGLPreloader.PreloadAllAssets();
			}
			#endif
			
			UIKit.OpenPanel<UIGamePanel>();
			yield break;
		}

        void OnDestroy()
        {
			UIKit.ClosePanel<UIGamePanel>();
        }
    }
}
