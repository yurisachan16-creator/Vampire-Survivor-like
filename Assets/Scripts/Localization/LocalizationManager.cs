using System;
using System.Collections;
using System.Collections.Generic;
using QFramework;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;
using UnityLocalizationSettings = UnityEngine.Localization.Settings.LocalizationSettings;

namespace VampireSurvivorLike
{
    public static class LocalizationManager
    {
        private static LocalizationSettings _settings;
        private static bool _initialized;
        private static bool _ready;
        private static bool _hookedUnityEvents;
        private static bool _reloadRunning;
        private static readonly Dictionary<string, StringTable> LoadedStringTables = new Dictionary<string, StringTable>(StringComparer.OrdinalIgnoreCase);
        private static readonly List<string> PreloadedTableList = new List<string>();

        private static ILocalizedAssetResolver _assetResolver;
        private static ILocaleFormatter _formatter;

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

            HookUnityLocalizationEvents();

            LocalizationRunner.Instance.StartCoroutine(InitializeAndSelectLocale());
        }

        public static void ChangeLanguage(LanguageId language)
        {
            Initialize();
            LocalizationRunner.Instance.StartCoroutine(SetSelectedLocaleAfterInit(language));
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

        public static string T(string key)
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;
            Initialize();

            if (!TryGetInternal(key, out var value))
            {
                if (_ready) LocalizationDebug.MissingKeys.Add(key);
                return key;
            }

            return PostProcess(value);
        }

        public static bool TryGet(string key, out string value)
        {
            value = string.Empty;
            if (string.IsNullOrEmpty(key)) return false;
            Initialize();

            if (!TryGetInternal(key, out var v))
            {
                if (_ready) LocalizationDebug.MissingKeys.Add(key);
                return false;
            }

            value = PostProcess(v);
            return true;
        }

        public static string Format(string key, params object[] args)
        {
            var text = T(key);
            if (args == null || args.Length == 0) return text;
            return string.Format(text, args);
        }

        public static void PreloadTable(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName)) return;
            Initialize();

            tableName = tableName.Trim();
            if (string.Equals(tableName, "core", StringComparison.OrdinalIgnoreCase)) return;

            if (!PreloadedTableList.Contains(tableName))
            {
                PreloadedTableList.Add(tableName);
            }

            TriggerReload();
        }

        public static void ClearCachedLanguages()
        {
            LoadedStringTables.Clear();
        }

        private static void HookUnityLocalizationEvents()
        {
            if (_hookedUnityEvents) return;
            _hookedUnityEvents = true;
        }

        private static IEnumerator InitializeAndSelectLocale()
        {
            yield return UnityLocalizationSettings.InitializationOperation;

            var loaded = LoadStoredLanguage();
            var detected = DetectSystemLanguage();
            var desired = !loaded.IsEmpty ? loaded : detected;

            if (!Settings.IsSupported(desired))
            {
                desired = Settings.DefaultLanguage;
            }

            yield return SetSelectedLocaleAfterInit(desired);
        }

        private static IEnumerator SetSelectedLocaleAfterInit(LanguageId language)
        {
            yield return UnityLocalizationSettings.InitializationOperation;

            var locale = FindLocale(language);
            if (locale == null)
            {
                locale = FindLocale(Settings.DefaultLanguage) ?? UnityLocalizationSettings.SelectedLocale;
            }

            if (locale == null)
            {
                SetNotReady();
                yield break;
            }

            var current = UnityLocalizationSettings.SelectedLocale;
            if (current != null && current == locale)
            {
                OnSelectedLocaleChanged(locale);
                yield break;
            }

            SetNotReady();
            UnityLocalizationSettings.SelectedLocale = locale;
            OnSelectedLocaleChanged(locale);
        }

        private static void OnSelectedLocaleChanged(Locale locale)
        {
            if (locale == null) return;
            var lang = new LanguageId(locale.Identifier.Code);

            CurrentLanguage.Value = lang;
            SaveLanguage(lang);
            LanguageChanged.Trigger(lang);

            TriggerReload();
        }

        private static void TriggerReload()
        {
            if (_reloadRunning) return;
            _reloadRunning = true;
            LocalizationRunner.Instance.StartCoroutine(ReloadTables());
        }

        private static IEnumerator ReloadTables()
        {
            SetNotReady();

            LoadedStringTables.Clear();
            var tableOrder = BuildTableOrder();

            for (var i = 0; i < tableOrder.Count; i++)
            {
                var tableName = tableOrder[i];
                var op = UnityLocalizationSettings.StringDatabase.GetTableAsync(tableName);
                yield return op;
                var table = op.Result as StringTable;
                if (table != null)
                {
                    LoadedStringTables[tableName] = table;
                }
            }

            _ready = true;
            _reloadRunning = false;
            ReadyChanged.Trigger();
        }

        private static void SetNotReady()
        {
            _ready = false;
            ReadyChanged.Trigger();
        }

        private static List<string> BuildTableOrder()
        {
            var order = new List<string> { "core" };
            for (var i = 0; i < PreloadedTableList.Count; i++)
            {
                var name = PreloadedTableList[i];
                if (string.IsNullOrWhiteSpace(name)) continue;
                if (string.Equals(name, "core", StringComparison.OrdinalIgnoreCase)) continue;
                if (!order.Contains(name)) order.Add(name);
            }

            return order;
        }

        private static bool TryGetInternal(string key, out string value)
        {
            value = string.Empty;
            if (!IsReady) return false;

            var order = BuildTableOrder();
            for (var i = order.Count - 1; i >= 0; i--)
            {
                var tableName = order[i];
                if (!LoadedStringTables.TryGetValue(tableName, out var table) || table == null) continue;
                var entry = table.GetEntry(key);
                if (entry == null) continue;
                value = entry.LocalizedValue ?? string.Empty;
                return !string.IsNullOrEmpty(value);
            }

            return false;
        }

        private static Locale FindLocale(LanguageId language)
        {
            var code = (language.IsEmpty ? string.Empty : language.ToString()) ?? string.Empty;
            if (string.IsNullOrWhiteSpace(code)) return null;

            var locales = UnityLocalizationSettings.AvailableLocales;
            if (locales == null || locales.Locales == null) return null;

            for (var i = 0; i < locales.Locales.Count; i++)
            {
                var locale = locales.Locales[i];
                if (locale == null) continue;
                if (string.Equals(locale.Identifier.Code, code, StringComparison.OrdinalIgnoreCase)) return locale;
            }

            return null;
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

        private static string PostProcess(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            return value.Replace("\\n", "\n").Replace("\\t", "\t");
        }
    }
}
