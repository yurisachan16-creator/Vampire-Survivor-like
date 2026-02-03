using UnityEngine;

namespace VampireSurvivorLike
{
    /// <summary>
    /// Boss技能基类，提供通用功能
    /// </summary>
    public abstract class BossSkillBase : IBossSkill
    {
        public abstract string SkillName { get; }
        public abstract float Cooldown { get; }
        
        protected EnemyMiniBoss Boss { get; private set; }
        protected float CooldownTimer { get; set; }
        protected float ExecutionTimer { get; set; }
        
        public bool IsReady => CooldownTimer <= 0 && !IsExecuting;
        public bool IsExecuting { get; protected set; }
        
        public virtual void Initialize(EnemyMiniBoss boss)
        {
            Boss = boss;
            CooldownTimer = 0;
            IsExecuting = false;
        }
        
        public bool TryExecute()
        {
            if (!IsReady) return false;
            if (Boss == null || !Player.Default) return false;
            
            IsExecuting = true;
            ExecutionTimer = 0;
            OnExecuteStart();
            return true;
        }
        
        public virtual void OnUpdate()
        {
            if (!IsExecuting && CooldownTimer > 0)
            {
                CooldownTimer -= Time.deltaTime;
            }
            
            if (IsExecuting)
            {
                ExecutionTimer += Time.deltaTime;
                OnExecuteUpdate();
            }
        }
        
        public virtual void OnFixedUpdate()
        {
            if (IsExecuting)
            {
                OnExecuteFixedUpdate();
            }
        }
        
        public void ResetCooldown()
        {
            CooldownTimer = Cooldown;
        }
        
        /// <summary>
        /// 结束技能执行
        /// </summary>
        protected void EndExecution()
        {
            IsExecuting = false;
            ResetCooldown();
            OnExecuteEnd();
        }
        
        /// <summary>
        /// 技能开始执行时调用
        /// </summary>
        protected abstract void OnExecuteStart();
        
        /// <summary>
        /// 技能执行中每帧调用
        /// </summary>
        protected abstract void OnExecuteUpdate();
        
        /// <summary>
        /// 技能执行中固定帧调用
        /// </summary>
        protected virtual void OnExecuteFixedUpdate() { }
        
        /// <summary>
        /// 技能结束时调用
        /// </summary>
        protected virtual void OnExecuteEnd() { }
        
        /// <summary>
        /// 获取指向玩家的方向
        /// </summary>
        protected Vector2 GetDirectionToPlayer()
        {
            if (!Player.Default) return Vector2.zero;
            return (Player.Default.transform.position - Boss.transform.position).normalized;
        }
        
        /// <summary>
        /// 获取与玩家的距离
        /// </summary>
        protected float GetDistanceToPlayer()
        {
            if (!Player.Default) return float.MaxValue;
            return Vector2.Distance(Boss.transform.position, Player.Default.transform.position);
        }
    }
}
