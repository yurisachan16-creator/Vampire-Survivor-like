using UnityEngine;
using QFramework;

namespace VampireSurvivorLike
{
	public partial class Exp : PowerUp
	{
        
        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<CollectableAera>())
            {
                FlyingToPalyer = true;
                
            }
        }

        
        

        //执行方法
        protected override void Execute()
        {
            AudioKit.PlaySound("Exp");
            Global.Exp.Value += 1;
            this.DestroyGameObjGracefully();
        }

        protected override Collider2D Collider2D => SelfCircleCollider2D;
    }
}
