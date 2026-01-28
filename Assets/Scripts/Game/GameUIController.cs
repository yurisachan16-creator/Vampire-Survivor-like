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
			yield return ResKit.InitAsync();
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
