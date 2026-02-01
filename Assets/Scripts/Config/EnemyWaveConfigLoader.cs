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
    /// CSV 配置数据行
    /// </summary>
    [Serializable]
    public class EnemyWaveConfigRow
    {
        public string GroupName;
        public string GroupDescription;
        public string WaveName;
        public bool Active;
        public string EnemyPrefabName;
        public float GenerateDuration;
        public int KeepSeconds;
        public float HPScale;
        public float SpeedScale;
        public float DamageScale;
        // 新增字段
        public float BaseSpeed = 2f;           // 敌人基础速度
        public bool IsTreasureChest = false;   // 是否生成宝箱
        public float ExpDropRate = 0.3f;       // 经验掉落概率
        public float CoinDropRate = 0.3f;      // 金币掉落概率
        public float HpDropRate = 0.1f;        // 回血道具掉落概率
        public float BombDropRate = 0.05f;     // 炸弹掉落概率
    }

    /// <summary>
    /// 敌人波次 CSV 配置加载器
    /// 支持从 StreamingAssets 读取 CSV 文件
    /// </summary>
    public static class EnemyWaveConfigLoader
    {
        private const string CONFIG_FILE_NAME = "Config/EnemyWaveConfig.csv";
        
        /// <summary>
        /// 同步加载配置（仅支持非 WebGL 平台）
        /// </summary>
        public static List<EnemyWaveConfigRow> LoadSync()
        {
            var path = Path.Combine(Application.streamingAssetsPath, CONFIG_FILE_NAME);
            
            if (!File.Exists(path))
            {
                Debug.LogError($"[EnemyWaveConfigLoader] 配置文件不存在: {path}");
                return new List<EnemyWaveConfigRow>();
            }

            var csvContent = File.ReadAllText(path, Encoding.UTF8);
            return ParseCSV(csvContent);
        }

        /// <summary>
        /// 异步加载配置（支持所有平台，包括 WebGL）
        /// </summary>
        public static IEnumerator LoadAsync(Action<List<EnemyWaveConfigRow>> onComplete)
        {
            var path = Path.Combine(Application.streamingAssetsPath, CONFIG_FILE_NAME);
            
            using (var request = UnityWebRequest.Get(path))
            {
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"[EnemyWaveConfigLoader] 加载配置失败: {request.error}");
                    onComplete?.Invoke(new List<EnemyWaveConfigRow>());
                    yield break;
                }

                var csvContent = request.downloadHandler.text;
                var rows = ParseCSV(csvContent);
                onComplete?.Invoke(rows);
            }
        }

        /// <summary>
        /// 解析 CSV 内容
        /// </summary>
        private static List<EnemyWaveConfigRow> ParseCSV(string csvContent)
        {
            var rows = new List<EnemyWaveConfigRow>();
            var lines = csvContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            // 跳过表头
            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                var values = ParseCSVLine(line);
                if (values.Length < 10)
                {
                    Debug.LogWarning($"[EnemyWaveConfigLoader] 第 {i + 1} 行数据不完整，跳过");
                    continue;
                }

                try
                {
                    var row = new EnemyWaveConfigRow
                    {
                        GroupName = values[0],
                        GroupDescription = values[1],
                        WaveName = values[2],
                        Active = ParseBool(values[3]),
                        EnemyPrefabName = values[4],
                        GenerateDuration = float.Parse(values[5]),
                        KeepSeconds = int.Parse(values[6]),
                        HPScale = float.Parse(values[7]),
                        SpeedScale = float.Parse(values[8]),
                        DamageScale = float.Parse(values[9]),
                        // 解析新增字段（使用默认值兼容旧配置）
                        BaseSpeed = values.Length > 10 && !string.IsNullOrEmpty(values[10]) ? float.Parse(values[10]) : 2f,
                        IsTreasureChest = values.Length > 11 && !string.IsNullOrEmpty(values[11]) && ParseBool(values[11]),
                        ExpDropRate = values.Length > 12 && !string.IsNullOrEmpty(values[12]) ? float.Parse(values[12]) : 0.3f,
                        CoinDropRate = values.Length > 13 && !string.IsNullOrEmpty(values[13]) ? float.Parse(values[13]) : 0.3f,
                        HpDropRate = values.Length > 14 && !string.IsNullOrEmpty(values[14]) ? float.Parse(values[14]) : 0.1f,
                        BombDropRate = values.Length > 15 && !string.IsNullOrEmpty(values[15]) ? float.Parse(values[15]) : 0.05f
                    };
                    rows.Add(row);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[EnemyWaveConfigLoader] 第 {i + 1} 行解析失败: {ex.Message}");
                }
            }

            Debug.Log($"[EnemyWaveConfigLoader] 成功加载 {rows.Count} 条波次配置");
            return rows;
        }

        /// <summary>
        /// 解析 CSV 行，处理逗号和引号
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
        /// 解析布尔值
        /// </summary>
        private static bool ParseBool(string value)
        {
            value = value.ToUpper().Trim();
            return value == "TRUE" || value == "1" || value == "YES";
        }

        /// <summary>
        /// 将配置行转换为按组分类的字典
        /// </summary>
        public static Dictionary<string, List<EnemyWaveConfigRow>> GroupByName(List<EnemyWaveConfigRow> rows)
        {
            var groups = new Dictionary<string, List<EnemyWaveConfigRow>>();

            foreach (var row in rows)
            {
                if (!groups.ContainsKey(row.GroupName))
                {
                    groups[row.GroupName] = new List<EnemyWaveConfigRow>();
                }
                groups[row.GroupName].Add(row);
            }

            return groups;
        }
    }
}
