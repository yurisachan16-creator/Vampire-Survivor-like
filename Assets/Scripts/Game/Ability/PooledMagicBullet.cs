using System.Collections.Generic;
using UnityEngine;

namespace VampireSurvivorLike
{
    [DisallowMultipleComponent]
    public sealed class PooledMagicBullet : MonoBehaviour, ObjectPoolSystem.IPoolable
    {
        private Rigidbody2D _rb;
        private float _damage;
        private float _maxDistanceFromPlayer;
        private float _knockbackForce;
        private int _hitCount;
        private int _maxHits;
        private bool _superMode;
        private readonly HashSet<int> _hitEnemyIds = new HashSet<int>(16);

        public void Configure(Vector2 direction, float speed, float damage, bool superMode, int maxHits, float maxDistanceFromPlayer, float knockbackForce)
        {
            EnsureRefs();
            _rb.velocity = direction.normalized * speed;
            transform.up = direction.normalized;
            _damage = damage;
            _superMode = superMode;
            _maxHits = Mathf.Max(1, maxHits);
            _maxDistanceFromPlayer = Mathf.Max(6f, maxDistanceFromPlayer);
            _knockbackForce = Mathf.Max(0f, knockbackForce);
            _hitCount = 0;
            _hitEnemyIds.Clear();
        }

        private void Update()
        {
            if (!Player.Default) return;
            if (((Vector2)Player.Default.transform.position - (Vector2)transform.position).sqrMagnitude > _maxDistanceFromPlayer * _maxDistanceFromPlayer)
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
            if (enemy == null) return;

            var enemyId = hitHurtBox.Owner.GetInstanceID();
            if (_hitEnemyIds.Contains(enemyId)) return;
            _hitEnemyIds.Add(enemyId);

            DamageSystem.CalculateDamage(_damage, enemy, maxNormalDamage: 2, criticalDamageTimes: _superMode ? 6f : 5f);

            if (_knockbackForce > 0f && collider.attachedRigidbody)
            {
                var dir = ((Vector2)collider.transform.position - (Vector2)transform.position).normalized;
                if (dir.sqrMagnitude > 0.001f)
                {
                    collider.attachedRigidbody.velocity = dir * _knockbackForce;
                }
            }

            _hitCount++;
            if (_hitCount >= _maxHits)
            {
                ObjectPoolSystem.Despawn(gameObject);
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
            _damage = 1f;
            _maxDistanceFromPlayer = 24f;
            _knockbackForce = 0f;
            _hitCount = 0;
            _maxHits = 1;
            _superMode = false;
            _hitEnemyIds.Clear();
        }

        public void OnDespawned()
        {
            if (_rb) _rb.velocity = Vector2.zero;
            _hitEnemyIds.Clear();
        }
    }
}
