using UnityEngine;
using QFramework;
using QAssetBundle;

namespace VampireSurvivorLike
{
    /// <summary>
    /// 范围攻击技能 - Boss向周围发射环形弹幕
    /// </summary>
    [System.Serializable]
    public class AreaAttackSkill : BossSkillBase
    {
        public override string SkillName => "环形弹幕";
        public override float Cooldown => _cooldown;
        
        [Header("技能参数")]
        [SerializeField] private float _cooldown = 8f;
        [SerializeField] private float _chargeDuration = 1f;         // 蓄力时间
        [SerializeField] private int _projectileCount = 8;           // 弹幕数量
        [SerializeField] private float _projectileSpeed = 8f;        // 弹幕速度
        [SerializeField] private float _projectileLifetime = 3f;     // 弹幕存活时间
        [SerializeField] private float _damage = 1f;                 // 弹幕伤害
        [SerializeField] private int _waveCount = 1;                 // 波次数量
        [SerializeField] private float _waveInterval = 0.3f;         // 波次间隔
        [SerializeField] private float _triggerDistance = 12f;       // 触发距离
        
        private GameObject _projectilePrefab;
        private int _currentWave;
        private float _waveTimer;
        private bool _isCharging;
        private Color _originalColor;
        
        public float TriggerDistance => _triggerDistance;
        
        public AreaAttackSkill() { }
        
        public AreaAttackSkill(float cooldown = 8f, int projectileCount = 8, 
            int waveCount = 1, float triggerDistance = 12f)
        {
            _cooldown = cooldown;
            _projectileCount = projectileCount;
            _waveCount = waveCount;
            _triggerDistance = triggerDistance;
        }
        
        public override void Initialize(EnemyMiniBoss boss)
        {
            base.Initialize(boss);
            
            // 获取或创建弹幕预制体
            _projectilePrefab = boss.BossProjectilePrefab;
        }
        
        protected override void OnExecuteStart()
        {
            _isCharging = true;
            _currentWave = 0;
            _waveTimer = 0;
            _originalColor = Boss.Sprite.color;
            Boss.SelfRigidbody2D.velocity = Vector2.zero;
            
            // 蓄力效果 - 放大闪烁
            Boss.Sprite.color = new Color(1f, 0.5f, 0f); // 橙色警告
        }
        
        protected override void OnExecuteUpdate()
        {
            if (_isCharging)
            {
                // 蓄力阶段 - 脉冲效果
                float pulse = 1f + 0.2f * Mathf.Sin(ExecutionTimer * 15f);
                Boss.Sprite.transform.localScale = Vector3.one * pulse;
                
                if (ExecutionTimer >= _chargeDuration)
                {
                    _isCharging = false;
                    Boss.Sprite.transform.localScale = Vector3.one;
                    Boss.Sprite.color = _originalColor;
                    FireWave();
                }
            }
            else
            {
                // 发射阶段
                _waveTimer += Time.deltaTime;
                
                if (_currentWave < _waveCount && _waveTimer >= _waveInterval)
                {
                    FireWave();
                    _waveTimer = 0;
                }
                
                // 所有波次发射完毕
                if (_currentWave >= _waveCount)
                {
                    EndExecution();
                }
            }
        }
        
        private void FireWave()
        {
            if (_projectilePrefab == null)
            {
                Debug.LogWarning("[AreaAttackSkill] 未设置弹幕预制体");
                _currentWave = _waveCount; // 跳过所有波次
                return;
            }
            
            AudioKit.PlaySound(Sfx.RETRO_EVENT_ACUTE_11);
            
            float angleStep = 360f / _projectileCount;
            float startAngle = _currentWave * (angleStep / 2); // 每波偏移一定角度
            
            for (int i = 0; i < _projectileCount; i++)
            {
                float angle = startAngle + i * angleStep;
                Vector2 direction = new Vector2(
                    Mathf.Cos(angle * Mathf.Deg2Rad),
                    Mathf.Sin(angle * Mathf.Deg2Rad)
                );
                
                SpawnProjectile(direction);
            }
            
            _currentWave++;
        }
        
        private void SpawnProjectile(Vector2 direction)
        {
            var projectile = ObjectPoolSystem.Spawn(_projectilePrefab, null, false);
            if (!projectile) return;
            projectile.transform.position = Boss.transform.position;
            projectile.transform.rotation = Quaternion.identity;
            projectile.SetActive(true);
            
            var rb = projectile.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = direction * _projectileSpeed;
            }
            
            var bossProjectile = projectile.GetComponent<BossProjectile>();
            if (bossProjectile != null)
            {
                bossProjectile.Initialize(_damage * Boss.DamageMultiplier, _projectileLifetime, Boss.BossType.ToString(), "BossProjectile");
            }
            
            // 设置旋转朝向移动方向
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            projectile.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        
        protected override void OnExecuteEnd()
        {
            Boss.Sprite.transform.localScale = Vector3.one;
            Boss.Sprite.color = _originalColor;
        }
    }
}
