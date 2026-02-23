using System.Collections.Generic;
using UnityEngine;

namespace VampireSurvivorLike
{
    [DisallowMultipleComponent]
    public sealed class PooledBoomerangProjectile : MonoBehaviour, ObjectPoolSystem.IPoolable
    {
        private enum FlightPhase
        {
            Outbound,
            Returning
        }

        private static readonly List<Transform> TargetBuffer = new List<Transform>(8);

        private Rigidbody2D _rb;
        private Vector2 _originPosition;
        private Vector2 _outboundDirection;
        private float _speed;
        private float _outboundDistance;
        private float _damage;
        private int _maxHitsPerSegment;
        private int _returnCount;
        private bool _superMode;
        private float _playerCatchRadius;
        private int _completedReturns;
        private int _segmentHitCount;
        private float _spinSpeed;
        private FlightPhase _phase;
        private readonly HashSet<int> _segmentHitEnemyIds = new HashSet<int>(16);

        public void Configure(
            Vector2 direction,
            float speed,
            float outboundDistance,
            float damage,
            int maxHitsPerSegment,
            int returnCount,
            bool superMode)
        {
            EnsureRefs();

            _outboundDirection = direction.sqrMagnitude > 0.001f ? direction.normalized : Vector2.right;
            _speed = Mathf.Max(1f, speed);
            _outboundDistance = Mathf.Max(2f, outboundDistance);
            _damage = Mathf.Max(1f, damage);
            _maxHitsPerSegment = Mathf.Max(1, maxHitsPerSegment);
            _returnCount = Mathf.Max(1, returnCount);
            _superMode = superMode;
            _playerCatchRadius = superMode ? 0.75f : 0.6f;
            _completedReturns = 0;

            BeginOutbound(_outboundDirection);
        }

        private void Update()
        {
            if (!Player.Default)
            {
                ObjectPoolSystem.Despawn(gameObject);
                return;
            }

            transform.Rotate(0f, 0f, _spinSpeed * Time.deltaTime);

            if (_phase == FlightPhase.Outbound)
            {
                _rb.velocity = _outboundDirection * _speed;
                if (((Vector2)transform.position - _originPosition).sqrMagnitude >= _outboundDistance * _outboundDistance)
                {
                    BeginReturning();
                }
            }
            else
            {
                var toPlayer = (Vector2)Player.Default.transform.position - (Vector2)transform.position;
                if (toPlayer.sqrMagnitude <= _playerCatchRadius * _playerCatchRadius)
                {
                    OnCaughtByPlayer();
                    return;
                }

                var returnSpeed = _speed * (_superMode ? 1.2f : 1.05f);
                _rb.velocity = toPlayer.normalized * returnSpeed;
            }
        }

        private void OnTriggerEnter2D(Collider2D collider)
        {
            var hitHurtBox = collider.GetComponent<HitHurtBox>();
            if (!hitHurtBox) return;
            if (!hitHurtBox.Owner || !hitHurtBox.Owner.CompareTag("Enemy")) return;
            if (_segmentHitCount >= _maxHitsPerSegment) return;

            var enemy = hitHurtBox.Owner.GetComponent<IEnemy>();
            if (enemy == null) return;

            var enemyId = hitHurtBox.Owner.GetInstanceID();
            if (_segmentHitEnemyIds.Contains(enemyId)) return;
            _segmentHitEnemyIds.Add(enemyId);

            DamageSystem.CalculateDamage(_damage, enemy, maxNormalDamage: 1, criticalDamageTimes: _superMode ? 5.5f : 4f);

            _segmentHitCount++;
            if (_segmentHitCount >= _maxHitsPerSegment && _phase == FlightPhase.Outbound)
            {
                BeginReturning();
            }
        }

        private void OnCaughtByPlayer()
        {
            _completedReturns++;
            if (_completedReturns >= _returnCount)
            {
                ObjectPoolSystem.Despawn(gameObject);
                return;
            }

            BeginOutbound(GetNextOutboundDirection());
        }

        private Vector2 GetNextOutboundDirection()
        {
            if (Player.Default)
            {
                EnemySpatialIndex.GetNearestTargets(Player.Default.transform.position, 20f, 1, TargetBuffer);
                if (TargetBuffer.Count > 0 && TargetBuffer[0])
                {
                    var targetDir = ((Vector2)TargetBuffer[0].position - (Vector2)Player.Default.transform.position).normalized;
                    if (targetDir.sqrMagnitude > 0.001f)
                    {
                        return targetDir;
                    }
                }
            }

            var fallback = Quaternion.Euler(0f, 0f, Random.Range(-45f, 45f)) * _outboundDirection;
            return ((Vector2)fallback).normalized;
        }

        private void BeginOutbound(Vector2 direction)
        {
            _phase = FlightPhase.Outbound;
            _outboundDirection = direction.sqrMagnitude > 0.001f ? direction.normalized : Vector2.right;
            _originPosition = transform.position;
            _segmentHitCount = 0;
            _segmentHitEnemyIds.Clear();
        }

        private void BeginReturning()
        {
            _phase = FlightPhase.Returning;
            _segmentHitCount = 0;
            _segmentHitEnemyIds.Clear();
        }

        private void EnsureRefs()
        {
            if (_rb) return;
            _rb = GetComponent<Rigidbody2D>();
        }

        public void OnSpawned()
        {
            EnsureRefs();
            _speed = 10f;
            _outboundDistance = 8f;
            _damage = 1f;
            _maxHitsPerSegment = 1;
            _returnCount = 1;
            _superMode = false;
            _playerCatchRadius = 0.6f;
            _completedReturns = 0;
            _segmentHitCount = 0;
            _phase = FlightPhase.Outbound;
            _spinSpeed = Random.Range(600f, 900f);
            _segmentHitEnemyIds.Clear();
            _originPosition = transform.position;
            _outboundDirection = Vector2.right;
        }

        public void OnDespawned()
        {
            if (_rb) _rb.velocity = Vector2.zero;
            _segmentHitEnemyIds.Clear();
            _segmentHitCount = 0;
            _completedReturns = 0;
        }
    }
}
