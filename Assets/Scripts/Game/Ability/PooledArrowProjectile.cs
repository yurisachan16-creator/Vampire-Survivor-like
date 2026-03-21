using System.Collections.Generic;
using UnityEngine;

namespace VampireSurvivorLike
{
    [DisallowMultipleComponent]
    public sealed class PooledArrowProjectile : MonoBehaviour, ObjectPoolSystem.IPoolable
    {
        private const float HomingSearchRadius = 8f;

        private Rigidbody2D _rb;
        private float _damage;
        private float _speed;
        private float _maxDistanceFromSpawn;
        private float _homingStrength;
        private Vector2 _direction;
        private Vector2 _spawnPosition;
        private int _hitCount;
        private int _maxHits;
        private Transform _homingTarget;
        private int _nextHomingQueryFrame;
        private readonly HashSet<int> _hitEnemyIds = new HashSet<int>(16);

        public void Configure(
            Vector2 direction,
            float speed,
            float damage,
            int maxHits,
            float maxDistanceFromSpawn,
            float homingStrength)
        {
            EnsureRefs();
            _direction = direction.sqrMagnitude > 0.001f ? direction.normalized : Vector2.up;
            _speed = Mathf.Max(1f, speed);
            _damage = damage;
            _maxHits = Mathf.Max(1, maxHits);
            _maxDistanceFromSpawn = Mathf.Max(6f, maxDistanceFromSpawn);
            _homingStrength = Mathf.Clamp01(homingStrength);
            _spawnPosition = transform.position;
            _hitCount = 0;
            _homingTarget = null;
            _nextHomingQueryFrame = Time.frameCount;
            _hitEnemyIds.Clear();
            _rb.velocity = _direction * _speed;
            transform.up = _direction;
        }

        private void Update()
        {
            if (_homingStrength > 0f)
            {
                if (Time.frameCount >= _nextHomingQueryFrame)
                {
                    RefreshHomingTarget();
                }

                if (_homingTarget)
                {
                    var toTarget = ((Vector2)_homingTarget.position - (Vector2)transform.position).normalized;
                    if (toTarget.sqrMagnitude > 0.0001f)
                    {
                        _direction = Vector2.Lerp(_direction, toTarget, _homingStrength * 10f * Time.deltaTime).normalized;
                    }
                }
            }

            _rb.velocity = _direction * _speed;
            transform.up = _direction;

            if (((Vector2)transform.position - _spawnPosition).sqrMagnitude > _maxDistanceFromSpawn * _maxDistanceFromSpawn)
            {
                ObjectPoolSystem.Despawn(gameObject);
            }
        }

        private void RefreshHomingTarget()
        {
            _nextHomingQueryFrame = Time.frameCount + (Application.isMobilePlatform ? 2 : 1);
            if (EnemySpatialIndex.TryGetNearestTarget(transform.position, HomingSearchRadius, out var nearest) && nearest)
            {
                _homingTarget = nearest;
            }
            else
            {
                _homingTarget = null;
            }
        }

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (!collider.TryGetComponent<HitHurtBox>(out var hitHurtBox)) return;
            if (!hitHurtBox.IsEnemyOwner) return;
            if (!hitHurtBox.TryGetEnemy(out var enemy)) return;

            var enemyId = hitHurtBox.Owner.GetInstanceID();
            if (_hitEnemyIds.Contains(enemyId)) return;
            _hitEnemyIds.Add(enemyId);

            DamageSystem.CalculateDamage(_damage, enemy, maxNormalDamage: 2, criticalDamageTimes: 5f);

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
            CombatLayerSettings.ApplyPlayerAttackLayer(gameObject);
            _damage = 1f;
            _speed = 10f;
            _maxDistanceFromSpawn = 20f;
            _maxHits = 1;
            _hitCount = 0;
            _homingStrength = 0f;
            _direction = Vector2.up;
            _spawnPosition = transform.position;
            _homingTarget = null;
            _nextHomingQueryFrame = 0;
            _hitEnemyIds.Clear();
        }

        public void OnDespawned()
        {
            if (_rb) _rb.velocity = Vector2.zero;
            _homingTarget = null;
            _hitEnemyIds.Clear();
        }
    }
}
