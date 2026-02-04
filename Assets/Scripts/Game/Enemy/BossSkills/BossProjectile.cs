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
        
        public void Initialize(float damage, float lifetime)
        {
            _damage = damage;
            _lifetime = lifetime;
            _timer = 0f;
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
            if (other.CompareTag("Player"))
            {
                // 对玩家造成伤害
                Global.HP.Value -= Mathf.Max(1, Mathf.CeilToInt(_damage));
                AudioKit.PlaySound("Hit");
                
                // 销毁弹幕
                Destroy(gameObject);
            }
        }
    }
}
