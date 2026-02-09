using System;
using System.Collections.Generic;
using UnityEngine;

namespace VampireSurvivorLike
{
    [CreateAssetMenu(menuName = "VampireSurvivorLike/Localization Settings", fileName = "LocalizationSettings")]
    public class LocalizationSettings : ScriptableObject
    {
        [SerializeField] public string PlayerPrefsLanguageKey = "Localization.Language";
        [SerializeField] public LanguageId DefaultLanguage = new LanguageId("zh-Hans");
        [SerializeField] public List<LanguageId> SupportedLanguages = new List<LanguageId> { new LanguageId("zh-Hans"), new LanguageId("zh-Hant"), new LanguageId("en"), new LanguageId("ja"), new LanguageId("ko") };
        [SerializeField] public List<LanguageId> FallbackChain = new List<LanguageId> { new LanguageId("zh-Hans") };

        public bool IsSupported(LanguageId language)
        {
            if (SupportedLanguages == null) return false;
            for (var i = 0; i < SupportedLanguages.Count; i++)
            {
                if (SupportedLanguages[i] == language) return true;
            }
            return false;
        }

        public IEnumerable<LanguageId> EnumerateFallbacks(LanguageId requested)
        {
            if (!requested.IsEmpty) yield return requested;

            if (FallbackChain != null)
            {
                for (var i = 0; i < FallbackChain.Count; i++)
                {
                    var lang = FallbackChain[i];
                    if (lang.IsEmpty) continue;
                    if (lang == requested) continue;
                    yield return lang;
                }
            }

            if (!DefaultLanguage.IsEmpty && DefaultLanguage != requested)
            {
                var alreadyInChain = false;
                if (FallbackChain != null)
                {
                    for (var i = 0; i < FallbackChain.Count; i++)
                    {
                        if (FallbackChain[i] == DefaultLanguage)
                        {
                            alreadyInChain = true;
                            break;
                        }
                    }
                }

                if (!alreadyInChain) yield return DefaultLanguage;
            }
        }

        public static LocalizationSettings CreateDefaultInstance()
        {
            var instance = CreateInstance<LocalizationSettings>();
            instance.PlayerPrefsLanguageKey = "Localization.Language";
            instance.DefaultLanguage = LanguageId.ZhHans;
            instance.SupportedLanguages = new List<LanguageId> { LanguageId.ZhHans, LanguageId.ZhHant, LanguageId.En, LanguageId.Ja, LanguageId.Ko };
            instance.FallbackChain = new List<LanguageId> { LanguageId.ZhHans };
            return instance;
        }
    }
}
