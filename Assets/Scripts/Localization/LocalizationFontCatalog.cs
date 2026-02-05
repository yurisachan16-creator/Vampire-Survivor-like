using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace VampireSurvivorLike
{
    [CreateAssetMenu(menuName = "VampireSurvivorLike/Localization Font Catalog", fileName = "LocalizationFontCatalog")]
    public class LocalizationFontCatalog : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            public string LanguageCode;
            public Font UguiFont;
            public TMP_FontAsset TmpFont;
            public List<TMP_FontAsset> TmpFallbackFonts = new List<TMP_FontAsset>();
        }

        [SerializeField] public List<Entry> Entries = new List<Entry>();

        public bool TryGet(LanguageId language, out Entry entry)
        {
            entry = null;
            if (Entries == null) return false;
            var code = (language.IsEmpty ? string.Empty : language.ToString()) ?? string.Empty;
            for (var i = 0; i < Entries.Count; i++)
            {
                var e = Entries[i];
                if (e == null) continue;
                if (string.Equals(e.LanguageCode, code, StringComparison.OrdinalIgnoreCase))
                {
                    entry = e;
                    return true;
                }
            }
            return false;
        }
    }
}
