using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace VampireSurvivorLike
{
    /// <summary>
    /// 技能配置数据
    /// </summary>
    [Serializable]
    public class AbilityConfigRow
    {
        public string AbilityKey;       // 技能唯一标识
        public string AbilityName;      // 技能名称
        public float Damage;            // 基础伤害
        public float Duration;          // 攻击间隔/持续时间
        public int Count;               // 数量（飞行物/剑等）
        public float Range;             // 范围
        public float Speed;             // 速度
        public int AttackCount;         // 穿透/攻击次数
        public string Description;      // 描述
    }

    /// <summary>
    /// 技能配置加载器
    /// </summary>
    public static class AbilityConfigLoader
    {
        private const string PRIMARY_CONFIG_FILE_NAME = "Config/AbilityConfig_i18n.csv";
        private const string LEGACY_CONFIG_FILE_NAME = "Config/AbilityConfig.csv";
        private static Dictionary<string, AbilityConfigRow> _configCache;

        /// <summary>
        /// 获取技能配置（缓存）
        /// </summary>
        public static AbilityConfigRow GetConfig(string abilityKey)
        {
            if (_configCache == null)
            {
                Debug.LogWarning("[AbilityConfigLoader] 配置尚未加载，请先调用 LoadAsync");
                return null;
            }

            if (_configCache.TryGetValue(abilityKey, out var config))
            {
                return config;
            }

            Debug.LogWarning($"[AbilityConfigLoader] 未找到技能配置: {abilityKey}");
            return null;
        }

        /// <summary>
        /// 异步加载配置
        /// </summary>
        public static IEnumerator LoadAsync(Action<Dictionary<string, AbilityConfigRow>> onComplete = null)
        {
            var primaryPath = Path.Combine(Application.streamingAssetsPath, PRIMARY_CONFIG_FILE_NAME);
            
            using (var request = UnityWebRequest.Get(primaryPath))
            {
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    var legacyPath = Path.Combine(Application.streamingAssetsPath, LEGACY_CONFIG_FILE_NAME);
                    using (var legacyRequest = UnityWebRequest.Get(legacyPath))
                    {
                        yield return legacyRequest.SendWebRequest();
                        if (legacyRequest.result != UnityWebRequest.Result.Success)
                        {
                            Debug.LogWarning($"[AbilityConfigLoader] 加载配置失败: {legacyRequest.error}，使用默认配置");
                            _configCache = GetDefaultConfig();
                            onComplete?.Invoke(_configCache);
                            yield break;
                        }

                        var legacyCsv = legacyRequest.downloadHandler.text;
                        _configCache = ParseCSV(legacyCsv);
                        Debug.Log($"[AbilityConfigLoader] 成功加载 {_configCache.Count} 条技能配置(legacy)");
                        onComplete?.Invoke(_configCache);
                        yield break;
                    }
                }

                var csvContent = request.downloadHandler.text;
                _configCache = ParseCSV(csvContent);
                Debug.Log($"[AbilityConfigLoader] 成功加载 {_configCache.Count} 条技能配置");
                onComplete?.Invoke(_configCache);
            }
        }

        /// <summary>
        /// 同步加载（非WebGL）
        /// </summary>
        public static Dictionary<string, AbilityConfigRow> LoadSync()
        {
            var path = Path.Combine(Application.streamingAssetsPath, PRIMARY_CONFIG_FILE_NAME);
            
            if (!File.Exists(path))
            {
                var legacyPath = Path.Combine(Application.streamingAssetsPath, LEGACY_CONFIG_FILE_NAME);
                if (!File.Exists(legacyPath))
                {
                    Debug.LogWarning("[AbilityConfigLoader] 配置文件不存在，使用默认配置");
                    _configCache = GetDefaultConfig();
                    return _configCache;
                }

                path = legacyPath;
            }

            var csvContent = File.ReadAllText(path, Encoding.UTF8);
            _configCache = ParseCSV(csvContent);
            return _configCache;
        }

        /// <summary>
        /// 获取默认配置（兼容无配置文件情况）
        /// </summary>
        private static Dictionary<string, AbilityConfigRow> GetDefaultConfig()
        {
            return new Dictionary<string, AbilityConfigRow>
            {
                { "simple_sword", new AbilityConfigRow { AbilityKey = "simple_sword", Damage = Config.InitSimpleSwordDamage, Duration = Config.InitSimpleSwordDuration, Count = Config.InitSimpleSwordCount, Range = Config.InitSimpleSwordRange } },
                { "simple_knife", new AbilityConfigRow { AbilityKey = "simple_knife", Damage = Config.InitSimpleKnifeDamage, Duration = Config.InitSimpleKnifeDuration, Count = Config.InitSimpleKnifeCount, AttackCount = Config.InitSimpleKnifeAttackCount } },
                { "rotate_sword", new AbilityConfigRow { AbilityKey = "rotate_sword", Damage = Config.InitRotateSwordDamage, Count = Config.InitRotateSwordCount, Speed = Config.InitRotateSwordSpeed, Range = Config.InitRotateSwordRange } },
                { "basket_ball", new AbilityConfigRow { AbilityKey = "basket_ball", Damage = Config.InitBasketBallDamage, Count = Config.InitBasketBallCount, Speed = Config.InitBasketBallSpeed } },
                { "bomb", new AbilityConfigRow { AbilityKey = "bomb", Damage = Config.InitBombDamage } },
                { "simple_axe", new AbilityConfigRow { AbilityKey = "simple_axe", Damage = Config.InitSimpleAxeDamage, Duration = Config.InitSimpleAxeDuration, Count = Config.InitSimpleAxeCount, AttackCount = Config.InitSimpleAxePierce } },
                { "magic_wand", new AbilityConfigRow { AbilityKey = "magic_wand", Damage = Config.InitMagicWandDamage, Duration = Config.InitMagicWandDuration, Count = Config.InitMagicWandCount, AttackCount = 1 } },
                { "simple_bow", new AbilityConfigRow { AbilityKey = "simple_bow", Damage = Config.InitSimpleBowDamage, Duration = Config.InitSimpleBowDuration, Count = Config.InitSimpleBowCount, AttackCount = Config.InitSimpleBowPierce } }
            };
        }

        /// <summary>
        /// 解析 CSV
        /// </summary>
        private static Dictionary<string, AbilityConfigRow> ParseCSV(string csvContent)
        {
            var result = new Dictionary<string, AbilityConfigRow>();
            var lines = csvContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            // 跳过表头
            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                var values = ParseCSVLine(line);
                if (values.Length < 8)
                {
                    Debug.LogWarning($"[AbilityConfigLoader] 第 {i + 1} 行数据不完整，跳过");
                    continue;
                }

                try
                {
                    var row = new AbilityConfigRow
                    {
                        AbilityKey = values[0],
                        AbilityName = values[1],
                        Damage = float.Parse(values[2]),
                        Duration = float.Parse(values[3]),
                        Count = int.Parse(values[4]),
                        Range = float.Parse(values[5]),
                        Speed = float.Parse(values[6]),
                        AttackCount = int.Parse(values[7]),
                        Description = values.Length > 8 ? values[8] : ""
                    };
                    result[row.AbilityKey] = row;
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[AbilityConfigLoader] 第 {i + 1} 行解析失败: {ex.Message}");
                }
            }

            return result;
        }

        /// <summary>
        /// 解析 CSV 行
        /// </summary>
        private static string[] ParseCSVLine(string line)
        {
            var result = new List<string>();
            var current = new StringBuilder();
            var inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                var c = line[i];

                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(current.ToString().Trim());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            result.Add(current.ToString().Trim());
            return result.ToArray();
        }

        /// <summary>
        /// 检查配置是否已加载
        /// </summary>
        public static bool IsLoaded => _configCache != null;
    }
}
