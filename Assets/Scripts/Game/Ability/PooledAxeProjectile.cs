using UnityEngine;
using QFramework;

namespace VampireSurvivorLike
{
    [DisallowMultipleComponent]
    public sealed class PooledAxeProjectile : MonoBehaviour, ObjectPoolSystem.IPoolable
    {
        private Rigidbody2D _rb;
        private float _damage;
        private float _despawnAbovePlayerDistance;

        public void Configure(Vector2 velocity, float damage, float despawnAbovePlayerDistance)
        {
            EnsureRefs();
            _rb.velocity = velocity;
            _damage = damage;
            _despawnAbovePlayerDistance = despawnAbovePlayerDistance;
        }

        private void Update()
        {
            if (!Player.Default) return;
            if (Player.Default.Position().y - transform.position.y > _despawnAbovePlayerDistance)
            {
                ObjectPoolSystem.Despawn(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D collider)
        {
            var hitHurtBox = collider.GetComponent<HitHurtBox>();
            if (!hitHurtBox) return;
            if (!hitHurtBox.Owner || !hitHurtBox.Owner.CompareTag("Enemy")) return;

            var enemy = hitHurtBox.Owner.GetComponent<IEnemy>();
            if (enemy != null)
            {
                enemy.Hurt(_damage);
            }
        }

        private void EnsureRefs()
        {
            if (_rb) return;
            _rb = GetComponent<Rigidbody2D>();
        }

        public void OnSpawned()
        {
            EnsureRefs();
            _damage = 2f;
            _despawnAbovePlayerDistance = 15f;
        }

        public void OnDespawned()
        {
            if (_rb) _rb.velocity = Vector2.zero;
        }
    }
}
