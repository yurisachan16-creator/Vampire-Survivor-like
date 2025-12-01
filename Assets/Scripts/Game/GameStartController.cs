using UnityEngine;
using QFramework;

namespace VampireSurvivorLike
{
	public partial class GameStartController : ViewController
	{
        void Awake()
        {
            ResKit.Init();
        }

		private void Start() 
		{
			UIKit.OpenPanel<UIGameStartPanel>();
		}
    }
}
