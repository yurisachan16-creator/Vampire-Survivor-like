using System.Collections.Generic;
using QFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VampireSurvivorLike
{
    /// <summary>
    /// 字体管理器：语言切换时自动为所有已注册的 Text/TMP_Text 应用对应语言的字体。
    /// 
    /// 【重要】为避免跨语言文字显示为方块（如简体中文字体下显示日文/韩文），
    /// 请在 LocalizationFontCatalog.asset 中为每个语言条目的 TmpFallbackFonts 添加
    /// 其他语言的 TMP 字体作为 fallback（尤其是 zh-Hans、zh-Hant、en 条目需要添加
    /// 日文和韩文字体 fallback，因为语言选择下拉列表始终显示所有语言的原生名称）。
    /// </summary>
    public static class FontManager
    {
        private static LocalizationFontCatalog _catalog;
        private static bool _initialized;

        private static readonly List<Text> UguiTexts = new List<Text>();
        private static readonly List<TMP_Text> TmpTexts = new List<TMP_Text>();

        // 记录每个 TMP_FontAsset 的原始 fallback 列表，防止反复追加导致资产引用泄漏
        private static readonly Dictionary<TMP_FontAsset, List<TMP_FontAsset>> OriginalFallbacks =
            new Dictionary<TMP_FontAsset, List<TMP_FontAsset>>();

        public static LocalizationFontCatalog Catalog
        {
            get
            {
                if (_catalog == null)
                {
                    _catalog = Resources.Load<LocalizationFontCatalog>("LocalizationFontCatalog");
                }

                return _catalog;
            }
            set => _catalog = value;
        }

        public static void Initialize()
        {
            if (_initialized) return;
            _initialized = true;

            LocalizationManager.CurrentLanguage.Register(_ => ApplyAll()).UnRegisterWhenGameObjectDestroyed(LocalizationRunner.Instance.gameObject);
            LocalizationManager.ReadyChanged.Register(ApplyAll).UnRegisterWhenGameObjectDestroyed(LocalizationRunner.Instance.gameObject);
        }

        public static void Register(Text text)
        {
            if (!text) return;
            Initialize();
            UguiTexts.Add(text);
            ApplyTo(text);
        }

        public static void Register(TMP_Text text)
        {
            if (!text) return;
            Initialize();
            TmpTexts.Add(text);
            ApplyTo(text);
        }

        /// <summary>
        /// 强制对所有已注册的文本重新应用字体（用于首次打开面板时，确保 dropdown 等组件显示正确）
        /// </summary>
        public static void ApplyAllRegistered()
        {
            ApplyAll();
        }

        private static void ApplyAll()
        {
            for (var i = UguiTexts.Count - 1; i >= 0; i--)
            {
                var t = UguiTexts[i];
                if (!t)
                {
                    UguiTexts.RemoveAt(i);
                    continue;
                }
                ApplyTo(t);
            }

            for (var i = TmpTexts.Count - 1; i >= 0; i--)
            {
                var t = TmpTexts[i];
                if (!t)
                {
                    TmpTexts.RemoveAt(i);
                    continue;
                }
                ApplyTo(t);
            }
        }

        private static void ApplyTo(Text text)
        {
            var catalog = Catalog;
            if (!catalog) return;

            if (catalog.TryGet(LocalizationManager.CurrentLanguage.Value, out var entry) && entry != null && entry.UguiFont)
            {
                text.font = entry.UguiFont;
            }
        }

        private static void ApplyTo(TMP_Text text)
        {
            var catalog = Catalog;
            if (!catalog) return;

            if (catalog.TryGet(LocalizationManager.CurrentLanguage.Value, out var entry) && entry != null)
            {
                if (entry.TmpFont && IsValidTmpFont(entry.TmpFont)) text.font = entry.TmpFont;
                if (text.font && entry.TmpFallbackFonts != null && entry.TmpFallbackFonts.Count > 0)
                {
                    var font = text.font;

                    // 首次遇到此字体时，保存其原始 fallback 列表
                    if (!OriginalFallbacks.ContainsKey(font))
                    {
                        OriginalFallbacks[font] = font.fallbackFontAssetTable != null
                            ? new List<TMP_FontAsset>(font.fallbackFontAssetTable)
                            : new List<TMP_FontAsset>();
                    }

                    // 恢复为原始 fallback 列表，再追加当前语言需要的 fallback
                    font.fallbackFontAssetTable = new List<TMP_FontAsset>(OriginalFallbacks[font]);
                    for (var i = 0; i < entry.TmpFallbackFonts.Count; i++)
                    {
                        var fallback = entry.TmpFallbackFonts[i];
                        if (!fallback || !IsValidTmpFont(fallback)) continue;
                        if (!font.fallbackFontAssetTable.Contains(fallback))
                        {
                            font.fallbackFontAssetTable.Add(fallback);
                        }
                    }
                }
            }
        }

        private static bool IsValidTmpFont(TMP_FontAsset font)
        {
            if (!font) return false;
            var atlas = font.atlasTextures;
            if (atlas == null || atlas.Length == 0) return false;
            for (var i = 0; i < atlas.Length; i++)
            {
                if (!atlas[i]) return false;
            }
            return true;
        }
    }
}
