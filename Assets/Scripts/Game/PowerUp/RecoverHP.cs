using UnityEngine;
using QFramework;

namespace VampireSurvivorLike
{
	public partial class RecoverHP : PowerUp
	{
		void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<CollectableAera>())
            {
				if(Global.HP.Value==Global.MaxHP.Value)
                {
                    
                }
                else
                {
                    FlyingToPalyer = true;
                }
            }
        }

        protected override void Execute()
        {
            AudioKit.PlaySound("Health");
            Global.HP.Value += 1;
            this.DestroyGameObjGracefully();
        }

        protected override Collider2D Collider2D => SelfCircleCollider2D;
	}
}
