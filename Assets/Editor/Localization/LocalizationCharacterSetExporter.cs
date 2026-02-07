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

        [MenuItem("VampireSurvivorLike/Unity Localization/Export Character Set (for TMP Font Asset)")]
        public static void Export()
        {
            Directory.CreateDirectory(OutputDir);

            var chars = new HashSet<char>();

            AddAsciiBaseline(chars);

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
                $"已生成字符集文件: {OutputFile}\\n字符数: {sorted.Count}\\n用于 TMP Font Asset Creator 的 Characters From File。",
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
    }
}

