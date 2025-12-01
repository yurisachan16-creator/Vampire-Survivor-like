using UnityEngine;
using QFramework;

namespace VampireSurvivorLike
{
	public partial class GameUIController : ViewController
	{
		void Start()
		{
			// Code Here

			UIKit.OpenPanel<UIGamePanel>();
		}

        void OnDestroy()
        {
			UIKit.ClosePanel<UIGamePanel>();
        }
    }
}
