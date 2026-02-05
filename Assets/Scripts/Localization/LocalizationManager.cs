using System;
using System.Collections.Generic;
using QFramework;
using UnityEngine;

namespace VampireSurvivorLike
{
    public static class LocalizationManager
    {
        private static LocalizationSettings _settings;
        private static bool _initialized;
        private static LocalizationManifest _manifest;
        private static Dictionary<string, string> _mergedTable = new Dictionary<string, string>();
        private static bool _ready;
        private static ILocalizationProvider _provider;
        private static ILocalizedAssetResolver _assetResolver;
        private static ILocaleFormatter _formatter;
        private static readonly Dictionary<string, LanguageCache> CacheByLanguage = new Dictionary<string, LanguageCache>();
        private static readonly LinkedList<string> CacheLru = new LinkedList<string>();
        private const int MaxCachedLanguages = 2;

        private sealed class LanguageCache
        {
            public readonly Dictionary<string, string> Merged = new Dictionary<string, string>();
            public readonly HashSet<string> LoadedTables = new HashSet<string>(StringComparer.Ordinal);
            public bool Ready;
        }

        public static LocalizationSettings Settings
        {
            get
            {
                if (_settings == null)
                {
                    _settings = Resources.Load<LocalizationSettings>("LocalizationSettings") ?? LocalizationSettings.CreateDefaultInstance();
                }

                return _settings;
            }
        }

        public static BindableProperty<LanguageId> CurrentLanguage { get; } = new BindableProperty<LanguageId>(LanguageId.ZhHans);

        public static EasyEvent<LanguageId> LanguageChanged { get; } = new EasyEvent<LanguageId>();
        public static EasyEvent ReadyChanged { get; } = new EasyEvent();

        public static bool IsReady => _ready;

        public static ILocalizationProvider Provider
        {
            get => _provider ??= new StreamingAssetsLocalizationProvider();
            set => _provider = value;
        }

        public static ILocalizedAssetResolver AssetResolver
        {
            get => _assetResolver ??= new SuffixLocalizedAssetResolver();
            set => _assetResolver = value;
        }

        public static ILocaleFormatter Formatter
        {
            get => _formatter ??= new CultureLocaleFormatter();
            set => _formatter = value;
        }

        public static void Initialize()
        {
            if (_initialized) return;
            _initialized = true;

            var loaded = LoadStoredLanguage();
            var detected = DetectSystemLanguage();
            var desired = !loaded.IsEmpty ? loaded : detected;

            if (!Settings.IsSupported(desired))
            {
                desired = Settings.DefaultLanguage;
            }

            CurrentLanguage.Value = desired;
            SaveLanguage(desired);

            LocalizationRunner.Instance.StartCoroutine(ActivateLanguage(desired));

            CurrentLanguage.Register(lang =>
            {
                SaveLanguage(lang);
                _ready = false;
                ReadyChanged.Trigger();
                LocalizationRunner.Instance.StartCoroutine(ActivateLanguage(lang));
                LanguageChanged.Trigger(lang);
            });
        }

        public static void ChangeLanguage(LanguageId language)
        {
            Initialize();

            var desired = language;
            if (!Settings.IsSupported(desired))
            {
                desired = Settings.DefaultLanguage;
            }

            if (CurrentLanguage.Value == desired) return;
            CurrentLanguage.Value = desired;
        }

        public static LanguageId DetectSystemLanguage()
        {
            var lang = Application.systemLanguage;
            return lang switch
            {
                SystemLanguage.ChineseSimplified => LanguageId.ZhHans,
                SystemLanguage.ChineseTraditional => LanguageId.ZhHant,
                SystemLanguage.Chinese => LanguageId.ZhHans,
                SystemLanguage.English => LanguageId.En,
                _ => Settings.DefaultLanguage
            };
        }

        private static LanguageId LoadStoredLanguage()
        {
            var key = Settings.PlayerPrefsLanguageKey;
            var value = PlayerPrefs.GetString(key, string.Empty);
            if (string.IsNullOrWhiteSpace(value)) return default;
            return new LanguageId(value);
        }

