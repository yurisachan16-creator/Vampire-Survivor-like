using UnityEngine;
using QFramework;

namespace VampireSurvivorLike
{
    public class Wine : PowerUp
    {
        private CircleCollider2D _collider;

        private void OnEnable()
        {
            PowerUpRegistry.ActiveWineCount++;
            var sr = GetComponent<SpriteRenderer>();
            LootGuideSystem.Current?.Register(this, LootGuideKind.Wine, sr ? sr.sprite : null);
            LootGuideSystem.Current?.TryPlayDropFeedback(transform.position, LootGuideKind.Wine);
        }

        private void OnDisable()
        {
            PowerUpRegistry.ActiveWineCount--;
            LootGuideSystem.Current?.Unregister(this);
        }

        private void OnTriggerEnter2D(Collider2D other)
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

            if (Global.HP.Value >= Global.MaxHP.Value)
            {
                FlyingToPalyer = false;
                return;
            }

            AudioKit.PlaySound("Health");
            Global.HP.Value = Mathf.Min(Global.MaxHP.Value, Global.HP.Value + 2);
            Global.RequestHPUIRefresh.Trigger();
            this.DestroyGameObjGracefully();
        }

        protected override Collider2D Collider2D => _collider ? _collider : (_collider = GetComponent<CircleCollider2D>());
    }
}
