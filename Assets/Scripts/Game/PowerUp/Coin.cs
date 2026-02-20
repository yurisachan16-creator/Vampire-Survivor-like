using UnityEngine;
using QFramework;

namespace VampireSurvivorLike
{
	public partial class Coin : PowerUp
	{
		private void OnEnable()
		{
			PowerUpRegistry.RegisterCoin(this);
			var sr = GetComponent<SpriteRenderer>();
			LootGuideSystem.Current?.Register(this, LootGuideKind.Coin, sr ? sr.sprite : null);
		}

		private void OnDisable()
		{
			PowerUpRegistry.UnregisterCoin(this);
			LootGuideSystem.Current?.Unregister(this);
		}

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
