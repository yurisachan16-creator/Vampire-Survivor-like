using UnityEngine;
using QFramework;

namespace VampireSurvivorLike
{
	public partial class Coin : ViewController
	{
		void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<CollectableAera>())
            {
                Global.Coin.Value += 1;
				this.DestroyGameObjGracefully();
            }
        }
	}
}
