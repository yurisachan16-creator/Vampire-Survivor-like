using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;
using VampireSurvivorLike;

namespace VampireSurvivorLike.EditorTools
{
    public static class UnityLocalizationImporter
    {
        private const string RuntimeLocalizationDir = "Assets/StreamingAssets/Localization";
        private const string OutputDir = "Assets/Localization";
        private const string StringTablesDir = OutputDir + "/StringTables";
        private const string LocalesDir = OutputDir + "/Locales";

        [MenuItem("VampireSurvivorLike/Unity Localization/Import StreamingAssets CSV")]
        public static void ImportStreamingAssetsCsv()
        {
            var runtimeDirAbs = ToAbsolutePath(RuntimeLocalizationDir);
            if (!Directory.Exists(runtimeDirAbs))
            {
                EditorUtility.DisplayDialog("Unity Localization", "找不到 Assets/StreamingAssets/Localization", "OK");
                return;
            }

            var manifest = LoadManifest(runtimeDirAbs);
            if (manifest == null)
            {
                EditorUtility.DisplayDialog("Unity Localization", "manifest.json 解析失败", "OK");
                return;
            }

            if (manifest.Tables == null || manifest.Tables.Count == 0)
            {
                EditorUtility.DisplayDialog("Unity Localization", "manifest.json 未包含 Tables", "OK");
                return;
            }

            if (manifest.Languages == null || manifest.Languages.Count == 0)
            {
                EditorUtility.DisplayDialog("Unity Localization", "manifest.json 未包含 Languages", "OK");
                return;
            }

            EnsureFolders();

            var localesByCode = new Dictionary<string, Locale>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < manifest.Languages.Count; i++)
            {
                var code = manifest.Languages[i];
                if (string.IsNullOrWhiteSpace(code)) continue;
                var locale = EnsureLocale(code.Trim());
                if (locale != null) localesByCode[code.Trim()] = locale;
            }

            if (localesByCode.Count == 0)
            {
                EditorUtility.DisplayDialog("Unity Localization", "没有可用语言（Locale 创建失败）", "OK");
                return;
            }

            var tableCollections = new Dictionary<string, StringTableCollection>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < manifest.Tables.Count; i++)
            {
                var tableName = manifest.Tables[i];
                if (string.IsNullOrWhiteSpace(tableName)) continue;
                tableName = tableName.Trim();

                var collection = LocalizationEditorSettings.GetStringTableCollection(tableName);
                if (collection == null)
                {
                    collection = LocalizationEditorSettings.CreateStringTableCollection(tableName, StringTablesDir);
                }

                tableCollections[tableName] = collection;
            }

            var importedEntryCount = 0;
            var importedFileCount = 0;

            foreach (var tableName in tableCollections.Keys)
            {
                foreach (var langCode in localesByCode.Keys)
                {
                    var filePath = Path.Combine(runtimeDirAbs, $"{tableName}.{langCode}.csv");
                    if (!File.Exists(filePath)) continue;
                    importedFileCount++;

                    var csvText = File.ReadAllText(filePath, Encoding.UTF8);
                    var kvs = LocalizationCsv.ParseKeyValueTable(csvText);

                    var collection = tableCollections[tableName];
                    var locale = localesByCode[langCode];

                    var stringTable = collection.GetTable(locale.Identifier) as StringTable;
                    if (stringTable == null)
                    {
                        stringTable = collection.AddNewTable(locale.Identifier) as StringTable;
                    }

                    if (stringTable == null) continue;

                    foreach (var kv in kvs)
                    {
                        if (string.IsNullOrWhiteSpace(kv.Key)) continue;
                        var entry = stringTable.GetEntry(kv.Key);
                        if (entry == null)
                        {
                            stringTable.AddEntry(kv.Key, kv.Value ?? string.Empty);
                        }
                        else
                        {
                            entry.Value = kv.Value ?? string.Empty;
                        }
                        importedEntryCount++;
                    }

                    EditorUtility.SetDirty(stringTable);
                    EditorUtility.SetDirty(stringTable.SharedData);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Unity Localization",
                $"导入完成\\n文件: {importedFileCount}\\n条目(累计写入): {importedEntryCount}\\n输出目录: {OutputDir}",
                "OK");
        }

        private static void EnsureFolders()
        {
            Directory.CreateDirectory(ToAbsolutePath(OutputDir));
            Directory.CreateDirectory(ToAbsolutePath(StringTablesDir));
            Directory.CreateDirectory(ToAbsolutePath(LocalesDir));
            AssetDatabase.Refresh();
        }

        private static Locale EnsureLocale(string code)
        {
            var locale = LocalizationEditorSettings.GetLocale(code);
            if (locale != null) return locale;

            locale = Locale.CreateLocale(code);
            if (locale == null) return null;

            var assetPath = $"{LocalesDir}/{code}.asset";
            if (!File.Exists(ToAbsolutePath(assetPath)))
            {
                AssetDatabase.CreateAsset(locale, assetPath);
            }

            LocalizationEditorSettings.AddLocale(locale);
            EditorUtility.SetDirty(locale);
            return locale;
        }

        private static LocalizationManifest LoadManifest(string runtimeDirAbs)
        {
            try
            {
                var file = Path.Combine(runtimeDirAbs, "manifest.json");
                if (!File.Exists(file)) return new LocalizationManifest();
                var json = File.ReadAllText(file, Encoding.UTF8);
                if (string.IsNullOrWhiteSpace(json)) return new LocalizationManifest();
                return JsonUtility.FromJson<LocalizationManifest>(json);
            }
            catch
            {
                return null;
            }
        }

        private static string ToAbsolutePath(string projectRelativePath)
        {
            var root = Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath;
            return Path.GetFullPath(Path.Combine(root, projectRelativePath));
        }
    }
}

