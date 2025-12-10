using UnityEngine;
using QFramework;

namespace VampireSurvivorLike
{
	public partial class Coin : GameplayObject
	{
		void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<CollectableAera>())
            {
                AudioKit.PlaySound("Coin");
                Global.Coin.Value += 1;
				this.DestroyGameObjGracefully();
            }
        }

        protected override Collider2D Collider2D => SelfCircleCollider2D;
	}
}
