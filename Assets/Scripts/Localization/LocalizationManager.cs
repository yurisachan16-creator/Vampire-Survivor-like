using System;
using System.Collections;
using System.Collections.Generic;
using QFramework;
using UnityEngine;

namespace VampireSurvivorLike
{
    public static class LocalizationManager
    {
        private static LocalizationSettings _settings;
        private static bool _initialized;
        private static bool _ready;
        private static bool _reloadRunning;
        private static bool _pendingReload;
        private static Coroutine _reloadCoroutine;
        private static readonly Dictionary<string, Dictionary<string, string>> LoadedTables = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        private static readonly List<string> PreloadedTableList = new List<string>();

        private static ILocalizationProvider _provider;
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

            LocalizationRunner.Instance.StartCoroutine(InitializeAndSelectLocale());
        }

        public static void ChangeLanguage(LanguageId language)
        {
            Initialize();
            LocalizationRunner.Instance.StartCoroutine(ApplyLanguage(language));
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
                SystemLanguage.Japanese => LanguageId.Ja,
                SystemLanguage.Korean => LanguageId.Ko,
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
                TriggerReload();
            }
        }

        public static void ClearCachedLanguages()
        {
            LoadedTables.Clear();
        }

        private static IEnumerator InitializeAndSelectLocale()
        {
            var loaded = LoadStoredLanguage();
            var detected = DetectSystemLanguage();
            var desired = !loaded.IsEmpty ? loaded : detected;

            if (!Settings.IsSupported(desired))
            {
                desired = Settings.DefaultLanguage;
            }

            yield return ApplyLanguage(desired);
        }

        private static IEnumerator ApplyLanguage(LanguageId language)
        {
            if (!Settings.IsSupported(language))
            {
                language = Settings.DefaultLanguage;
            }

            var changed = CurrentLanguage.Value != language;
            CurrentLanguage.Value = language;
            SaveLanguage(language);

            if (changed)
            {
                LanguageChanged.Trigger(language);
            }

            yield return StartReload();
        }

        private static void TriggerReload()
        {
            if (_reloadRunning)
            {
                _pendingReload = true;
                return;
            }
            StartReload();
        }

        private static Coroutine StartReload()
        {
            if (_reloadCoroutine != null)
            {
                LocalizationRunner.Instance.StopCoroutine(_reloadCoroutine);
            }
            _reloadRunning = true;
            _reloadCoroutine = LocalizationRunner.Instance.StartCoroutine(ReloadTables());
            return _reloadCoroutine;
        }

        private static IEnumerator ReloadTables()
        {
            // 标记为未就绪，防止中间状态下 T()/TryGet() 返回原始 key
            _ready = false;

            // 使用双缓冲：先加载到临时字典，完成后一次性替换，避免清空-加载的非原子操作
            var newTables = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            var tableOrder = BuildTableOrder();
            var currentLang = CurrentLanguage.Value;

            for (var i = 0; i < tableOrder.Count; i++)
            {
                var tableName = tableOrder[i];
                Dictionary<string, string> entries = null;
                yield return Provider.LoadTable(tableName, currentLang, dict => entries = dict);

                if (entries != null && entries.Count > 0)
                {
                    newTables[tableName] = entries;
                }
                else
                {
                    // Fallback: try loading from fallback chain
                    foreach (var fallbackLang in Settings.EnumerateFallbacks(currentLang))
                    {
                        if (fallbackLang == currentLang) continue;
                        yield return Provider.LoadTable(tableName, fallbackLang, dict => entries = dict);
                        if (entries != null && entries.Count > 0)
                        {
                            newTables[tableName] = entries;
                            break;
                        }
                    }
                }
            }

            // 原子替换：一次性清空旧数据并写入新数据，避免中间状态
            LoadedTables.Clear();
            foreach (var kvp in newTables)
            {
                LoadedTables[kvp.Key] = kvp.Value;
            }

            _reloadRunning = false;
            _reloadCoroutine = null;

            // 如果在加载过程中有新的表被添加（如 PreloadTable 在 reload 期间被调用），
            // 需要重新加载以包含新表，此时不触发 ReadyChanged 避免中间状态
            if (_pendingReload)
            {
                _pendingReload = false;
                StartReload();
                yield break;
            }

            _ready = true;
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
                if (!LoadedTables.TryGetValue(tableName, out var table) || table == null) continue;
                if (table.TryGetValue(key, out var entry) && !string.IsNullOrEmpty(entry))
                {
                    value = entry;
                    return true;
                }
            }

            return false;
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
