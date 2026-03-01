using UnityEngine;

namespace VampireSurvivorLike
{
    [DisallowMultipleComponent]
    public sealed class PooledSwordSlash : MonoBehaviour, ObjectPoolSystem.IPoolable
    {
        private float _t;
        private float _damage;
        private bool _hasHit;
        private bool _playing;

        private const float PhaseA = 0.2f;
        private const float PhaseB = 0.2f;
        private const float PhaseC = 0.3f;
        private const float Total = PhaseA + PhaseB + PhaseC;

        public void Configure(float damage)
        {
            _damage = damage;
            _hasHit = false;
            _t = 0f;
            _playing = true;
            transform.localEulerAngles = Vector3.zero;
            transform.localScale = Vector3.one;
        }

        private void Update()
        {
            if (!_playing) return;

            _t += Time.deltaTime;

            var z = 0f;
            var scale = 1f;

            if (_t <= PhaseA)
            {
                var p = Mathf.Clamp01(_t / PhaseA);
                z = Mathf.Lerp(0f, 10f, p);
                scale = Mathf.Lerp(0f, 1f, Mathf.Clamp01(p * 4f));
            }
            else if (_t <= PhaseA + PhaseB)
            {
                var p = Mathf.Clamp01((_t - PhaseA) / PhaseB);
                z = Mathf.Lerp(10f, -180f, p);
                scale = 1f;
            }
            else if (_t <= Total)
            {
                var p = Mathf.Clamp01((_t - PhaseA - PhaseB) / PhaseC);
                z = Mathf.Lerp(-180f, 0f, p);
                scale = Mathf.Abs(z) / 180f;
            }
            else
            {
                _playing = false;
                ObjectPoolSystem.Despawn(gameObject);
                return;
            }

            transform.localEulerAngles = new Vector3(0f, 0f, z);
            transform.localScale = new Vector3(scale, scale, 1f);
        }

        private void OnTriggerEnter2D(Collider2D collider2D)
        {
            if (_hasHit) return;
            if (!collider2D.TryGetComponent<HitHurtBox>(out var hitHurtBox)) return;
            if (!hitHurtBox.IsEnemyOwner) return;
            if (!hitHurtBox.TryGetEnemy(out var enemy)) return;

            _hasHit = true;
            DamageSystem.CalculateDamage(_damage, enemy);
        }

        public void OnSpawned()
        {
            _t = 0f;
            _damage = 0f;
            _hasHit = false;
            _playing = false;
        }

        public void OnDespawned()
        {
            _playing = false;
        }
    }
}

