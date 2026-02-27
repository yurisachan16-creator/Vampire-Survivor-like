using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace VampireSurvivorLike
{
    public static class EnemyConfigValidator
    {
        private const string EnemyWaveCsvRelativePath = "Assets/StreamingAssets/Config/EnemyWaveConfig.csv";
        private const string EnemyPrefabMappingAssetPath = "Assets/Art/Config/Enemy Prefab Mapping.asset";

        [MenuItem("Tools/Validate/Enemy Config (CSV + Prefabs)")]
        public static void ValidateEnemyConfig()
        {
            var errors = new List<string>();

            var mapping = AssetDatabase.LoadAssetAtPath<EnemyPrefabMapping>(EnemyPrefabMappingAssetPath);
            if (mapping == null)
            {
                errors.Add($"未找到映射表资产: {EnemyPrefabMappingAssetPath}");
                Print(errors);
                return;
            }

            if (!File.Exists(EnemyWaveCsvRelativePath))
            {
                errors.Add($"未找到CSV文件: {EnemyWaveCsvRelativePath}");
                Print(errors);
                return;
            }

            var lines = File.ReadAllLines(EnemyWaveCsvRelativePath);
            if (lines.Length <= 1)
            {
                errors.Add("EnemyWaveConfig.csv为空或只有表头");
                Print(errors);
                return;
            }

            var header = SplitCsvLine(lines[0]);
            var nameIndex = Array.IndexOf(header, "EnemyPrefabName");
            if (nameIndex < 0)
            {
                errors.Add("CSV表头缺少 EnemyPrefabName 列");
                Print(errors);
                return;
            }

            var referenced = new Dictionary<string, GameObject>(StringComparer.Ordinal);
            for (var i = 1; i < lines.Length; i++)
            {
                var line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;

                var cols = SplitCsvLine(line);
                if (cols.Length <= nameIndex)
                {
                    errors.Add($"第{i + 1}行列数不足，无法读取 EnemyPrefabName: {line}");
                    continue;
                }

                var prefabName = cols[nameIndex].Trim();
                if (string.IsNullOrEmpty(prefabName))
                {
                    errors.Add($"第{i + 1}行 EnemyPrefabName 为空");
                    continue;
                }

                var prefab = mapping.GetPrefab(prefabName);
                if (prefab == null)
                {
                    errors.Add($"第{i + 1}行 EnemyPrefabName='{prefabName}' 在映射表中未找到Prefab");
                    continue;
                }

                referenced[prefabName] = prefab;
            }

            foreach (var kv in referenced)
            {
                var key = kv.Key;
                var prefab = kv.Value;

                if (prefab == null)
                {
                    errors.Add($"映射Key '{key}' 对应Prefab为null");
                    continue;
                }

                var enemy = prefab.GetComponentInChildren<IEnemy>(true);
                if (enemy == null)
                {
                    errors.Add($"Prefab '{prefab.name}' (Key='{key}') 缺少 IEnemy 组件");
                }

                var spriteRenderer = prefab.GetComponentInChildren<SpriteRenderer>(true);
                if (spriteRenderer == null)
                {
                    errors.Add($"Prefab '{prefab.name}' (Key='{key}') 缺少 SpriteRenderer");
                }
                else if (spriteRenderer.sprite == null)
                {
                    errors.Add($"Prefab '{prefab.name}' (Key='{key}') SpriteRenderer未绑定Sprite");
                }
            }

            Print(errors);
        }

        private static void Print(List<string> errors)
        {
            if (errors.Count == 0)
            {
                Debug.Log("[EnemyConfigValidator] OK");
                return;
            }

            Debug.LogError("[EnemyConfigValidator] Failed\n" + string.Join("\n", errors));
        }

        private static string[] SplitCsvLine(string line)
        {
            return line.Split(',').Select(s => s.Trim()).ToArray();
        }
    }
}

