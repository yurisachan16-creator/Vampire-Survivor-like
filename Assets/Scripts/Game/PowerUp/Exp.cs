using UnityEngine;
using QFramework;

namespace VampireSurvivorLike
{
	public partial class Exp : PowerUp
	{
		private void OnEnable()
		{
			PowerUpRegistry.RegisterExp(this);
			var sr = GetComponent<SpriteRenderer>();
			LootGuideSystem.Current?.Register(this, LootGuideKind.Exp, sr ? sr.sprite : null);
			LootGuideSystem.Current?.TryPlayDropFeedback(transform.position, LootGuideKind.Exp);
		}

		private void OnDisable()
		{
			PowerUpRegistry.UnregisterExp(this);
			LootGuideSystem.Current?.Unregister(this);
		}
        
        
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
