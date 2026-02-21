using UnityEngine;
using QFramework;
using System.Collections.Generic;

namespace VampireSurvivorLike
{
    [DisallowMultipleComponent]
    public sealed class PooledAxeProjectile : MonoBehaviour, ObjectPoolSystem.IPoolable
    {
        private Rigidbody2D _rb;
        private float _damage;
        private float _despawnAbovePlayerDistance;
        private int _hitCount;
        private int _maxPierce;
        private bool _infinitePierce;
        private readonly HashSet<int> _hitEnemyIds = new HashSet<int>(16);

        public void Configure(Vector2 velocity, float damage, float despawnAbovePlayerDistance, int maxPierce, bool infinitePierce)
        {
            EnsureRefs();
            _rb.velocity = velocity;
            _damage = damage;
            _despawnAbovePlayerDistance = despawnAbovePlayerDistance;
            _maxPierce = Mathf.Max(1, maxPierce);
            _infinitePierce = infinitePierce;
            _hitCount = 0;
            _hitEnemyIds.Clear();
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
                var enemyId = hitHurtBox.Owner.GetInstanceID();
                if (_hitEnemyIds.Contains(enemyId)) return;
                _hitEnemyIds.Add(enemyId);

                DamageSystem.CalculateDamage(_damage, enemy);

                if (_infinitePierce) return;
                _hitCount++;
                if (_hitCount >= _maxPierce)
                {
                    ObjectPoolSystem.Despawn(gameObject);
                }
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
            _hitCount = 0;
            _maxPierce = 1;
            _infinitePierce = false;
            _hitEnemyIds.Clear();
        }

        public void OnDespawned()
        {
            if (_rb) _rb.velocity = Vector2.zero;
            _hitEnemyIds.Clear();
        }
    }
}