        private static void SaveLanguage(LanguageId language)
        {
            var key = Settings.PlayerPrefsLanguageKey;
            PlayerPrefs.SetString(key, language.ToString());
            PlayerPrefs.Save();
        }

        public static string T(string key)
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;
            if (_mergedTable != null && _mergedTable.TryGetValue(key, out var value))
            {
                return PostProcess(value);
            }

            LocalizationDebug.MissingKeys.Add(key);
            return key;
        }

        public static bool TryGet(string key, out string value)
        {
            value = string.Empty;
            if (string.IsNullOrEmpty(key)) return false;
            if (_mergedTable != null && _mergedTable.TryGetValue(key, out var v))
            {
                value = PostProcess(v);
                return true;
            }
            LocalizationDebug.MissingKeys.Add(key);
            return false;
        }

        public static string Format(string key, params object[] args)
        {
            var text = T(key);
            if (args == null || args.Length == 0) return text;
            return string.Format(text, args);
        }

        private static string PostProcess(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            return value.Replace("\\n", "\n").Replace("\\t", "\t");
        }

        public static void PreloadTable(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName)) return;
            Initialize();
            LocalizationRunner.Instance.StartCoroutine(LoadTableForLanguage(tableName, CurrentLanguage.Value));
        }

        public static void ClearCachedLanguages()
        {
            CacheByLanguage.Clear();
            CacheLru.Clear();
        }

        private static System.Collections.IEnumerator ActivateLanguage(LanguageId language)
        {
            var langKey = (language.IsEmpty ? Settings.DefaultLanguage.ToString() : language.ToString()).ToLowerInvariant();

            if (CacheByLanguage.TryGetValue(langKey, out var cache) && cache != null && cache.Ready)
            {
                TouchCache(langKey);
                _mergedTable = cache.Merged;
                _ready = true;
                ReadyChanged.Trigger();
                yield break;
            }

            TouchCache(langKey);
            cache = CacheByLanguage[langKey];
            _mergedTable = cache.Merged;

            yield return EnsureManifestLoaded();
            yield return LoadTableForLanguage("core", language);

            _ready = cache.Ready;
            ReadyChanged.Trigger();
        }

        private static void TouchCache(string langKey)
        {
            if (!CacheByLanguage.TryGetValue(langKey, out var cache) || cache == null)
            {
                cache = new LanguageCache();
                CacheByLanguage[langKey] = cache;
            }

            var node = CacheLru.Find(langKey);
            if (node != null) CacheLru.Remove(node);
            CacheLru.AddFirst(langKey);

            while (CacheLru.Count > MaxCachedLanguages)
            {
                var last = CacheLru.Last;
                if (last == null) break;
                var removeKey = last.Value;
                CacheLru.RemoveLast();
                CacheByLanguage.Remove(removeKey);
            }
        }

        private static LanguageCache GetCurrentCache(LanguageId language)
        {
            var key = (language.IsEmpty ? Settings.DefaultLanguage.ToString() : language.ToString()).ToLowerInvariant();
            TouchCache(key);
            return CacheByLanguage[key];
        }

        private static System.Collections.IEnumerator EnsureManifestLoaded()
        {
            if (_manifest != null) yield break;

            var provider = Provider;
            LocalizationManifest manifest = null;
            yield return provider.LoadManifest(m => manifest = m);
            _manifest = manifest;
        }

        private static System.Collections.IEnumerator LoadTableForLanguage(string tableName, LanguageId language)
        {
            var cache = GetCurrentCache(language);
            if (cache.LoadedTables.Contains(tableName)) yield break;

            var provider = Provider;
            Dictionary<string, string> table = null;
            yield return provider.LoadTable(tableName, language, t => table = t);
            table ??= new Dictionary<string, string>();

            foreach (var kv in table)
            {
                cache.Merged[kv.Key] = kv.Value;
            }

            cache.LoadedTables.Add(tableName);
            if (tableName == "core") cache.Ready = true;

            if (language == CurrentLanguage.Value)
            {
                _mergedTable = cache.Merged;
                _ready = cache.Ready;
                ReadyChanged.Trigger();
            }
        }
    }
}
