using UnityEngine;
using QFramework;
using QAssetBundle;

namespace VampireSurvivorLike
{
    /// <summary>
    /// 旋转攻击技能 - Boss原地旋转造成范围伤害
    /// </summary>
    [System.Serializable]
    public class SpinAttackSkill : BossSkillBase
    {
        public override string SkillName => "旋转攻击";
        public override float Cooldown => _cooldown;
        
        [Header("技能参数")]
        [SerializeField] private float _cooldown = 6f;
        [SerializeField] private float _chargeTime = 0.5f;           // 蓄力时间
        [SerializeField] private float _spinDuration = 2f;           // 旋转持续时间
        [SerializeField] private float _spinSpeed = 720f;            // 旋转速度（度/秒）
        [SerializeField] private float _moveSpeed = 4f;              // 旋转时移动速度
        [SerializeField] private float _damageRadius = 2f;           // 伤害半径
        [SerializeField] private float _damagePerSecond = 2f;        // 每秒伤害
        [SerializeField] private float _triggerDistance = 8f;        // 触发距离
        
        private enum SpinPhase { Charging, Spinning }
        private SpinPhase _phase;
        private float _spinTimer;
        private float _damageTimer;
        private Color _originalColor;
        private Vector3 _originalScale;
        
        public float TriggerDistance => _triggerDistance;
        
        public SpinAttackSkill() { }
        
        public SpinAttackSkill(float cooldown = 6f, float spinDuration = 2f, float triggerDistance = 8f)
        {
            _cooldown = cooldown;
            _spinDuration = spinDuration;
            _triggerDistance = triggerDistance;
        }
        
        protected override void OnExecuteStart()
        {
            _phase = SpinPhase.Charging;
            _spinTimer = 0;
            _damageTimer = 0;
            _originalColor = Boss.Sprite.color;
            _originalScale = Boss.Sprite.transform.localScale;
            Boss.SelfRigidbody2D.velocity = Vector2.zero;
        }
        
        protected override void OnExecuteUpdate()
        {
            switch (_phase)
            {
                case SpinPhase.Charging:
                    UpdateChargePhase();
                    break;
                    
                case SpinPhase.Spinning:
                    UpdateSpinPhase();
                    break;
            }
        }
        
        private void UpdateChargePhase()
        {
            // 蓄力收缩效果
            float t = ExecutionTimer / _chargeTime;
            float scale = 1f - 0.3f * t;
            Boss.Sprite.transform.localScale = _originalScale * scale;
            Boss.Sprite.color = Color.Lerp(_originalColor, Color.yellow, t);
            
            if (ExecutionTimer >= _chargeTime)
            {
                StartSpinning();
            }
        }
        
        private void StartSpinning()
        {
            _phase = SpinPhase.Spinning;
            _spinTimer = 0;
            Boss.Sprite.transform.localScale = _originalScale * 1.3f; // 放大
            Boss.Sprite.color = Color.yellow;
            
            AudioKit.PlaySound(Sfx.RETRO_EVENT_UI_01);
        }
        
        private void UpdateSpinPhase()
        {
            _spinTimer += Time.deltaTime;
            
            // 旋转效果
            Boss.Sprite.transform.Rotate(0, 0, _spinSpeed * Time.deltaTime);
            
            // 向玩家移动
            if (Player.Default)
            {
                Vector2 direction = GetDirectionToPlayer();
                Boss.SelfRigidbody2D.velocity = direction * _moveSpeed;
            }
            
            // 伤害判定
            _damageTimer += Time.deltaTime;
            if (_damageTimer >= 0.1f) // 每0.1秒判定一次
            {
                CheckDamage();
                _damageTimer = 0;
            }
            
            // 旋转结束
            if (_spinTimer >= _spinDuration)
            {
                EndExecution();
            }
        }
        
        private void CheckDamage()
        {
            if (!Player.Default) return;
            
            float distance = GetDistanceToPlayer();
            if (distance <= _damageRadius)
            {
                float damage = _damagePerSecond * 0.1f * Boss.DamageMultiplier;
                Player.Default.ApplyDamage(Mathf.Max(1, Mathf.CeilToInt(damage)), Boss.BossType.ToString(), "BossSpin");
            }
        }
        
        protected override void OnExecuteEnd()
        {
            Boss.Sprite.transform.localScale = _originalScale;
            Boss.Sprite.transform.rotation = Quaternion.identity;
            Boss.Sprite.color = _originalColor;
            Boss.SelfRigidbody2D.velocity = Vector2.zero;
        }
    }
}
