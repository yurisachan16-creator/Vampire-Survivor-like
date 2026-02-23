using System.Collections.Generic;
using UnityEngine;

namespace VampireSurvivorLike
{
    [DisallowMultipleComponent]
    public sealed class HolyWaterZone : MonoBehaviour, ObjectPoolSystem.IPoolable
    {
        private static readonly List<Transform> TargetsBuffer = new List<Transform>(256);

        private float _damagePerTick;
        private float _tickInterval;
        private float _lifeTime;
        private float _radius;
        private float _slowMultiplier;
        private float _slowDuration;
        private bool _followPlayer;
        private bool _superMode;
        private float _tickTimer;
        private float _lifeTimer;

        public void Configure(
            float damagePerTick,
            float tickInterval,
            float lifeTime,
            float radius,
            float slowMultiplier,
            float slowDuration,
            bool followPlayer,
            bool superMode)
        {
            _damagePerTick = Mathf.Max(1f, damagePerTick);
            _tickInterval = Mathf.Max(0.12f, tickInterval);
            _lifeTime = Mathf.Max(_tickInterval, lifeTime);
            _radius = Mathf.Max(0.5f, radius);
            _slowMultiplier = Mathf.Clamp(slowMultiplier, 0.25f, 1f);
            _slowDuration = Mathf.Max(0.05f, slowDuration);
            _followPlayer = followPlayer;
            _superMode = superMode;
            _tickTimer = 0f;
            _lifeTimer = 0f;

            transform.localScale = Vector3.one * (_radius * 1.65f);
        }

        private void Update()
        {
            if (_followPlayer && Player.Default)
            {
                transform.position = Player.Default.transform.position;
            }

            _lifeTimer += Time.deltaTime;
            if (_lifeTimer >= _lifeTime)
            {
                ObjectPoolSystem.Despawn(gameObject);
                return;
            }

            _tickTimer += Time.deltaTime;
            if (_tickTimer < _tickInterval) return;

            _tickTimer -= _tickInterval;
            ApplyTickDamage();
        }

        private void ApplyTickDamage()
        {
            EnemySpatialIndex.GetNearestTargets(transform.position, _radius, 160, TargetsBuffer);
            if (TargetsBuffer.Count == 0) return;

            var center = (Vector2)transform.position;
            var radiusSqr = _radius * _radius;
            for (var i = 0; i < TargetsBuffer.Count; i++)
            {
                var target = TargetsBuffer[i];
                if (!target) continue;
                if (((Vector2)target.position - center).sqrMagnitude > radiusSqr) continue;

                var enemy = target.GetComponent<IEnemy>();
                if (enemy == null) continue;

                DamageSystem.CalculateDamage(_damagePerTick, enemy, maxNormalDamage: _superMode ? 2 : 1, criticalDamageTimes: _superMode ? 4f : 3f);
                enemy.ApplySlow(_slowMultiplier, _slowDuration);
            }
        }

        public void OnSpawned()
        {
            _damagePerTick = 1f;
            _tickInterval = 0.5f;
            _lifeTime = 2f;
            _radius = 1f;
            _slowMultiplier = 1f;
            _slowDuration = 0.1f;
            _followPlayer = false;
            _superMode = false;
            _tickTimer = 0f;
            _lifeTimer = 0f;
            transform.localScale = Vector3.one;
        }

        public void OnDespawned()
        {
            _tickTimer = 0f;
            _lifeTimer = 0f;
            _followPlayer = false;
        }
    }
}
