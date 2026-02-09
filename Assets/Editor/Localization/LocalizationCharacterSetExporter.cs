using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization.Tables;

namespace VampireSurvivorLike.EditorTools
{
    public static class LocalizationCharacterSetExporter
    {
        private const string TablesSearchDir = "Assets/Localization/StringTables";
        private const string OutputDir = "Assets/Localization/Generated";
        private const string OutputFile = OutputDir + "/FontCharacters.txt";

        /// <summary>
        /// 硬编码在 UI 中的额外字符（不来自本地化表，但必须包含在字体中）
        /// 包括：语言显示名、分辨率 UI 文本、特殊符号
        /// </summary>
        private const string HardcodedUiChars =
            "简体中文繁體" +       // 语言名称：简体中文、繁體中文
            "分辨率自动检测推荐" + // 分辨率设置 UI
            "调试" +               // 调试 HUD
            "×★";                 // 特殊符号（分辨率显示：1920×1080 ★）

        [MenuItem("VampireSurvivorLike/Unity Localization/Export Character Set (for TMP Font Asset)")]
        public static void Export()
        {
            Directory.CreateDirectory(OutputDir);

            var chars = new HashSet<char>();

            AddAsciiBaseline(chars);
            AddHardcodedUiChars(chars);
            AddStreamingAssetsCsvChars(chars);

            var tableGuids = AssetDatabase.FindAssets("t:StringTable", new[] { TablesSearchDir });
            for (var i = 0; i < tableGuids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(tableGuids[i]);
                var table = AssetDatabase.LoadAssetAtPath<StringTable>(path);
                if (table == null) continue;

                foreach (var entry in table.Values)
                {
                    if (entry == null) continue;
                    var value = entry.Value;
                    if (string.IsNullOrEmpty(value)) continue;

                    for (var c = 0; c < value.Length; c++)
                    {
                        chars.Add(value[c]);
                    }
                }
            }

            var sorted = new List<char>(chars);
            sorted.Sort();

            var sb = new StringBuilder(sorted.Count + 64);
            for (var i = 0; i < sorted.Count; i++)
            {
                var ch = sorted[i];
                if (ch == '\r') continue;
                sb.Append(ch);
            }

            File.WriteAllText(OutputFile, sb.ToString(), new UTF8Encoding(true));
            AssetDatabase.ImportAsset(OutputFile, ImportAssetOptions.ForceUpdate);

            EditorUtility.DisplayDialog(
                "Unity Localization",
                $"已生成字符集文件: {OutputFile}\n字符数: {sorted.Count}\n用于 TMP Font Asset Creator 的 Characters From File。",
                "OK");
        }

        private static void AddAsciiBaseline(HashSet<char> chars)
        {
            for (var c = 32; c <= 126; c++)
            {
                chars.Add((char)c);
            }

            chars.Add('\n');
            chars.Add('\t');
        }

        /// <summary>
        /// 添加硬编码 UI 字符（语言名称、分辨率文本等非来自本地化表的字符）
        /// </summary>
        private static void AddHardcodedUiChars(HashSet<char> chars)
        {
            for (var i = 0; i < HardcodedUiChars.Length; i++)
            {
                chars.Add(HardcodedUiChars[i]);
            }
        }

        /// <summary>
        /// 扫描 StreamingAssets/Localization 下的 CSV 文件，提取所有字符
        /// </summary>
        private static void AddStreamingAssetsCsvChars(HashSet<char> chars)
        {
            var csvDir = Path.Combine(Application.streamingAssetsPath, "Localization");
            if (!Directory.Exists(csvDir)) return;

            var csvFiles = Directory.GetFiles(csvDir, "*.csv", SearchOption.TopDirectoryOnly);
            for (var f = 0; f < csvFiles.Length; f++)
            {
                var content = File.ReadAllText(csvFiles[f], Encoding.UTF8);
                for (var i = 0; i < content.Length; i++)
                {
                    var ch = content[i];
                    if (ch == '\r') continue;
                    chars.Add(ch);
                }
            }
        }
    }
}

