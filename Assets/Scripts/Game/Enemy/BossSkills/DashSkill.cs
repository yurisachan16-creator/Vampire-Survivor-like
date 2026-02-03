using UnityEngine;
using QFramework;

namespace VampireSurvivorLike
{
    /// <summary>
    /// 冲刺技能 - Boss向玩家方向高速冲刺
    /// </summary>
    [System.Serializable]
    public class DashSkill : BossSkillBase
    {
        public override string SkillName => "冲刺";
        public override float Cooldown => _cooldown;
        
        [Header("技能参数")]
        [SerializeField] private float _cooldown = 5f;
        [SerializeField] private float _warningDuration = 1.5f;      // 预警时间
        [SerializeField] private float _dashSpeedMultiplier = 12f;   // 冲刺速度倍率
        [SerializeField] private float _dashExtraDistance = 5f;      // 冲刺超过目标的额外距离
        [SerializeField] private float _waitAfterDash = 0.5f;        // 冲刺后等待时间
        [SerializeField] private float _triggerDistance = 15f;       // 触发距离
        
        private enum DashPhase { Warning, Dashing, Waiting }
        private DashPhase _phase;
        private Vector3 _dashStartPos;
        private float _targetDashDistance;
        private Vector2 _dashDirection;
        private Color _originalColor;
        private int _warningFrameCount;
        
        public float TriggerDistance => _triggerDistance;
        
        public DashSkill() { }
        
        public DashSkill(float cooldown = 5f, float warningDuration = 1.5f, 
            float dashSpeedMultiplier = 12f, float triggerDistance = 15f)
        {
            _cooldown = cooldown;
            _warningDuration = warningDuration;
            _dashSpeedMultiplier = dashSpeedMultiplier;
            _triggerDistance = triggerDistance;
        }
        
        protected override void OnExecuteStart()
        {
            _phase = DashPhase.Warning;
            _warningFrameCount = 0;
            _originalColor = Boss.Sprite.color;
            Boss.SelfRigidbody2D.velocity = Vector2.zero;
            
            // 计算冲刺方向和距离
            _dashDirection = GetDirectionToPlayer();
            _targetDashDistance = GetDistanceToPlayer() + _dashExtraDistance;
        }
        
        protected override void OnExecuteUpdate()
        {
            switch (_phase)
            {
                case DashPhase.Warning:
                    UpdateWarningPhase();
                    break;
                    
                case DashPhase.Dashing:
                    UpdateDashingPhase();
                    break;
                    
                case DashPhase.Waiting:
                    UpdateWaitingPhase();
                    break;
            }
        }
        
        private void UpdateWarningPhase()
        {
            _warningFrameCount++;
            
            // 闪烁预警效果（频率逐渐加快）
            int maxFrames = (int)(_warningDuration * 60);
            int frames = 3 + (maxFrames - _warningFrameCount) / 10;
            frames = Mathf.Max(frames, 2);
            
            if (_warningFrameCount / frames % 2 == 0)
            {
                Boss.Sprite.color = Color.red;
            }
            else
            {
                Boss.Sprite.color = _originalColor;
            }
            
            // 预警时间结束，开始冲刺
            if (ExecutionTimer >= _warningDuration)
            {
                StartDashing();
            }
        }
        
        private void StartDashing()
        {
            _phase = DashPhase.Dashing;
            Boss.Sprite.color = _originalColor;
            _dashStartPos = Boss.transform.position;
            
            // 重新计算方向（玩家可能移动了）
            _dashDirection = GetDirectionToPlayer();
            _targetDashDistance = GetDistanceToPlayer() + _dashExtraDistance;
            
            // 设置冲刺速度
            Boss.SelfRigidbody2D.velocity = _dashDirection * Boss.MovementSpeed * _dashSpeedMultiplier;
            
            AudioKit.PlaySound("Dash");
        }
        
        private void UpdateDashingPhase()
        {
            float currentDashDistance = Vector2.Distance(Boss.transform.position, _dashStartPos);
            
            // 冲刺距离达到目标
            if (currentDashDistance >= _targetDashDistance)
            {
                _phase = DashPhase.Waiting;
                Boss.SelfRigidbody2D.velocity = Vector2.zero;
                ExecutionTimer = 0; // 重置计时器用于等待阶段
            }
        }
        
        private void UpdateWaitingPhase()
        {
            if (ExecutionTimer >= _waitAfterDash)
            {
                EndExecution();
            }
        }
        
        protected override void OnExecuteEnd()
        {
            Boss.Sprite.color = _originalColor;
            Boss.SelfRigidbody2D.velocity = Vector2.zero;
        }
    }
}
