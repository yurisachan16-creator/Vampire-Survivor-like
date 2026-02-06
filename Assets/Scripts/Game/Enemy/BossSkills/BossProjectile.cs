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
        private string _bossId = string.Empty;
        private string _damageSource = "BossProjectile";
        
        public void Initialize(float damage, float lifetime)
        {
            _damage = damage;
            _lifetime = lifetime;
            _timer = 0f;
            _bossId = string.Empty;
            _damageSource = "BossProjectile";
        }

        public void Initialize(float damage, float lifetime, string bossId, string damageSource)
        {
            _damage = damage;
            _lifetime = lifetime;
            _timer = 0f;
            _bossId = bossId ?? string.Empty;
            _damageSource = string.IsNullOrEmpty(damageSource) ? "BossProjectile" : damageSource;
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
            // 检查是否击中玩家
            if (other.GetComponentInParent<Player>() != null)
            {
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
