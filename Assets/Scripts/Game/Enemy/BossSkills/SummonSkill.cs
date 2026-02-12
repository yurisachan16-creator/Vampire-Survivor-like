using UnityEngine;
using QFramework;
using QAssetBundle;

namespace VampireSurvivorLike
{
    /// <summary>
    /// 召唤技能 - Boss召唤小怪援助
    /// </summary>
    [System.Serializable]
    public class SummonSkill : BossSkillBase
    {
        public override string SkillName => "召唤";
        public override float Cooldown => _cooldown;
        
        [Header("技能参数")]
        [SerializeField] private float _cooldown = 12f;
        [SerializeField] private float _summonDuration = 2f;         // 召唤持续时间
        [SerializeField] private int _summonCount = 4;               // 召唤数量
        [SerializeField] private float _summonRadius = 3f;           // 召唤半径
        [SerializeField] private float _summonInterval = 0.3f;       // 每只召唤间隔
        [SerializeField] private float _triggerHPPercent = 0.7f;     // 血量低于此百分比时触发
        
        private GameObject _minionPrefab;
        private int _summonedCount;
        private float _summonTimer;
        private float _initialHealth;
        private bool _isSummoning;
        private Color _originalColor;
        
        public float TriggerHPPercent => _triggerHPPercent;
        
        public SummonSkill() { }
        
        public SummonSkill(float cooldown = 12f, int summonCount = 4, float triggerHPPercent = 0.7f)
        {
            _cooldown = cooldown;
            _summonCount = summonCount;
            _triggerHPPercent = triggerHPPercent;
        }
        
        public override void Initialize(EnemyMiniBoss boss)
        {
            base.Initialize(boss);
            _minionPrefab = boss.MinionPrefab;
            _initialHealth = boss.Health;
        }
        
        /// <summary>
        /// 检查是否应该触发召唤（基于血量百分比）
        /// </summary>
        public bool ShouldTrigger()
        {
            if (Boss == null) return false;
            float hpPercent = Boss.Health / _initialHealth;
            return hpPercent <= _triggerHPPercent;
        }
        
        protected override void OnExecuteStart()
        {
            _isSummoning = true;
            _summonedCount = 0;
            _summonTimer = 0;
            _originalColor = Boss.Sprite.color;
            Boss.SelfRigidbody2D.velocity = Vector2.zero;
            
            // 召唤特效 - 紫色光芒
            Boss.Sprite.color = new Color(0.8f, 0.2f, 1f);
            
            AudioKit.PlaySound(Sfx.LEVELUP);
        }
        
        protected override void OnExecuteUpdate()
        {
            if (!_isSummoning) return;
            
            // 脉冲效果
            float pulse = 1f + 0.15f * Mathf.Sin(ExecutionTimer * 10f);
            Boss.Sprite.transform.localScale = Vector3.one * pulse;
            
            _summonTimer += Time.deltaTime;
            
            // 按间隔召唤小怪
            if (_summonedCount < _summonCount && _summonTimer >= _summonInterval)
            {
                SummonMinion();
                _summonTimer = 0;
            }
            
            // 召唤完成或时间到
            if (_summonedCount >= _summonCount || ExecutionTimer >= _summonDuration)
            {
                _isSummoning = false;
                EndExecution();
            }
        }
        
        private void SummonMinion()
        {
            if (_minionPrefab == null)
            {
                Debug.LogWarning("[SummonSkill] 未设置小怪预制体");
                _summonedCount = _summonCount;
                return;
            }
            
            // 在Boss周围随机位置召唤
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * _summonRadius;
            Vector3 spawnPos = Boss.transform.position + new Vector3(offset.x, offset.y, 0);
            
            var minion = ObjectPoolSystem.Spawn(_minionPrefab, null, false);
            if (!minion) return;
            minion.transform.position = spawnPos;
            minion.transform.rotation = Quaternion.identity;
            minion.SetActive(true);
            
            // 播放召唤特效
            SpawnSummonEffect(spawnPos);
            
            _summonedCount++;
            
            AudioKit.PlaySound(Sfx.EXP);
        }
        
        private void SpawnSummonEffect(Vector3 position)
        {
            // 简单的缩放动画效果（可以扩展为粒子特效）
            // 这里仅作示意，实际可通过FxController播放特效
        }
        
        protected override void OnExecuteEnd()
        {
            Boss.Sprite.transform.localScale = Vector3.one;
            Boss.Sprite.color = _originalColor;
        }
    }
}
