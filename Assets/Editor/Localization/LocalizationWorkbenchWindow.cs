using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace VampireSurvivorLike.EditorTools
{
    public sealed class LocalizationWorkbenchWindow : EditorWindow
    {
        private const string DefaultExportCsvPath = "Docs/LocalizationExport.csv";
        private const string DefaultExportMetaPath = "Docs/LocalizationExport.meta.json";
        private const string LocalizationDirRelative = "Assets/StreamingAssets/Localization";

        private string _exportCsvPath = DefaultExportCsvPath;
        private string _exportMetaPath = DefaultExportMetaPath;

        [MenuItem("VampireSurvivorLike/Localization Workbench")]
        public static void Open()
        {
            GetWindow<LocalizationWorkbenchWindow>("Localization Workbench");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Runtime Path", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(LocalizationDirRelative);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Export / Import", EditorStyles.boldLabel);

            _exportCsvPath = EditorGUILayout.TextField("Export CSV", _exportCsvPath);
            _exportMetaPath = EditorGUILayout.TextField("Export Meta", _exportMetaPath);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Export")) Export();
                if (GUILayout.Button("Import")) Import();
            }
        }

        private static string ProjectPathToAbsolute(string projectRelativePath)
        {
            if (string.IsNullOrWhiteSpace(projectRelativePath)) return string.Empty;
            var root = Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath;
            return Path.GetFullPath(Path.Combine(root, projectRelativePath));
        }

        private void Export()
        {
            var runtimeDir = ProjectPathToAbsolute(LocalizationDirRelative);
            if (!Directory.Exists(runtimeDir))
            {
                EditorUtility.DisplayDialog("Localization", "StreamingAssets/Localization 不存在", "OK");
                return;
            }

            var manifestFile = Path.Combine(runtimeDir, "manifest.json");
            var manifestJson = File.Exists(manifestFile) ? File.ReadAllText(manifestFile, Encoding.UTF8) : string.Empty;
            var manifest = !string.IsNullOrWhiteSpace(manifestJson) ? JsonUtility.FromJson<LocalizationManifest>(manifestJson) : null;
            var languages = (manifest?.Languages != null && manifest.Languages.Count > 0) ? manifest.Languages : new List<string> { "zh-Hans", "en" };
            var tables = (manifest?.Tables != null && manifest.Tables.Count > 0) ? manifest.Tables : new List<string> { "core" };

            var data = new Dictionary<string, Dictionary<string, string>>();
            var fileHashes = new Dictionary<string, string>();
            var keyToTable = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (var lang in languages)
            {
                var merged = new Dictionary<string, string>();
                foreach (var table in tables)
                {
                    var file = Path.Combine(runtimeDir, $"{table}.{lang}.csv");
                    if (!File.Exists(file)) continue;
                    var text = File.ReadAllText(file, Encoding.UTF8);
                    fileHashes[$"{table}.{lang}"] = ComputeSha256(text);
                    foreach (var kv in LocalizationCsv.ParseKeyValueTable(text))
                    {
                        merged[kv.Key] = kv.Value;
                        if (!keyToTable.ContainsKey(kv.Key)) keyToTable[kv.Key] = table;
                    }
                }
                data[lang] = merged;
            }

            var allKeys = new SortedSet<string>();
            foreach (var lang in languages)
            {
                if (!data.TryGetValue(lang, out var dict) || dict == null) continue;
                foreach (var k in dict.Keys) allKeys.Add(k);
            }

            var exportCsvAbs = ProjectPathToAbsolute(_exportCsvPath);
            Directory.CreateDirectory(Path.GetDirectoryName(exportCsvAbs) ?? ".");

            var sb = new StringBuilder();
            sb.Append("table,key");
            for (var i = 0; i < languages.Count; i++)
            {
                sb.Append(",");
                sb.Append(EscapeCsv(languages[i]));
            }
            sb.AppendLine();

            foreach (var key in allKeys)
            {
                var table = keyToTable.TryGetValue(key, out var t) ? t : "core";
                sb.Append(EscapeCsv(table));
                sb.Append(",");
                sb.Append(EscapeCsv(key));
                for (var i = 0; i < languages.Count; i++)
                {
                    var lang = languages[i];
                    sb.Append(",");
                    if (data.TryGetValue(lang, out var dict) && dict != null && dict.TryGetValue(key, out var value))
                    {
                        sb.Append(EscapeCsv(value));
                    }
                    else
                    {
                        sb.Append(string.Empty);
                    }
                }
                sb.AppendLine();
            }

            File.WriteAllText(exportCsvAbs, sb.ToString(), new UTF8Encoding(true));

            var meta = new ExportMeta
            {
                Languages = languages,
                Tables = tables,
                RuntimeFileSha256 = fileHashes
            };

            var exportMetaAbs = ProjectPathToAbsolute(_exportMetaPath);
            Directory.CreateDirectory(Path.GetDirectoryName(exportMetaAbs) ?? ".");
            File.WriteAllText(exportMetaAbs, JsonUtility.ToJson(meta, true), new UTF8Encoding(true));

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Localization", $"已导出: {_exportCsvPath}", "OK");
        }

        private void Import()
        {
            var runtimeDir = ProjectPathToAbsolute(LocalizationDirRelative);
            if (!Directory.Exists(runtimeDir))
            {
                EditorUtility.DisplayDialog("Localization", "StreamingAssets/Localization 不存在", "OK");
                return;
            }

            var exportCsvAbs = ProjectPathToAbsolute(_exportCsvPath);
            if (!File.Exists(exportCsvAbs))
            {
                EditorUtility.DisplayDialog("Localization", $"找不到导入文件: {_exportCsvPath}", "OK");
                return;
            }

            var exportMetaAbs = ProjectPathToAbsolute(_exportMetaPath);
            var meta = File.Exists(exportMetaAbs) ? JsonUtility.FromJson<ExportMeta>(File.ReadAllText(exportMetaAbs, Encoding.UTF8)) : null;

            var lines = File.ReadAllLines(exportCsvAbs, Encoding.UTF8).Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();
            if (lines.Length < 2)
            {
                EditorUtility.DisplayDialog("Localization", "导入 CSV 内容为空", "OK");
                return;
            }

            var header = SplitCsv(lines[0]);
            if (header.Length < 2)
            {
                EditorUtility.DisplayDialog("Localization", "CSV 表头非法", "OK");
                return;
            }

            var hasTableColumn = string.Equals(header[0], "table", StringComparison.OrdinalIgnoreCase) &&
                                 header.Length >= 3 &&
                                 string.Equals(header[1], "key", StringComparison.OrdinalIgnoreCase);

            if (!hasTableColumn && !string.Equals(header[0], "key", StringComparison.OrdinalIgnoreCase))
            {
                EditorUtility.DisplayDialog("Localization", "CSV 表头必须以 key 开头，或以 table,key 开头", "OK");
                return;
            }

            var languageStartIndex = hasTableColumn ? 2 : 1;
            var languages = header.Skip(languageStartIndex).ToList();
            var translationsByTable = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>(StringComparer.Ordinal);

            for (var i = 1; i < lines.Length; i++)
            {
                var row = SplitCsv(lines[i]);
                if (row.Length < (hasTableColumn ? 2 : 1)) continue;

                var table = hasTableColumn ? row[0] : "core";
                var key = hasTableColumn ? row[1] : row[0];
                if (string.IsNullOrWhiteSpace(key)) continue;

                if (string.IsNullOrWhiteSpace(table)) table = "core";
                if (!translationsByTable.TryGetValue(table, out var perLang))
                {
                    perLang = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
                    translationsByTable[table] = perLang;
                }

                for (var col = 0; col < languages.Count; col++)
                {
                    var lang = languages[col];
                    if (!perLang.TryGetValue(lang, out var dict))
                    {
                        dict = new Dictionary<string, string>(StringComparer.Ordinal);
                        perLang[lang] = dict;
                    }

                    var valueIndex = languageStartIndex + col;
                    var value = valueIndex < row.Length ? row[valueIndex] : string.Empty;
                    dict[key] = value ?? string.Empty;
                }
            }

            var manifestFile = Path.Combine(runtimeDir, "manifest.json");
            var manifestJson = File.Exists(manifestFile) ? File.ReadAllText(manifestFile, Encoding.UTF8) : string.Empty;
            var manifest = !string.IsNullOrWhiteSpace(manifestJson) ? JsonUtility.FromJson<LocalizationManifest>(manifestJson) : new LocalizationManifest();
            if (manifest.Languages == null) manifest.Languages = new List<string>();
            if (manifest.Tables == null) manifest.Tables = new List<string>();

            foreach (var lang in languages)
            {
                if (!manifest.Languages.Contains(lang)) manifest.Languages.Add(lang);
            }

            if (!manifest.Tables.Contains("core")) manifest.Tables.Add("core");
            foreach (var table in translationsByTable.Keys)
            {
                if (!manifest.Tables.Contains(table)) manifest.Tables.Add(table);
            }

            var conflicts = new List<string>();
            if (meta != null && meta.RuntimeFileSha256 != null)
            {
                foreach (var kv in meta.RuntimeFileSha256)
                {
                    var runtimeFile = Path.Combine(runtimeDir, $"{kv.Key}.csv");
                    if (!File.Exists(runtimeFile)) continue;
                    var currentText = File.ReadAllText(runtimeFile, Encoding.UTF8);
                    var currentHash = ComputeSha256(currentText);
                    if (!string.Equals(currentHash, kv.Value, StringComparison.OrdinalIgnoreCase))
                    {
                        conflicts.Add(kv.Key);
                    }
                }
            }

            foreach (var table in translationsByTable)
            {
                foreach (var lang in languages)
                {
                    if (!table.Value.TryGetValue(lang, out var dict) || dict == null) continue;
                    var file = Path.Combine(runtimeDir, $"{table.Key}.{lang}.csv");
                    var csv = BuildKeyValueCsv(dict);
                    File.WriteAllText(file, csv, new UTF8Encoding(true));
                }
            }

            File.WriteAllText(manifestFile, JsonUtility.ToJson(manifest, true), new UTF8Encoding(true));

            if (conflicts.Count > 0)
            {
                var conflictFile = Path.Combine(runtimeDir, $"import.conflicts.txt");
                File.WriteAllText(conflictFile, string.Join("\n", conflicts), new UTF8Encoding(true));
            }

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Localization", "已导入并写回 runtime 表文件", "OK");
        }

        [Serializable]
        private class ExportMeta
        {
            public List<string> Languages = new List<string>();
            public List<string> Tables = new List<string>();
            public Dictionary<string, string> RuntimeFileSha256 = new Dictionary<string, string>();
        }

        private static string BuildKeyValueCsv(Dictionary<string, string> dict)
        {
            var sb = new StringBuilder();
            sb.AppendLine("key,value");
            foreach (var kv in dict.OrderBy(k => k.Key, StringComparer.Ordinal))
            {
                sb.Append(EscapeCsv(kv.Key));
                sb.Append(",");
                sb.Append(EscapeCsv(kv.Value ?? string.Empty));
                sb.AppendLine();
            }
            return sb.ToString();
        }

        private static string EscapeCsv(string value)
        {
            if (value == null) return string.Empty;
            var needsQuotes = value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r");
            if (!needsQuotes) return value;
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        private static string[] SplitCsv(string line)
        {
            var result = new List<string>();
            var current = new StringBuilder();
            var inQuotes = false;

            for (var i = 0; i < line.Length; i++)
            {
                var c = line[i];
                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                        continue;
                    }
                    inQuotes = !inQuotes;
                    continue;
                }

                if (c == ',' && !inQuotes)
                {
                    result.Add(current.ToString());
                    current.Clear();
                    continue;
                }
                current.Append(c);
            }

            result.Add(current.ToString());
            return result.ToArray();
        }

        private static string ComputeSha256(string text)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(text ?? string.Empty);
                var hash = sha.ComputeHash(bytes);
                var sb = new StringBuilder(hash.Length * 2);
                for (var i = 0; i < hash.Length; i++) sb.Append(hash[i].ToString("x2"));
                return sb.ToString();
            }
        }
    }
}
