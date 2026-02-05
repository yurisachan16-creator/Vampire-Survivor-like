using System.Collections.Generic;
using QFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VampireSurvivorLike
{
    public static class FontManager
    {
        private static LocalizationFontCatalog _catalog;
        private static bool _initialized;

        private static readonly List<Text> UguiTexts = new List<Text>();
        private static readonly List<TMP_Text> TmpTexts = new List<TMP_Text>();

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
                if (entry.TmpFont) text.font = entry.TmpFont;
                if (entry.TmpFallbackFonts != null && entry.TmpFallbackFonts.Count > 0)
                {
                    for (var i = 0; i < entry.TmpFallbackFonts.Count; i++)
                    {
                        var fallback = entry.TmpFallbackFonts[i];
                        if (!fallback) continue;
                        if (text.font && !text.font.fallbackFontAssetTable.Contains(fallback))
                        {
                            text.font.fallbackFontAssetTable.Add(fallback);
                        }
                    }
                }
            }
        }
    }
}
