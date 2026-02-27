using UnityEngine;
using QFramework;

namespace VampireSurvivorLike
{
    /// <summary>
    /// Boss弹幕 - 用于范围攻击技能
    /// </summary>
    public class BossProjectile : MonoBehaviour
    {
        private float _damage = 1f;
        private float _lifetime = 3f;
        private float _timer = 0f;
        private float _armingDelay = 0.08f;
        private bool _hasHit = false;
        private string _bossId = string.Empty;
        private string _damageSource = "BossProjectile";
        private Collider2D _hitCollider;
        private Rigidbody2D _rigidbody2D;

        private void Awake()
        {
            _hitCollider = GetComponentInChildren<Collider2D>();
            _rigidbody2D = GetComponent<Rigidbody2D>();
        }
        
        public void Initialize(float damage, float lifetime)
        {
            _damage = damage;
            _lifetime = lifetime;
            _timer = 0f;
            _hasHit = false;
            _bossId = string.Empty;
            _damageSource = "BossProjectile";
            if (_hitCollider) _hitCollider.enabled = true;
        }

        public void Initialize(float damage, float lifetime, string bossId, string damageSource)
        {
            _damage = damage;
            _lifetime = lifetime;
            _timer = 0f;
            _hasHit = false;
            _bossId = bossId ?? string.Empty;
            _damageSource = string.IsNullOrEmpty(damageSource) ? "BossProjectile" : damageSource;
            if (_hitCollider) _hitCollider.enabled = true;
        }
        
        void Update()
        {
            _timer += Time.deltaTime;
            if (_timer >= _lifetime)
            {
                Destroy(gameObject);
            }
        }
        
        void OnTriggerEnter2D(Collider2D other)
        {
            if (_hasHit || _timer < _armingDelay) return;

            // 检查是否击中玩家
            if (other.GetComponentInParent<Player>() != null)
            {
                _hasHit = true;
                if (_hitCollider) _hitCollider.enabled = false;
                if (_rigidbody2D) _rigidbody2D.velocity = Vector2.zero;

                var player = Player.Default;
                if (player)
                {
                    player.ApplyDamage(Mathf.Max(1, Mathf.CeilToInt(_damage)), _bossId, _damageSource);
                }
                
                // 销毁弹幕
                Destroy(gameObject);
            }
        }
    }
}
