using UnityEngine;
using QFramework;

namespace VampireSurvivorLike
{
    public class Cherry : PowerUp
    {
        private CircleCollider2D _collider;

        private void OnEnable()
        {
            PowerUpRegistry.ActiveCherryCount++;
            var sr = GetComponent<SpriteRenderer>();
            LootGuideSystem.Current?.Register(this, LootGuideKind.Cherry, sr ? sr.sprite : null);
            LootGuideSystem.Current?.TryPlayDropFeedback(transform.position, LootGuideKind.Cherry);
        }

        private void OnDisable()
        {
            PowerUpRegistry.ActiveCherryCount = Mathf.Max(0, PowerUpRegistry.ActiveCherryCount - 1);
            LootGuideSystem.Current?.Unregister(this);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.GetComponent<CollectableAera>()) return;
            if (Global.IsGameOver.Value) return;

            FlyingToPalyer = true;
        }

        protected override void Execute()
        {
            if (Global.IsGameOver.Value)
            {
                this.DestroyGameObjGracefully();
                return;
            }

            var damage = CalculateCherryDamage();
            foreach (var enemyObj in GameObject.FindGameObjectsWithTag("Enemy"))
            {
                if (!enemyObj || !enemyObj.activeSelf) continue;
                var enemy = enemyObj.GetComponent<IEnemy>();
                if (enemy == null) continue;
                DamageSystem.CalculateDamage(damage, enemy, maxNormalDamage: 1, criticalDamageTimes: 2.5f);
            }

            AudioKit.PlaySound("BombExplosion");
            CameraController.ShakeCamera();
            this.DestroyGameObjGracefully();
        }

        private static float CalculateCherryDamage()
        {
            var highest = 4f;

            if (Global.SimpleSwordUnlocked.Value) highest = Mathf.Max(highest, Global.SimpleAbilityDamage.Value);
            if (Global.SimpleKnifeUnlocked.Value) highest = Mathf.Max(highest, Global.SimpleKnifeDamage.Value);
            if (Global.RotateSwordUnlocked.Value) highest = Mathf.Max(highest, Global.RotateSwordDamage.Value);
            if (Global.BasketBallUnlocked.Value) highest = Mathf.Max(highest, Global.BasketBallDamage.Value);
            if (Global.SimpleAxeUnlocked.Value) highest = Mathf.Max(highest, Global.SimpleAxeDamage.Value);
            if (Global.MagicWandUnlocked.Value) highest = Mathf.Max(highest, Global.MagicWandDamage.Value);
            if (Global.SimpleBowUnlocked.Value) highest = Mathf.Max(highest, Global.SimpleBowDamage.Value);
            if (Global.BoomerangUnlocked.Value) highest = Mathf.Max(highest, Global.BoomerangDamage.Value);
            if (Global.HolyWaterUnlocked.Value) highest = Mathf.Max(highest, Global.HolyWaterDamage.Value);

            return Mathf.Clamp(highest * 2f, 6f, 40f);
        }

        protected override Collider2D Collider2D => _collider ? _collider : (_collider = GetComponent<CircleCollider2D>());
    }
}
