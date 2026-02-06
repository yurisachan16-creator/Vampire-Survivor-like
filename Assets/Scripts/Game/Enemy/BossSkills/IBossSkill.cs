using UnityEngine;

namespace VampireSurvivorLike
{
    /// <summary>
    /// Boss技能接口
    /// </summary>
    public interface IBossSkill
    {
        /// <summary>
        /// 技能名称
        /// </summary>
        string SkillName { get; }
        
        /// <summary>
        /// 技能冷却时间（秒）
        /// </summary>
        float Cooldown { get; }
        
        /// <summary>
        /// 技能是否准备就绪
        /// </summary>
        bool IsReady { get; }
        
        /// <summary>
        /// 技能是否正在执行中
        /// </summary>
        bool IsExecuting { get; }
        
        /// <summary>
        /// 初始化技能
        /// </summary>
        void Initialize(EnemyMiniBoss boss);
        
        /// <summary>
        /// 尝试执行技能
        /// </summary>
        /// <returns>是否成功开始执行</returns>
        bool TryExecute();
        
        /// <summary>
        /// 更新技能状态（在Update中调用）
        /// </summary>
        void OnUpdate();
        
        /// <summary>
        /// 固定更新（在FixedUpdate中调用）
        /// </summary>
        void OnFixedUpdate();
        
        /// <summary>
        /// 重置技能冷却
        /// </summary>
        void ResetCooldown();
    }
}
