using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace VampireSurvivorLike
{
    public static class DifficultyConfigLoader
    {
        private const string ConfigFileName = "Config/DifficultyConfig.csv";

        public static Dictionary<GameDifficulty, DifficultyProfile> LoadSync()
        {
            var path = Path.Combine(Application.streamingAssetsPath, ConfigFileName);
            if (!File.Exists(path))
            {
                Debug.LogWarning($"[DifficultyConfigLoader] Config file not found: {path}");
                return null;
            }

            var csvContent = File.ReadAllText(path);
            return ParseCsv(csvContent);
        }

        public static IEnumerator LoadAsync(Action<Dictionary<GameDifficulty, DifficultyProfile>> onComplete)
        {
            var path = Path.Combine(Application.streamingAssetsPath, ConfigFileName);
            var requestPath = ToRequestPath(path);
            using (var request = UnityWebRequest.Get(requestPath))
            {
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning($"[DifficultyConfigLoader] Load failed: {request.error}");
                    if (File.Exists(path))
                    {
                        Dictionary<GameDifficulty, DifficultyProfile> fallback = null;
                        try
                        {
                            fallback = ParseCsv(File.ReadAllText(path));
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning($"[DifficultyConfigLoader] Fallback load failed: {e.Message}");
                        }

                        onComplete?.Invoke(fallback);
                        yield break;
                    }

                    onComplete?.Invoke(null);
                    yield break;
                }

                var result = ParseCsv(request.downloadHandler.text);
                onComplete?.Invoke(result);
            }
        }

        private static Dictionary<GameDifficulty, DifficultyProfile> ParseCsv(string csvContent)
        {
            if (string.IsNullOrWhiteSpace(csvContent))
            {
                return null;
            }

            var map = new Dictionary<GameDifficulty, DifficultyProfile>();
            var lines = csvContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length <= 1)
            {
                return map;
            }

            for (var i = 1; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (line.StartsWith("#")) continue;

                var values = ParseCsvLine(line);
                if (values.Length < 11)
                {
                    Debug.LogWarning($"[DifficultyConfigLoader] Line {i + 1} has insufficient columns.");
                    continue;
                }

                try
                {
                    var difficulty = ParseDifficulty(values[0]);
                    var profile = new DifficultyProfile(
                        enemyHpMultiplier: ParseFloat(values[1]),
                        enemySpeedMultiplier: ParseFloat(values[2]),
                        enemyDamageMultiplier: ParseFloat(values[3]),
                        spawnRateMultiplier: ParseFloat(values[4]),
                        expDropRateMultiplier: ParseFloat(values[5]),
                        coinDropRateMultiplier: ParseFloat(values[6]),
                        hpDropRateMultiplier: ParseFloat(values[7]),
                        bombDropRateMultiplier: ParseFloat(values[8]),
                        expValueMultiplier: ParseFloat(values[9]),
                        coinValueMultiplier: ParseFloat(values[10]));

                    map[difficulty] = profile;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[DifficultyConfigLoader] Failed parsing line {i + 1}: {e.Message}");
                }
            }

            Debug.Log($"[DifficultyConfigLoader] Loaded {map.Count} difficulty profiles.");
            return map;
        }

        private static string ToRequestPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return path;
            if (path.Contains("://")) return path;
            return $"file:///{path.Replace("\\", "/")}";
        }

        private static string[] ParseCsvLine(string line)
        {
            var result = new List<string>(12);
            var start = 0;
            var inQuotes = false;

            for (var i = 0; i < line.Length; i++)
            {
                var c = line[i];
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(line.Substring(start, i - start).Trim().Trim('"'));
                    start = i + 1;
                }
            }

            result.Add(line.Substring(start).Trim().Trim('"'));
            return result.ToArray();
        }

        private static GameDifficulty ParseDifficulty(string raw)
        {
            switch ((raw ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "easy":
                    return GameDifficulty.Easy;
                case "hard":
                    return GameDifficulty.Hard;
                case "normal":
                default:
                    return GameDifficulty.Normal;
            }
        }

        private static float ParseFloat(string raw)
        {
            if (float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                return value;
            }

            throw new FormatException($"Invalid float value: {raw}");
        }
    }
}
