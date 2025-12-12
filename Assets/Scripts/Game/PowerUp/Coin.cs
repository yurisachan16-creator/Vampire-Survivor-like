using UnityEngine;
using QFramework;

namespace VampireSurvivorLike
{
	public partial class Coin : PowerUp
	{

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<CollectableAera>())
            {
                FlyingToPalyer = true;
            }
        }

        

        protected override Collider2D Collider2D => SelfCircleCollider2D;

        protected override void Execute()
        {
            AudioKit.PlaySound("Coin");
            Global.Coin.Value += 1;
            this.DestroyGameObjGracefully();
        }

        
	}
}
