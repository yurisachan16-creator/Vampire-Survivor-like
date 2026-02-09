using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace VampireSurvivorLike.EditorTools
{
    /// <summary>
    /// TMP 字体字符覆盖验证工具
    /// 扫描所有本地化文本和硬编码 UI 文本，对比 TMP 字体资源，报告缺失字符
    /// </summary>
    public static class FontCharacterValidator
    {
        /// <summary>
        /// 硬编码在 UI 中的必须字符（语言名称、分辨率文本等）
        /// </summary>
        private static readonly string[] HardcodedTexts = new[]
        {
            "简体中文",
            "繁體中文",
            "English",
            "日本語",
            "한국어",
            "Français",
            "Deutsch",
            "Español",
            "自动检测",
            "分辨率",
            "调试HUD",
            "1920×1080",
            "2160×1080",
            "2340×1080",
            "2400×1080",
            "2560×1080",
            "1280×720",
            "2560×1440",
            "★",
        };

        [MenuItem("VampireSurvivorLike/Validate Font Character Coverage")]
        public static void Validate()
        {
            // 查找 TMP 字体资源
            var fontGuids = AssetDatabase.FindAssets("t:TMP_FontAsset", new[] { "Assets/Art/Font" });
            if (fontGuids.Length == 0)
            {
                EditorUtility.DisplayDialog("Font Validator", "未找到 TMP 字体资源（Assets/Art/Font）", "OK");
                return;
            }

            var fontPath = AssetDatabase.GUIDToAssetPath(fontGuids[0]);
            var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath);
            if (fontAsset == null)
            {
                EditorUtility.DisplayDialog("Font Validator", $"无法加载字体资源: {fontPath}", "OK");
                return;
            }

            // 收集字体中已有的字符
            var fontChars = new HashSet<uint>();
            if (fontAsset.characterTable != null)
            {
                foreach (var ch in fontAsset.characterTable)
                {
                    fontChars.Add(ch.unicode);
                }
            }

            // 收集所有需要的字符
            var requiredChars = new HashSet<char>();

            // 1. 从 StreamingAssets CSV 文件收集
            var csvDir = Path.Combine(Application.streamingAssetsPath, "Localization");
            if (Directory.Exists(csvDir))
            {
                var csvFiles = Directory.GetFiles(csvDir, "*.csv");
                foreach (var csvFile in csvFiles)
                {
                    var content = File.ReadAllText(csvFile, Encoding.UTF8);
                    foreach (var ch in content)
                    {
                        if (ch != '\r' && ch != '\n')
                            requiredChars.Add(ch);
                    }
                }
            }

            // 2. 从硬编码文本收集
            foreach (var text in HardcodedTexts)
            {
                foreach (var ch in text)
                {
                    requiredChars.Add(ch);
                }
            }

            // 3. 对比并报告缺失
            var missing = new List<char>();
            var missingDetails = new StringBuilder();
            foreach (var ch in requiredChars)
            {
                // 跳过 ASCII 控制字符和空格
                if (ch <= 32) continue;
                // 只检查非 ASCII 字符（ASCII 字符通常都在字体中）
                if (ch <= 126) continue;

                if (!fontChars.Contains(ch))
                {
                    missing.Add(ch);
                }
            }

            missing.Sort();

            if (missing.Count == 0)
            {
                EditorUtility.DisplayDialog("Font Validator",
                    $"✅ 字体 '{fontAsset.name}' 覆盖了所有必需的 {requiredChars.Count} 个字符。\n无缺失字符。",
                    "OK");
            }
            else
            {
                missingDetails.AppendLine($"⚠️ 字体 '{fontAsset.name}' 缺失 {missing.Count} 个字符：\n");
                foreach (var ch in missing)
                {
                    missingDetails.AppendLine($"  '{ch}' (U+{((int)ch):X4})");
                }
                missingDetails.AppendLine();
                missingDetails.AppendLine("修复步骤：");
                missingDetails.AppendLine("1. 运行菜单: VampireSurvivorLike → Unity Localization → Export Character Set");
                missingDetails.AppendLine("2. 打开 Window → TextMeshPro → Font Asset Creator");
                missingDetails.AppendLine($"3. Source Font: {fontAsset.name} 对应的 TTF 文件");
                missingDetails.AppendLine("4. Character Set: Characters from File");
                missingDetails.AppendLine("5. 选择 Assets/Localization/Generated/FontCharacters.txt");
                missingDetails.AppendLine("6. 点击 Generate Font Atlas → Save 覆盖现有字体资源");

                Debug.LogWarning(missingDetails.ToString());
                EditorUtility.DisplayDialog("Font Validator",
                    $"⚠️ 缺失 {missing.Count} 个字符。详情已输出到 Console。\n\n缺失示例：{string.Join("", missing.GetRange(0, Mathf.Min(20, missing.Count)))}",
                    "OK");
            }
        }
    }
}
