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
            if (Global.IsGameOver.Value)
            {
                this.DestroyGameObjGracefully();
                return;
            }

            AudioKit.PlaySound("Health");
            Global.HP.Value += 1;
            Global.RequestHPUIRefresh.Trigger();
            this.DestroyGameObjGracefully();
        }

        protected override Collider2D Collider2D => SelfCircleCollider2D;
	}
}
