using System;
using System.Collections.Generic;
using UnityEngine;

namespace VampireSurvivorLike
{
    /// <summary>
    /// 敌人预制体映射表
    /// 用于将CSV中的敌人名称映射到实际的预制体
    /// </summary>
    [CreateAssetMenu(menuName = "VampireSurvivorLike/EnemyPrefabMapping")]
    public class EnemyPrefabMapping : ScriptableObject
    {
        [Serializable]
        public class EnemyEntry
        {
            public string Name;
            public GameObject Prefab;
        }

        [SerializeField]
        public List<EnemyEntry> Enemies = new List<EnemyEntry>();

        private Dictionary<string, GameObject> _prefabCache;

        /// <summary>
        /// 初始化预制体缓存
        /// </summary>
        private void BuildCache()
        {
            if (_prefabCache != null) return;

            _prefabCache = new Dictionary<string, GameObject>();
            foreach (var entry in Enemies)
            {
                if (!string.IsNullOrEmpty(entry.Name) && entry.Prefab != null)
                {
                    _prefabCache[entry.Name] = entry.Prefab;
                }
            }
        }

        /// <summary>
        /// 根据名称获取敌人预制体
        /// </summary>
        public GameObject GetPrefab(string name)
        {
            BuildCache();

            if (_prefabCache.TryGetValue(name, out var prefab))
            {
                return prefab;
            }

            Debug.LogWarning($"[EnemyPrefabMapping] 未找到敌人预制体: {name}");
            return null;
        }

        /// <summary>
        /// 检查是否存在指定名称的预制体
        /// </summary>
        public bool HasPrefab(string name)
        {
            BuildCache();
            return _prefabCache.ContainsKey(name);
        }

        /// <summary>
        /// 刷新缓存（编辑器中修改后调用）
        /// </summary>
        public void RefreshCache()
        {
            _prefabCache = null;
            BuildCache();
        }

#if UNITY_EDITOR
        /// <summary>
        /// 编辑器工具：自动从指定文件夹加载所有敌人预制体
        /// </summary>
        [ContextMenu("Auto Load Enemy Prefabs")]
        private void AutoLoadPrefabs()
        {
            Enemies.Clear();
            var guids = UnityEditor.AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Art/Prefab/Enemy" });
            
            foreach (var guid in guids)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    Enemies.Add(new EnemyEntry
                    {
                        Name = prefab.name,
                        Prefab = prefab
                    });
                }
            }
            
            UnityEditor.EditorUtility.SetDirty(this);
            Debug.Log($"[EnemyPrefabMapping] 自动加载了 {Enemies.Count} 个敌人预制体");
        }
#endif
    }
}
