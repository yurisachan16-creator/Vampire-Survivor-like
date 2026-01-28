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
			yield return ResKit.InitAsync();
			#endif
			UIKit.OpenPanel<UIGameStartPanel>();
			yield break;
		}
    }
}
