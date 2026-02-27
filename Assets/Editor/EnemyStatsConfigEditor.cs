using UnityEngine;
using UnityEditor;
using VampireSurvivorLike;
using System.Linq;
using System.Collections.Generic;

namespace VampireSurvivorLikeEditor
{
    /// <summary>
    /// 敌人属性配置编辑器 - 提供可视化的敌人属性管理界面
    /// </summary>
    [CustomEditor(typeof(EnemyStatsConfig))]
    public class EnemyStatsConfigEditor : Editor
    {
        private SerializedProperty enemyStatsProp;
        private Vector2 scrollPosition;
        private string searchFilter = "";
        private bool showBatchEdit = false;
        private float batchHP = 10f;
        private float batchSpeed = 2f;
        
        private void OnEnable()
        {
            enemyStatsProp = serializedObject.FindProperty("EnemyStats");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            // 标题
            EditorGUILayout.LabelField("敌人属性配置管理", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // 工具栏
            DrawToolbar();
            
            EditorGUILayout.Space();
            
            // 批量编辑面板
            if (showBatchEdit)
            {
                DrawBatchEditPanel();
            }
            
            // 搜索和统计
            DrawSearchAndStats();
            
            EditorGUILayout.Space();
            
            // 敌人列表
            DrawEnemyList();
            
            serializedObject.ApplyModifiedProperties();
        }
        
        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            if (GUILayout.Button("自动填充预制体", EditorStyles.toolbarButton, GUILayout.Width(100)))
            {
                AutoPopulateFromPrefabs();
            }
            
            if (GUILayout.Button("排序", EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                SortEntries();
            }
            
            showBatchEdit = GUILayout.Toggle(showBatchEdit, "批量编辑", EditorStyles.toolbarButton, GUILayout.Width(70));
            
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("导出CSV", EditorStyles.toolbarButton, GUILayout.Width(70)))
            {
                ExportToCSV();
            }
            
            if (GUILayout.Button("从CSV导入", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                ImportFromCSV();
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawBatchEditPanel()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("批量编辑", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            batchHP = EditorGUILayout.FloatField("设置所有血量为", batchHP);
            if (GUILayout.Button("应用", GUILayout.Width(50)))
            {
                ApplyBatchHP(batchHP);
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            batchSpeed = EditorGUILayout.FloatField("设置所有速度为", batchSpeed);
            if (GUILayout.Button("应用", GUILayout.Width(50)))
            {
                ApplyBatchSpeed(batchSpeed);
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("所有血量 x1.5"))
            {
                MultiplyAllHP(1.5f);
            }
            if (GUILayout.Button("所有血量 x2"))
            {
                MultiplyAllHP(2f);
            }
            if (GUILayout.Button("所有血量 /2"))
            {
                MultiplyAllHP(0.5f);
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawSearchAndStats()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("搜索:", GUILayout.Width(40));
            searchFilter = EditorGUILayout.TextField(searchFilter);
            if (GUILayout.Button("清除", GUILayout.Width(50)))
            {
                searchFilter = "";
            }
            EditorGUILayout.EndHorizontal();
            
            // 统计信息
            int totalCount = enemyStatsProp.arraySize;
            EditorGUILayout.LabelField($"共 {totalCount} 个敌人配置", EditorStyles.miniLabel);
        }
        
        private void DrawEnemyList()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // 表头
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("敌人ID", EditorStyles.boldLabel, GUILayout.Width(150));
            EditorGUILayout.LabelField("显示名", EditorStyles.boldLabel, GUILayout.Width(80));
            EditorGUILayout.LabelField("血量", EditorStyles.boldLabel, GUILayout.Width(60));
            EditorGUILayout.LabelField("速度", EditorStyles.boldLabel, GUILayout.Width(60));
            EditorGUILayout.LabelField("伤害", EditorStyles.boldLabel, GUILayout.Width(50));
            EditorGUILayout.LabelField("操作", EditorStyles.boldLabel, GUILayout.Width(50));
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(2);
            
            // 列表内容
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.MaxHeight(400));
            
            for (int i = 0; i < enemyStatsProp.arraySize; i++)
            {
                var entry = enemyStatsProp.GetArrayElementAtIndex(i);
                var enemyId = entry.FindPropertyRelative("EnemyId");
                var displayName = entry.FindPropertyRelative("DisplayName");
                
                // 搜索过滤
                if (!string.IsNullOrEmpty(searchFilter))
                {
                    if (!enemyId.stringValue.ToLower().Contains(searchFilter.ToLower()) &&
                        !displayName.stringValue.ToLower().Contains(searchFilter.ToLower()))
                    {
                        continue;
                    }
                }
                
                DrawEnemyEntry(entry, i);
            }
            
            EditorGUILayout.EndScrollView();
            
            // 添加按钮
            EditorGUILayout.Space();
            if (GUILayout.Button("+ 添加新敌人配置"))
            {
                enemyStatsProp.InsertArrayElementAtIndex(enemyStatsProp.arraySize);
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawEnemyEntry(SerializedProperty entry, int index)
        {
            var enemyId = entry.FindPropertyRelative("EnemyId");
            var displayName = entry.FindPropertyRelative("DisplayName");
            var baseHP = entry.FindPropertyRelative("BaseHP");
            var baseSpeed = entry.FindPropertyRelative("BaseSpeed");
            var baseDamage = entry.FindPropertyRelative("BaseDamageMultiplier");
            
            EditorGUILayout.BeginHorizontal();
            
            // 根据ID判断类型并着色
            Color rowColor = GetRowColor(enemyId.stringValue);
            GUI.backgroundColor = rowColor;
            
            enemyId.stringValue = EditorGUILayout.TextField(enemyId.stringValue, GUILayout.Width(150));
            displayName.stringValue = EditorGUILayout.TextField(displayName.stringValue, GUILayout.Width(80));
            baseHP.floatValue = EditorGUILayout.FloatField(baseHP.floatValue, GUILayout.Width(60));
            baseSpeed.floatValue = EditorGUILayout.FloatField(baseSpeed.floatValue, GUILayout.Width(60));
            baseDamage.floatValue = EditorGUILayout.FloatField(baseDamage.floatValue, GUILayout.Width(50));
            
            GUI.backgroundColor = Color.white;
            
            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                enemyStatsProp.DeleteArrayElementAtIndex(index);
            }
            
            // 展开详细配置
            if (GUILayout.Button("...", GUILayout.Width(25)))
            {
                entry.isExpanded = !entry.isExpanded;
            }
            
            EditorGUILayout.EndHorizontal();
            
            // 展开详细配置
            if (entry.isExpanded)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                EditorGUILayout.PropertyField(entry.FindPropertyRelative("ExpDropRate"), new GUIContent("经验掉落率"));
                EditorGUILayout.PropertyField(entry.FindPropertyRelative("CoinDropRate"), new GUIContent("金币掉落率"));
                EditorGUILayout.PropertyField(entry.FindPropertyRelative("HpDropRate"), new GUIContent("血瓶掉落率"));
                EditorGUILayout.PropertyField(entry.FindPropertyRelative("BombDropRate"), new GUIContent("炸弹掉落率"));
                EditorGUILayout.PropertyField(entry.FindPropertyRelative("DissolveColor"), new GUIContent("溶解颜色"));
                
                EditorGUILayout.EndVertical();
                EditorGUI.indentLevel--;
            }
        }
        
        private Color GetRowColor(string enemyId)
        {
            if (string.IsNullOrEmpty(enemyId)) return Color.white;
            
            if (enemyId.Contains("Boss"))
                return new Color(1f, 0.7f, 0.7f); // 红色 - Boss
            if (enemyId.Contains("Elite"))
                return new Color(1f, 0.9f, 0.7f); // 金色 - 精英
            return new Color(0.9f, 0.9f, 0.9f); // 灰色 - 普通
        }
        
        private void AutoPopulateFromPrefabs()
        {
            var config = target as EnemyStatsConfig;
            var existingIds = new HashSet<string>(config.EnemyStats.Select(e => e.EnemyId));
            
            // 查找所有敌人预制体
            string[] guids = AssetDatabase.FindAssets("Enemy_ t:Prefab", new[] { "Assets/Art/Prefab/Enemy" });
            int addedCount = 0;
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                
                if (prefab == null) continue;
                
                string enemyId = prefab.name;
                if (existingIds.Contains(enemyId)) continue;
                
                // 获取预制体上的属性
                var enemy = prefab.GetComponent<Enemy>();
                var miniBoss = prefab.GetComponent<EnemyMiniBoss>();
                
                var newEntry = new EnemyStatsEntry
                {
                    EnemyId = enemyId,
                    DisplayName = enemyId.Replace("Enemy_", "").Replace("_", " ")
                };
                
                if (miniBoss != null)
                {
                    newEntry.BaseHP = miniBoss.Health;
                    newEntry.BaseSpeed = miniBoss.MovementSpeed;
                    newEntry.BaseDamageMultiplier = miniBoss.DamageMultiplier;
                }
                else if (enemy != null)
                {
                    newEntry.BaseHP = enemy.Health;
                    newEntry.BaseSpeed = enemy.MovementSpeed;
                    newEntry.BaseDamageMultiplier = enemy.DamageMultiplier;
                    newEntry.ExpDropRate = enemy.ExpDropRate;
                    newEntry.CoinDropRate = enemy.CoinDropRate;
                    newEntry.HpDropRate = enemy.HpDropRate;
                    newEntry.BombDropRate = enemy.BombDropRate;
                    newEntry.DissolveColor = enemy.DissolveColor;
                }
                
                config.EnemyStats.Add(newEntry);
                existingIds.Add(enemyId);
                addedCount++;
            }
            
            if (addedCount > 0)
            {
                EditorUtility.SetDirty(config);
                Debug.Log($"[EnemyStatsConfig] 添加了 {addedCount} 个敌人配置");
            }
            else
            {
                Debug.Log("[EnemyStatsConfig] 没有新的预制体需要添加");
            }
        }
        
        private void SortEntries()
        {
            var config = target as EnemyStatsConfig;
            config.EnemyStats = config.EnemyStats
                .OrderBy(e => e.EnemyId.Contains("Boss") ? 1 : 0)
                .ThenBy(e => e.EnemyId)
                .ToList();
            EditorUtility.SetDirty(config);
        }
        
        private void ApplyBatchHP(float hp)
        {
            for (int i = 0; i < enemyStatsProp.arraySize; i++)
            {
                var entry = enemyStatsProp.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("BaseHP").floatValue = hp;
            }
        }
        
        private void ApplyBatchSpeed(float speed)
        {
            for (int i = 0; i < enemyStatsProp.arraySize; i++)
            {
                var entry = enemyStatsProp.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("BaseSpeed").floatValue = speed;
            }
        }
        
        private void MultiplyAllHP(float multiplier)
        {
            for (int i = 0; i < enemyStatsProp.arraySize; i++)
            {
                var entry = enemyStatsProp.GetArrayElementAtIndex(i);
                var hp = entry.FindPropertyRelative("BaseHP");
                hp.floatValue *= multiplier;
            }
        }
        
        private void ExportToCSV()
        {
            var config = target as EnemyStatsConfig;
            string path = EditorUtility.SaveFilePanel("导出敌人配置", "Assets/StreamingAssets/Config", "EnemyStatsConfig", "csv");
            
            if (string.IsNullOrEmpty(path)) return;
            
            var lines = new List<string>();
            lines.Add("EnemyId,DisplayName,BaseHP,BaseSpeed,BaseDamageMultiplier,ExpDropRate,CoinDropRate,HpDropRate,BombDropRate");
            
            foreach (var entry in config.EnemyStats)
            {
                lines.Add($"{entry.EnemyId},{entry.DisplayName},{entry.BaseHP},{entry.BaseSpeed},{entry.BaseDamageMultiplier},{entry.ExpDropRate},{entry.CoinDropRate},{entry.HpDropRate},{entry.BombDropRate}");
            }
            
            System.IO.File.WriteAllLines(path, lines);
            AssetDatabase.Refresh();
            Debug.Log($"[EnemyStatsConfig] 已导出到: {path}");
        }
        
        private void ImportFromCSV()
        {
            string path = EditorUtility.OpenFilePanel("导入敌人配置", "Assets/StreamingAssets/Config", "csv");
            
            if (string.IsNullOrEmpty(path)) return;
            
            var config = target as EnemyStatsConfig;
            var lines = System.IO.File.ReadAllLines(path);
            
            if (lines.Length < 2)
            {
                Debug.LogError("[EnemyStatsConfig] CSV文件为空或格式错误");
                return;
            }
            
            config.EnemyStats.Clear();
            
            for (int i = 1; i < lines.Length; i++)
            {
                var parts = lines[i].Split(',');
                if (parts.Length < 9) continue;
                
                config.EnemyStats.Add(new EnemyStatsEntry
                {
                    EnemyId = parts[0],
                    DisplayName = parts[1],
                    BaseHP = float.Parse(parts[2]),
                    BaseSpeed = float.Parse(parts[3]),
                    BaseDamageMultiplier = float.Parse(parts[4]),
                    ExpDropRate = float.Parse(parts[5]),
                    CoinDropRate = float.Parse(parts[6]),
                    HpDropRate = float.Parse(parts[7]),
                    BombDropRate = float.Parse(parts[8])
                });
            }
            
            EditorUtility.SetDirty(config);
            Debug.Log($"[EnemyStatsConfig] 已从CSV导入 {config.EnemyStats.Count} 个配置");
        }
    }
}
