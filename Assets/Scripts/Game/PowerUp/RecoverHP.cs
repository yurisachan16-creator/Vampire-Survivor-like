using UnityEngine;
using QFramework;

namespace VampireSurvivorLike
{
	public partial class RecoverHP : PowerUp
	{
		private void OnEnable()
		{
			PowerUpRegistry.ActiveRecoverHPCount++;
			var sr = GetComponent<SpriteRenderer>();
			LootGuideSystem.Current?.Register(this, LootGuideKind.RecoverHP, sr ? sr.sprite : null);
			LootGuideSystem.Current?.TryPlayDropFeedback(transform.position, LootGuideKind.RecoverHP);
		}

		private void OnDisable()
		{
			PowerUpRegistry.ActiveRecoverHPCount--;
			LootGuideSystem.Current?.Unregister(this);
		}

		void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.GetComponent<CollectableAera>()) return;
            if (Global.IsGameOver.Value) return;
            if (Global.HP.Value >= Global.MaxHP.Value) return;

            FlyingToPalyer = true;
        }

        protected override void Execute()
        {
            if (Global.IsGameOver.Value)
            {
                this.DestroyGameObjGracefully();
                return;
            }

            // 并发拾取时再次判定，避免满血后仍被第二个回血道具继续加血
            if (Global.HP.Value >= Global.MaxHP.Value)
            {
                FlyingToPalyer = false;
                return;
            }

            AudioKit.PlaySound("Health");
            Global.HP.Value = Mathf.Min(Global.MaxHP.Value, Global.HP.Value + 1);
            Global.RequestHPUIRefresh.Trigger();
            this.DestroyGameObjGracefully();
        }

        protected override Collider2D Collider2D => SelfCircleCollider2D;
	}
}
