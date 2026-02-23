using System.Collections.Generic;
using UnityEngine;

namespace VampireSurvivorLike
{
    [DisallowMultipleComponent]
    public sealed class PooledHolyWaterProjectile : MonoBehaviour, ObjectPoolSystem.IPoolable
    {
        private static readonly List<Transform> HomingTargetBuffer = new List<Transform>(4);

        private Rigidbody2D _rb;
        private Vector2 _direction;
        private Vector2 _spawnPosition;
        private float _speed;
        private float _maxDistanceFromSpawn;
        private float _homingStrength;
        private float _lifeTimer;
        private bool _exploded;
        private GameObject _zoneTemplate;
        private float _zoneDamage;
        private float _zoneTickInterval;
        private float _zoneLifeTime;
        private float _zoneRadius;
        private float _zoneSlowMultiplier;
        private float _zoneSlowDuration;
        private bool _superMode;
        private readonly HashSet<int> _hitEnemyIds = new HashSet<int>(8);

        public void Configure(
            Vector2 direction,
            float speed,
            float maxDistanceFromSpawn,
            float homingStrength,
            GameObject zoneTemplate,
            float zoneDamage,
            float zoneTickInterval,
            float zoneLifeTime,
            float zoneRadius,
            float zoneSlowMultiplier,
            float zoneSlowDuration,
            bool superMode)
        {
            EnsureRefs();
            _direction = direction.sqrMagnitude > 0.001f ? direction.normalized : Vector2.right;
            _speed = Mathf.Max(1f, speed);
            _maxDistanceFromSpawn = Mathf.Max(4f, maxDistanceFromSpawn);
            _homingStrength = Mathf.Clamp01(homingStrength);
            _zoneTemplate = zoneTemplate;
            _zoneDamage = Mathf.Max(1f, zoneDamage);
            _zoneTickInterval = Mathf.Max(0.1f, zoneTickInterval);
            _zoneLifeTime = Mathf.Max(_zoneTickInterval, zoneLifeTime);
            _zoneRadius = Mathf.Max(0.5f, zoneRadius);
            _zoneSlowMultiplier = Mathf.Clamp(zoneSlowMultiplier, 0.25f, 1f);
            _zoneSlowDuration = Mathf.Max(0.05f, zoneSlowDuration);
            _superMode = superMode;
            _spawnPosition = transform.position;
            _lifeTimer = 0f;
            _exploded = false;
            _hitEnemyIds.Clear();
            _rb.velocity = _direction * _speed;
            transform.up = _direction;
        }

        private void Update()
        {
            if (_exploded) return;

            _lifeTimer += Time.deltaTime;
            if (_lifeTimer >= 2.4f)
            {
                Explode(transform.position);
                return;
            }

            if (_homingStrength > 0f)
            {
                EnemySpatialIndex.GetNearestTargets(transform.position, 10f, 1, HomingTargetBuffer);
                if (HomingTargetBuffer.Count > 0 && HomingTargetBuffer[0])
                {
                    var toTarget = ((Vector2)HomingTargetBuffer[0].position - (Vector2)transform.position).normalized;
                    _direction = Vector2.Lerp(_direction, toTarget, _homingStrength * 12f * Time.deltaTime).normalized;
                }
            }

            _rb.velocity = _direction * _speed;
            transform.up = _direction;

            if (((Vector2)transform.position - _spawnPosition).sqrMagnitude > _maxDistanceFromSpawn * _maxDistanceFromSpawn)
            {
                Explode(transform.position);
            }
        }

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (_exploded) return;
            var hitHurtBox = collider.GetComponent<HitHurtBox>();
            if (!hitHurtBox) return;
            if (!hitHurtBox.Owner || !hitHurtBox.Owner.CompareTag("Enemy")) return;

            var enemyId = hitHurtBox.Owner.GetInstanceID();
            if (_hitEnemyIds.Contains(enemyId)) return;
            _hitEnemyIds.Add(enemyId);

            var enemy = hitHurtBox.Owner.GetComponent<IEnemy>();
            if (enemy != null)
            {
                DamageSystem.CalculateDamage(_zoneDamage * 0.85f, enemy, maxNormalDamage: 1, criticalDamageTimes: _superMode ? 3.6f : 3f);
            }

            Explode(collider.bounds.center);
        }

        private void Explode(Vector2 position)
        {
            if (_exploded) return;
            _exploded = true;

            if (_zoneTemplate != null)
            {
                var zoneGo = ObjectPoolSystem.Spawn(_zoneTemplate, null, true);
                if (zoneGo != null)
                {
                    zoneGo.transform.position = position;
                    var zone = zoneGo.GetComponent<HolyWaterZone>();
                    if (!zone) zone = zoneGo.AddComponent<HolyWaterZone>();
                    zone.Configure(
                        _zoneDamage,
                        _zoneTickInterval,
                        _zoneLifeTime,
                        _zoneRadius,
                        _zoneSlowMultiplier,
                        _zoneSlowDuration,
                        false,
                        _superMode);
                }
            }

            ObjectPoolSystem.Despawn(gameObject);
        }

        private void EnsureRefs()
        {
            if (_rb) return;
            _rb = GetComponent<Rigidbody2D>();
        }

        public void OnSpawned()
        {
            EnsureRefs();
            _direction = Vector2.right;
            _spawnPosition = transform.position;
            _speed = 8f;
            _maxDistanceFromSpawn = 12f;
            _homingStrength = 0f;
            _zoneTemplate = null;
            _zoneDamage = 1f;
            _zoneTickInterval = 0.4f;
            _zoneLifeTime = 2f;
            _zoneRadius = 1f;
            _zoneSlowMultiplier = 1f;
            _zoneSlowDuration = 0.1f;
            _superMode = false;
            _lifeTimer = 0f;
            _exploded = false;
            _hitEnemyIds.Clear();
        }

        public void OnDespawned()
        {
            if (_rb) _rb.velocity = Vector2.zero;
            _hitEnemyIds.Clear();
            _exploded = false;
        }
    }
}
