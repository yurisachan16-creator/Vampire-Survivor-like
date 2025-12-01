using UnityEngine;
using QFramework;

namespace VampireSurvivorLike
{
	public partial class GameStartController : ViewController
	{
        

		private void Start() 
		{
			UIKit.OpenPanel<UIGameStartPanel>();
		}
    }
}
