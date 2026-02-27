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
    /// 时间轴频道 CSV 配置数据行
    /// </summary>
    [Serializable]
    public class SpawnChannelConfigRow
    {
        public string ChannelName;
        public bool Active = true;
        public string EnemyPrefabName;
        public string Phase;
        public float StartTimeSec;
        public float EndTimeSec = -1f;
        public float SpawnIntervalSec = 1f;
        public int SpawnCount = 0;
        public float HPScale = 1f;
        public float SpeedScale = 1f;
        public float DamageScale = 1f;
        public float BaseSpeed = 2f;
        public bool IsTreasureChest = false;
        public float ExpDropRate = 0.3f;
        public float CoinDropRate = 0.3f;
        public float HpDropRate = 0.1f;
        public float BombDropRate = 0.05f;
        public string SpawnPattern = "edge";
        public int BurstCount = 1;
        public float SpawnRadius = 12f;
    }

    /// <summary>
    /// 时间轴频道 CSV 配置加载器
    /// 支持从 StreamingAssets 读取 CSV 文件
    /// </summary>
    public static class SpawnChannelConfigLoader
    {
        private const string CONFIG_FILE_NAME = "Config/EnemyWaveConfig.csv";

        /// <summary>
        /// 同步加载配置（仅支持非 WebGL 平台）
        /// </summary>
        public static List<SpawnChannelConfigRow> LoadSync()
        {
            var path = Path.Combine(Application.streamingAssetsPath, CONFIG_FILE_NAME);

            if (!File.Exists(path))
            {
                Debug.LogError($"[SpawnChannelConfigLoader] 配置文件不存在: {path}");
                return new List<SpawnChannelConfigRow>();
            }

            var csvContent = File.ReadAllText(path, Encoding.UTF8);
            return ParseCSV(csvContent);
        }

        /// <summary>
        /// 异步加载配置（支持所有平台，包括 WebGL）
        /// </summary>
        public static IEnumerator LoadAsync(Action<List<SpawnChannelConfigRow>> onComplete)
        {
            var path = Path.Combine(Application.streamingAssetsPath, CONFIG_FILE_NAME);

            using (var request = UnityWebRequest.Get(path))
            {
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"[SpawnChannelConfigLoader] 加载配置失败: {request.error}");
                    onComplete?.Invoke(new List<SpawnChannelConfigRow>());
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
        private static List<SpawnChannelConfigRow> ParseCSV(string csvContent)
        {
            var rows = new List<SpawnChannelConfigRow>();
            var lines = csvContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            // 跳过表头
            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                var values = ParseCSVLine(line);
                if (values.Length < 6)
                {
                    Debug.LogWarning($"[SpawnChannelConfigLoader] 第 {i + 1} 行数据不完整（至少需要6列），跳过");
                    continue;
                }

                try
                {
                    // CSV列顺序（新时间轴格式）：
                    // 0  ChannelName
                    // 1  Active
                    // 2  EnemyPrefabName
                    // 3  Phase (small/boss)
                    // 4  StartTimeSec
                    // 5  EndTimeSec (-1 = 持续到游戏结束)
                    // 6  SpawnIntervalSec
                    // 7  SpawnCount (0 = 按时间持续刷)
                    // 8  HPScale
                    // 9  SpeedScale
                    // 10 DamageScale
                    // 11 BaseSpeed
                    // 12 IsTreasureChest
                    // 13 ExpDropRate
                    // 14 CoinDropRate
                    // 15 HpDropRate
                    // 16 BombDropRate
                    // 17 SpawnPattern
                    // 18 BurstCount
                    // 19 SpawnRadius
                    var row = new SpawnChannelConfigRow
                    {
                        ChannelName = values[0],
                        Active = ParseBool(values[1]),
                        EnemyPrefabName = values[2],
                        Phase = values[3],
                        StartTimeSec = float.Parse(values[4]),
                        EndTimeSec = float.Parse(values[5]),
                        SpawnIntervalSec = values.Length > 6 && !string.IsNullOrEmpty(values[6]) ? float.Parse(values[6]) : 1f,
                        SpawnCount = values.Length > 7 && !string.IsNullOrEmpty(values[7]) ? int.Parse(values[7]) : 0,
                        HPScale = values.Length > 8 && !string.IsNullOrEmpty(values[8]) ? float.Parse(values[8]) : 1f,
                        SpeedScale = values.Length > 9 && !string.IsNullOrEmpty(values[9]) ? float.Parse(values[9]) : 1f,
                        DamageScale = values.Length > 10 && !string.IsNullOrEmpty(values[10]) ? float.Parse(values[10]) : 1f,
                        BaseSpeed = values.Length > 11 && !string.IsNullOrEmpty(values[11]) ? float.Parse(values[11]) : 2f,
                        IsTreasureChest = values.Length > 12 && !string.IsNullOrEmpty(values[12]) && ParseBool(values[12]),
                        ExpDropRate = values.Length > 13 && !string.IsNullOrEmpty(values[13]) ? float.Parse(values[13]) : 0.3f,
                        CoinDropRate = values.Length > 14 && !string.IsNullOrEmpty(values[14]) ? float.Parse(values[14]) : 0.3f,
                        HpDropRate = values.Length > 15 && !string.IsNullOrEmpty(values[15]) ? float.Parse(values[15]) : 0.1f,
                        BombDropRate = values.Length > 16 && !string.IsNullOrEmpty(values[16]) ? float.Parse(values[16]) : 0.05f,
                        SpawnPattern = values.Length > 17 && !string.IsNullOrEmpty(values[17]) ? values[17].Trim().ToLowerInvariant() : "edge",
                        BurstCount = values.Length > 18 && !string.IsNullOrEmpty(values[18]) ? int.Parse(values[18]) : 1,
                        SpawnRadius = values.Length > 19 && !string.IsNullOrEmpty(values[19]) ? float.Parse(values[19]) : 12f
                    };
                    rows.Add(row);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[SpawnChannelConfigLoader] 第 {i + 1} 行解析失败: {ex.Message}");
                }
            }

            Debug.Log($"[SpawnChannelConfigLoader] 成功加载 {rows.Count} 条频道配置");
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
    }
}
