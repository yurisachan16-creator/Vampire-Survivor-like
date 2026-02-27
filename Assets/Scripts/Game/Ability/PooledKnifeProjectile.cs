using UnityEngine;
using QFramework;

namespace VampireSurvivorLike
{
    [DisallowMultipleComponent]
    public sealed class PooledKnifeProjectile : MonoBehaviour, ObjectPoolSystem.IPoolable
    {
        private Rigidbody2D _rb;
        private int _hitCount;
        private int _maxHits;
        private float _baseDamage;
        private bool _superKnife;
        private float _maxDistanceFromPlayer;

        public void Configure(Vector2 direction, float speed, float baseDamage, bool superKnife, int maxHits, float maxDistanceFromPlayer)
        {
            EnsureRefs();
            _rb.velocity = direction * speed;
            transform.up = direction;

            _baseDamage = baseDamage;
            _superKnife = superKnife;
            _maxHits = Mathf.Max(2, maxHits);
            _maxDistanceFromPlayer = Mathf.Max(0.1f, maxDistanceFromPlayer);
            _hitCount = 0;
        }

        private void Update()
        {
            if (!Player.Default) return;
            if (Player.Default.Distance2D(gameObject) > _maxDistanceFromPlayer)
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

            var damageTimes = _superKnife ? Random.Range(2, 4) : 1;
            DamageSystem.CalculateDamage(_baseDamage * damageTimes, enemy);

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
            _hitCount = 0;
            _maxHits = 2;
            _baseDamage = 0f;
            _superKnife = false;
            _maxDistanceFromPlayer = 20f;
        }

        public void OnDespawned()
        {
            if (_rb) _rb.velocity = Vector2.zero;
        }
    }
}
