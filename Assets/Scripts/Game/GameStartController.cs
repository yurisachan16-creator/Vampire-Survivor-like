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
			// WebGL 平台：初始化 ResKit 并预加载所有资源
			yield return ResKit.InitAsync();
			yield return WebGLPreloader.PreloadAllAssets();
			#endif
			
			// 预加载完成后，OpenPanel 可以从缓存中同步获取资源
			UIKit.OpenPanel<UIGameStartPanel>();
			yield break;
		}
    }
}
