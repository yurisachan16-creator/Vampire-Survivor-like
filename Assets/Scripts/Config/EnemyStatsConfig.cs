using System;
using System.Collections.Generic;
using UnityEngine;

namespace VampireSurvivorLike
{
    /// <summary>
    /// 敌人属性配置 - 集中管理所有敌人的基础属性
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyStatsConfig", menuName = "VampireSurvivorLike/Enemy Stats Config")]
    public class EnemyStatsConfig : ScriptableObject
    {
        [Header("配置说明")]
        [TextArea(2, 4)]
        public string Description = "在此配置所有敌人的基础属性，敌人会在Start时自动读取对应配置";
        
        [Header("敌人属性列表")]
        [SerializeField]
        public List<EnemyStatsEntry> EnemyStats = new List<EnemyStatsEntry>();
        
        /// <summary>
        /// 敌人属性字典缓存（运行时使用）
        /// </summary>
        private Dictionary<string, EnemyStatsEntry> _statsDict;
        
        /// <summary>
        /// 根据敌人ID获取属性配置
        /// </summary>
        public EnemyStatsEntry GetStats(string enemyId)
        {
            if (_statsDict == null)
            {
                BuildDictionary();
            }
            
            if (_statsDict.TryGetValue(enemyId, out var stats))
            {
                return stats;
            }
            
            Debug.LogWarning($"[EnemyStatsConfig] 未找到敌人配置: {enemyId}");
            return null;
        }
        
        /// <summary>
        /// 构建字典缓存
        /// </summary>
        private void BuildDictionary()
        {
            _statsDict = new Dictionary<string, EnemyStatsEntry>();
            foreach (var entry in EnemyStats)
            {
                if (string.IsNullOrEmpty(entry.EnemyId)) continue;
                
                if (_statsDict.ContainsKey(entry.EnemyId))
                {
                    Debug.LogWarning($"[EnemyStatsConfig] 重复的敌人ID: {entry.EnemyId}");
                    continue;
                }
                
                _statsDict[entry.EnemyId] = entry;
            }
        }
        
        /// <summary>
        /// 编辑器中修改后刷新缓存
        /// </summary>
        private void OnValidate()
        {
            _statsDict = null;
        }
        
        /// <summary>
        /// 全局单例实例
        /// </summary>
        private static EnemyStatsConfig _instance;
        
        /// <summary>
        /// 获取全局配置实例
        /// </summary>
        public static EnemyStatsConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<EnemyStatsConfig>("EnemyStatsConfig");
                    if (_instance == null)
                    {
                        Debug.LogError("[EnemyStatsConfig] 未找到配置文件！请在 Resources 文件夹中创建 EnemyStatsConfig");
                    }
                }
                return _instance;
            }
        }
    }
    
    /// <summary>
    /// 单个敌人的属性配置条目
    /// </summary>
    [Serializable]
    public class EnemyStatsEntry
    {
        [Header("敌人标识")]
        [Tooltip("敌人唯一ID，建议使用预制体名称，如 Enemy_Ghost")]
        public string EnemyId;
        
        [Tooltip("敌人显示名称（仅用于编辑器显示）")]
        public string DisplayName;
        
        [Header("基础属性")]
        [Tooltip("基础血量")]
        public float BaseHP = 3f;
        
        [Tooltip("基础移动速度")]
        public float BaseSpeed = 2f;
        
        [Tooltip("基础伤害倍率")]
        public float BaseDamageMultiplier = 1f;
        
        [Header("掉落配置")]
        [Tooltip("经验掉落概率")]
        [Range(0f, 1f)]
        public float ExpDropRate = 0.3f;
        
        [Tooltip("金币掉落概率")]
        [Range(0f, 1f)]
        public float CoinDropRate = 0.3f;
        
        [Tooltip("血瓶掉落概率")]
        [Range(0f, 1f)]
        public float HpDropRate = 0.1f;
        
        [Tooltip("炸弹掉落概率")]
        [Range(0f, 1f)]
        public float BombDropRate = 0.05f;
        
        [Header("视觉效果")]
        [Tooltip("死亡溶解颜色")]
        public Color DissolveColor = Color.yellow;
    }
}
