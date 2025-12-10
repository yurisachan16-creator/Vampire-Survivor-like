using UnityEngine;
using QFramework;

namespace VampireSurvivorLike
{
	public partial class Exp : GameplayObject
	{
        
        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<CollectableAera>())
            {
                AudioKit.PlaySound("Exp");
                Global.Exp.Value += 1;
				this.DestroyGameObjGracefully();
            }
        }

        protected override Collider2D Collider2D => SelfCircleCollider2D;
    }
}
