using UnityEngine;
using UnityEngine.UI;
using QFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VampireSurvivorLike
{
    public enum GameDifficulty
    {
        Easy = 0,
        Normal = 1,
        Hard = 2
    }

    public readonly struct DifficultyProfile
    {
        public readonly float EnemyHpMultiplier;
        public readonly float EnemySpeedMultiplier;
        public readonly float EnemyDamageMultiplier;
        public readonly float SpawnRateMultiplier;
        public readonly float ExpDropRateMultiplier;
        public readonly float CoinDropRateMultiplier;
        public readonly float HpDropRateMultiplier;
        public readonly float BombDropRateMultiplier;
        public readonly float ExpValueMultiplier;
        public readonly float CoinValueMultiplier;

        public DifficultyProfile(
            float enemyHpMultiplier,
            float enemySpeedMultiplier,
            float enemyDamageMultiplier,
            float spawnRateMultiplier,
            float expDropRateMultiplier,
            float coinDropRateMultiplier,
            float hpDropRateMultiplier,
            float bombDropRateMultiplier,
            float expValueMultiplier,
            float coinValueMultiplier)
        {
            EnemyHpMultiplier = enemyHpMultiplier;
            EnemySpeedMultiplier = enemySpeedMultiplier;
            EnemyDamageMultiplier = enemyDamageMultiplier;
            SpawnRateMultiplier = spawnRateMultiplier;
            ExpDropRateMultiplier = expDropRateMultiplier;
            CoinDropRateMultiplier = coinDropRateMultiplier;
            HpDropRateMultiplier = hpDropRateMultiplier;
            BombDropRateMultiplier = bombDropRateMultiplier;
            ExpValueMultiplier = expValueMultiplier;
            CoinValueMultiplier = coinValueMultiplier;
        }
    }

    /// <summary>
    /// 分辨率选项数据
    /// </summary>
    [Serializable]
    public struct ResolutionOption
    {
        public int Width;
        public int Height;
        public string AspectLabel; // 如 "16:9", "18:9"
        public bool IsAutoDetect; // 标记为"自动检测"选项

        public ResolutionOption(int w, int h, string aspect, bool auto = false)
        {
            Width = w;
            Height = h;
            AspectLabel = aspect;
            IsAutoDetect = auto;
        }

        /// <summary>
        /// 显示文本，如 "1920×1080 (16:9)" 或 "自动检测"
        /// </summary>
        public string GetDisplayText(bool isRecommended = false)
        {
            if (IsAutoDetect) return ""; // 由本地化系统提供
            var text = $"{Width}×{Height} ({AspectLabel})";
            if (isRecommended) text += " ★";
            return text;
        }

        public float GetAspectRatio()
        {
            return Height > 0 ? (float)Width / Height : 0f;
        }
    }

    /// <summary>
    /// 游戏设置管理类
    /// 处理分辨率设置和音频设置的持久化
    /// </summary>
    public static class GameSettings
    {
        private const string KEY_FULLSCREEN = "GameSettings_Fullscreen";
        private const string KEY_RESOLUTION_WIDTH = "GameSettings_ResWidth";
        private const string KEY_RESOLUTION_HEIGHT = "GameSettings_ResHeight";
        private const string KEY_RESOLUTION_INDEX = "GameSettings_ResIndex";
        private const string KEY_LOOT_GUIDE = "GameSettings_LootGuide";
        private const string KEY_MOBILE_DEBUG_HUD = "GameSettings_MobileDebugHud";
        private const string KEY_PERFORMANCE_HUD = "GameSettings_PerformanceHud";
        private const string KEY_MAX_SMALL_ENEMY_WEBGL = "GameSettings_MaxSmallEnemy_WebGL";
        private const string KEY_MAX_SMALL_ENEMY_PC = "GameSettings_MaxSmallEnemy_PC";
        private const string KEY_MAX_SMALL_ENEMY_MOBILE = "GameSettings_MaxSmallEnemy_Mobile";
        private const string KEY_PC_INSTANCED_ENEMY_RENDERER = "GameSettings_PcInstancedEnemyRenderer";
        private const string KEY_SELECTED_DIFFICULTY = "GameSettings_SelectedDifficulty";

        /// <summary>
        /// 预设分辨率列表（覆盖主流屏幕比例）
        /// </summary>
        private static readonly ResolutionOption[] PresetResolutions = new ResolutionOption[]
        {
            new ResolutionOption(1280, 720,   "16:9"),
            new ResolutionOption(1920, 1080,  "16:9"),
            new ResolutionOption(2560, 1440,  "16:9"),
            new ResolutionOption(2160, 1080,  "18:9"),
            new ResolutionOption(2340, 1080,  "19.5:9"),
            new ResolutionOption(2400, 1080,  "20:9"),
            new ResolutionOption(2560, 1080,  "21:9"),
        };

        private static List<ResolutionOption> _cachedResolutions;
        private static bool _difficultyStateInitialized;
        private static bool _difficultyConfigLoaded;
        private static bool _enableAdaptiveMobilePerformance = true;
        private static GameDifficulty _selectedDifficulty = GameDifficulty.Normal;
        private static GameDifficulty _activeRunDifficulty = GameDifficulty.Normal;
        private static bool _activeRunCaptured;
        private static readonly Dictionary<GameDifficulty, DifficultyProfile> RuntimeDifficultyProfiles = new Dictionary<GameDifficulty, DifficultyProfile>(3);
        private static readonly Dictionary<GameDifficulty, DifficultyProfile> DefaultDifficultyProfiles = new Dictionary<GameDifficulty, DifficultyProfile>(3)
        {
            {
                GameDifficulty.Easy,
                new DifficultyProfile(
                    enemyHpMultiplier: 0.85f,
                    enemySpeedMultiplier: 0.85f,
                    enemyDamageMultiplier: 0.85f,
                    spawnRateMultiplier: 0.85f,
                    expDropRateMultiplier: 0.8f,
                    coinDropRateMultiplier: 0.8f,
                    hpDropRateMultiplier: 0.8f,
                    bombDropRateMultiplier: 0.8f,
                    expValueMultiplier: 0.8f,
                    coinValueMultiplier: 0.8f)
            },
            {
                GameDifficulty.Normal,
                new DifficultyProfile(
                    enemyHpMultiplier: 1f,
                    enemySpeedMultiplier: 1f,
                    enemyDamageMultiplier: 1f,
                    spawnRateMultiplier: 1f,
                    expDropRateMultiplier: 1f,
                    coinDropRateMultiplier: 1f,
                    hpDropRateMultiplier: 1f,
                    bombDropRateMultiplier: 1f,
                    expValueMultiplier: 1f,
                    coinValueMultiplier: 1f)
            },
            {
                GameDifficulty.Hard,
                new DifficultyProfile(
                    enemyHpMultiplier: 1.2f,
                    enemySpeedMultiplier: 1.2f,
                    enemyDamageMultiplier: 1.2f,
                    spawnRateMultiplier: 1.2f,
                    expDropRateMultiplier: 1.3f,
                    coinDropRateMultiplier: 1.3f,
                    hpDropRateMultiplier: 1.3f,
                    bombDropRateMultiplier: 1.3f,
                    expValueMultiplier: 1.3f,
                    coinValueMultiplier: 1.3f)
            }
        };

        /// <summary>
        /// 是否全屏（向后兼容，分辨率模式下 Android/WebGL 始终全屏，PC 按分辨率切换）
        /// </summary>
        public static bool IsFullscreen
        {
            get => PlayerPrefs.GetInt(KEY_FULLSCREEN, 1) == 1;
            set
            {
                PlayerPrefs.SetInt(KEY_FULLSCREEN, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        /// <summary>
        /// 保存的分辨率宽度
        /// </summary>
        public static int ResolutionWidth
        {
            get => PlayerPrefs.GetInt(KEY_RESOLUTION_WIDTH, Screen.currentResolution.width);
            set
            {
                PlayerPrefs.SetInt(KEY_RESOLUTION_WIDTH, value);
                PlayerPrefs.Save();
            }
        }

        /// <summary>
        /// 保存的分辨率高度
        /// </summary>
        public static int ResolutionHeight
        {
            get => PlayerPrefs.GetInt(KEY_RESOLUTION_HEIGHT, Screen.currentResolution.height);
            set
            {
                PlayerPrefs.SetInt(KEY_RESOLUTION_HEIGHT, value);
                PlayerPrefs.Save();
            }
        }

        /// <summary>
        /// 保存的分辨率选项索引（0 = 自动检测）
        /// </summary>
        public static int ResolutionIndex
        {
            get => PlayerPrefs.GetInt(KEY_RESOLUTION_INDEX, 0);
            set
            {
                PlayerPrefs.SetInt(KEY_RESOLUTION_INDEX, value);
                PlayerPrefs.Save();
            }
        }

        public static bool EnableLootGuide
        {
            get => PlayerPrefs.GetInt(KEY_LOOT_GUIDE, 1) == 1;
            set
            {
                PlayerPrefs.SetInt(KEY_LOOT_GUIDE, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public static bool EnableMobileDebugHud
        {
            get => PlayerPrefs.GetInt(KEY_MOBILE_DEBUG_HUD, 0) == 1;
            set
            {
                PlayerPrefs.SetInt(KEY_MOBILE_DEBUG_HUD, value ? 1 : 0);
                PlayerPrefs.Save();
                MobileDebugHud.ApplyStartup();
            }
        }

        public static bool EnablePerformanceHud
        {
            get => PlayerPrefs.GetInt(KEY_PERFORMANCE_HUD, Debug.isDebugBuild ? 1 : 0) == 1;
            set
            {
                PlayerPrefs.SetInt(KEY_PERFORMANCE_HUD, value ? 1 : 0);
                PlayerPrefs.Save();
                PerformanceHud.ApplyStartup();
            }
        }

        public static bool EnablePcInstancedEnemyRenderer
        {
            get => PlayerPrefs.GetInt(KEY_PC_INSTANCED_ENEMY_RENDERER, 0) == 1;
            set
            {
                PlayerPrefs.SetInt(KEY_PC_INSTANCED_ENEMY_RENDERER, value ? 1 : 0);
                PlayerPrefs.Save();
                PcInstancedEnemyRenderer.ApplyStartup();
            }
        }

        public static bool EnableAdaptiveMobilePerformance
        {
            get => _enableAdaptiveMobilePerformance;
            set => _enableAdaptiveMobilePerformance = value;
        }

        public static int MaxSmallEnemyCountWebGL
        {
            get => PlayerPrefs.GetInt(KEY_MAX_SMALL_ENEMY_WEBGL, 2500);
            set
            {
                PlayerPrefs.SetInt(KEY_MAX_SMALL_ENEMY_WEBGL, Mathf.Max(0, value));
                PlayerPrefs.Save();
            }
        }

        public static int MaxSmallEnemyCountPC
        {
            get => PlayerPrefs.GetInt(KEY_MAX_SMALL_ENEMY_PC, 500);
            set
            {
                PlayerPrefs.SetInt(KEY_MAX_SMALL_ENEMY_PC, Mathf.Max(0, value));
                PlayerPrefs.Save();
            }
        }

        public static int MaxSmallEnemyCountMobile
        {
            get => PlayerPrefs.GetInt(KEY_MAX_SMALL_ENEMY_MOBILE, 220);
            set
            {
                PlayerPrefs.SetInt(KEY_MAX_SMALL_ENEMY_MOBILE, Mathf.Max(0, value));
                PlayerPrefs.Save();
            }
        }

        public static int GetMaxSmallEnemyCountForCurrentPlatform()
        {
            #if UNITY_WEBGL && !UNITY_EDITOR
            return MaxSmallEnemyCountWebGL;
            #else
            if (Application.isMobilePlatform) return MaxSmallEnemyCountMobile;
            return MaxSmallEnemyCountPC;
            #endif
        }

        public static GameDifficulty SelectedDifficulty
        {
            get
            {
                EnsureDifficultyStateInitialized();
                return _selectedDifficulty;
            }
            set
            {
                EnsureDifficultyStateInitialized();
                var normalized = NormalizeDifficulty(value);
                if (_selectedDifficulty == normalized) return;

                _selectedDifficulty = normalized;
                PlayerPrefs.SetInt(KEY_SELECTED_DIFFICULTY, (int)_selectedDifficulty);
                PlayerPrefs.Save();
            }
        }

        public static GameDifficulty ActiveRunDifficulty
        {
            get
            {
                EnsureDifficultyStateInitialized();
                return _activeRunCaptured ? _activeRunDifficulty : _selectedDifficulty;
            }
        }

        public static bool HasPendingDifficultyChange
        {
            get
            {
                EnsureDifficultyStateInitialized();
                return _activeRunCaptured && _activeRunDifficulty != _selectedDifficulty;
            }
        }

        public static bool DifficultyConfigLoaded => _difficultyConfigLoaded;

        public static void SetSelectedDifficultyByIndex(int index)
        {
            SelectedDifficulty = (GameDifficulty)index;
        }

        public static void CaptureRunDifficulty()
        {
            EnsureDifficultyStateInitialized();
            _activeRunDifficulty = _selectedDifficulty;
            _activeRunCaptured = true;
        }

        public static void CaptureRunDifficultyIfNeeded()
        {
            EnsureDifficultyStateInitialized();
            if (_activeRunCaptured) return;
            CaptureRunDifficulty();
        }

        public static void ClearActiveRunDifficulty()
        {
            EnsureDifficultyStateInitialized();
            _activeRunCaptured = false;
            _activeRunDifficulty = _selectedDifficulty;
        }

        public static DifficultyProfile GetSelectedProfile()
        {
            EnsureDifficultyStateInitialized();
            return GetProfileByDifficulty(_selectedDifficulty);
        }

        public static DifficultyProfile GetActiveRunProfile()
        {
            EnsureDifficultyStateInitialized();
            return GetProfileByDifficulty(ActiveRunDifficulty);
        }

        public static float GetEnemyStrengthDeltaPercent(DifficultyProfile profile)
        {
            var average =
                (profile.EnemyHpMultiplier +
                 profile.EnemySpeedMultiplier +
                 profile.EnemyDamageMultiplier +
                 profile.SpawnRateMultiplier) / 4f;
            return (average - 1f) * 100f;
        }

        public static float GetRewardDeltaPercent(DifficultyProfile profile)
        {
            var average =
                (profile.ExpDropRateMultiplier +
                 profile.CoinDropRateMultiplier +
                 profile.HpDropRateMultiplier +
                 profile.BombDropRateMultiplier +
                 profile.ExpValueMultiplier +
                 profile.CoinValueMultiplier) / 6f;
            return (average - 1f) * 100f;
        }

        public static string GetDifficultyLocalizationKey(GameDifficulty difficulty)
        {
            switch (difficulty)
            {
                case GameDifficulty.Easy:
                    return "ui.settings.difficulty_easy";
                case GameDifficulty.Hard:
                    return "ui.settings.difficulty_hard";
                default:
                    return "ui.settings.difficulty_normal";
            }
        }

        public static IEnumerator LoadDifficultyConfigAsync()
        {
            EnsureDifficultyStateInitialized();
            if (_difficultyConfigLoaded) yield break;

            Dictionary<GameDifficulty, DifficultyProfile> loadedProfiles = null;
            yield return DifficultyConfigLoader.LoadAsync(dict => loadedProfiles = dict);
            ApplyDifficultyProfiles(loadedProfiles);
            _difficultyConfigLoaded = true;
        }

        public static void ApplyDifficultyProfiles(Dictionary<GameDifficulty, DifficultyProfile> loadedProfiles)
        {
            EnsureDifficultyStateInitialized();
            RuntimeDifficultyProfiles.Clear();
            foreach (var pair in DefaultDifficultyProfiles)
            {
                RuntimeDifficultyProfiles[pair.Key] = pair.Value;
            }

            if (loadedProfiles == null) return;

            foreach (var pair in loadedProfiles)
            {
                RuntimeDifficultyProfiles[pair.Key] = NormalizeProfile(pair.Value);
            }
        }

        private static DifficultyProfile GetProfileByDifficulty(GameDifficulty difficulty)
        {
            EnsureDifficultyStateInitialized();
            var normalized = NormalizeDifficulty(difficulty);
            if (RuntimeDifficultyProfiles.TryGetValue(normalized, out var profile))
            {
                return profile;
            }

            return DefaultDifficultyProfiles[GameDifficulty.Normal];
        }

        private static void EnsureDifficultyStateInitialized()
        {
            if (_difficultyStateInitialized) return;

            RuntimeDifficultyProfiles.Clear();
            foreach (var pair in DefaultDifficultyProfiles)
            {
                RuntimeDifficultyProfiles[pair.Key] = pair.Value;
            }

            var raw = PlayerPrefs.GetInt(KEY_SELECTED_DIFFICULTY, (int)GameDifficulty.Normal);
            _selectedDifficulty = NormalizeDifficulty((GameDifficulty)raw);
            _activeRunDifficulty = _selectedDifficulty;
            _activeRunCaptured = false;
            _difficultyStateInitialized = true;
        }

        private static DifficultyProfile NormalizeProfile(DifficultyProfile profile)
        {
            return new DifficultyProfile(
                enemyHpMultiplier: Mathf.Max(0.01f, profile.EnemyHpMultiplier),
                enemySpeedMultiplier: Mathf.Max(0.01f, profile.EnemySpeedMultiplier),
                enemyDamageMultiplier: Mathf.Max(0.01f, profile.EnemyDamageMultiplier),
                spawnRateMultiplier: Mathf.Max(0.01f, profile.SpawnRateMultiplier),
                expDropRateMultiplier: Mathf.Max(0f, profile.ExpDropRateMultiplier),
                coinDropRateMultiplier: Mathf.Max(0f, profile.CoinDropRateMultiplier),
                hpDropRateMultiplier: Mathf.Max(0f, profile.HpDropRateMultiplier),
                bombDropRateMultiplier: Mathf.Max(0f, profile.BombDropRateMultiplier),
                expValueMultiplier: Mathf.Max(0f, profile.ExpValueMultiplier),
                coinValueMultiplier: Mathf.Max(0f, profile.CoinValueMultiplier));
        }

        private static GameDifficulty NormalizeDifficulty(GameDifficulty difficulty)
        {
            if ((int)difficulty < (int)GameDifficulty.Easy || (int)difficulty > (int)GameDifficulty.Hard)
            {
                return GameDifficulty.Normal;
            }

            return difficulty;
        }

        /// <summary>
        /// 获取当前平台可用的分辨率列表（第一项为"自动检测"）
        /// </summary>
        public static List<ResolutionOption> GetAvailableResolutions()
        {
            if (_cachedResolutions != null) return _cachedResolutions;

            _cachedResolutions = new List<ResolutionOption>();

            // 第一项：自动检测
            _cachedResolutions.Add(new ResolutionOption(0, 0, "", true));

            // 获取当前屏幕最大分辨率
            var maxWidth = Screen.currentResolution.width;
            var maxHeight = Screen.currentResolution.height;

            for (var i = 0; i < PresetResolutions.Length; i++)
            {
                var res = PresetResolutions[i];
                // 过滤超过屏幕最大分辨率的选项（Android/移动设备允许所有预设）
                if (!Application.isMobilePlatform && (res.Width > maxWidth || res.Height > maxHeight))
                    continue;
                _cachedResolutions.Add(res);
            }

            // 如果当前屏幕分辨率不在预设列表中，添加为自定义选项
            if (!Application.isMobilePlatform)
            {
                var found = false;
                for (var i = 1; i < _cachedResolutions.Count; i++)
                {
                    if (_cachedResolutions[i].Width == maxWidth && _cachedResolutions[i].Height == maxHeight)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found && maxWidth > 0 && maxHeight > 0)
                {
                    var aspect = GetAspectLabel(maxWidth, maxHeight);
                    _cachedResolutions.Add(new ResolutionOption(maxWidth, maxHeight, aspect));
                }
            }

            return _cachedResolutions;
        }

        /// <summary>
        /// 自动检测最优分辨率，返回推荐的分辨率选项索引
        /// </summary>
        public static int AutoDetectResolutionIndex()
        {
            var resolutions = GetAvailableResolutions();
            var screenW = Screen.currentResolution.width;
            var screenH = Screen.currentResolution.height;
            var screenAspect = screenH > 0 ? (float)screenW / screenH : 16f / 9f;

            var bestIndex = 1; // 默认第一个非自动选项
            var bestScore = float.MaxValue;

            for (var i = 1; i < resolutions.Count; i++)
            {
                var res = resolutions[i];
                var resAspect = res.GetAspectRatio();
                // 优先匹配宽高比，其次匹配分辨率大小
                var aspectDiff = Mathf.Abs(resAspect - screenAspect);
                var sizeDiff = Mathf.Abs(res.Width - screenW) + Mathf.Abs(res.Height - screenH);
                var score = aspectDiff * 10000f + sizeDiff;
                if (score < bestScore)
                {
                    bestScore = score;
                    bestIndex = i;
                }
            }

            return bestIndex;
        }

        /// <summary>
        /// 获取推荐分辨率索引（用于标记★）
        /// </summary>
        public static int GetRecommendedIndex()
        {
            return AutoDetectResolutionIndex();
        }

        /// <summary>
        /// 计算宽高比标签
        /// </summary>
        public static string GetAspectLabel(int width, int height)
        {
            if (height <= 0) return "?";
            var ratio = (float)width / height;

            if (Mathf.Abs(ratio - 16f / 9f) < 0.05f) return "16:9";
            if (Mathf.Abs(ratio - 18f / 9f) < 0.05f) return "18:9";
            if (Mathf.Abs(ratio - 19.5f / 9f) < 0.05f) return "19.5:9";
            if (Mathf.Abs(ratio - 20f / 9f) < 0.05f) return "20:9";
            if (Mathf.Abs(ratio - 21f / 9f) < 0.05f) return "21:9";
            if (Mathf.Abs(ratio - 4f / 3f) < 0.05f) return "4:3";
            if (Mathf.Abs(ratio - 16f / 10f) < 0.05f) return "16:10";

            // 通用计算
            var gcd = GCD(width, height);
            return $"{width / gcd}:{height / gcd}";
        }

        private static int GCD(int a, int b)
        {
            while (b != 0)
            {
                var t = b;
                b = a % b;
                a = t;
            }
            return a;
        }

        /// <summary>
        /// 应用分辨率设置
        /// </summary>
        /// <param name="index">分辨率列表中的索引，0 = 自动检测</param>
        public static void ApplyResolution(int index)
        {
            var resolutions = GetAvailableResolutions();
            if (index < 0 || index >= resolutions.Count) index = 0;

            ResolutionIndex = index;

            int targetW, targetH;

            if (index == 0) // 自动检测
            {
                var autoIndex = AutoDetectResolutionIndex();
                if (autoIndex > 0 && autoIndex < resolutions.Count)
                {
                    targetW = resolutions[autoIndex].Width;
                    targetH = resolutions[autoIndex].Height;
                }
                else
                {
                    targetW = Screen.currentResolution.width;
                    targetH = Screen.currentResolution.height;
                }
            }
            else
            {
                targetW = resolutions[index].Width;
                targetH = resolutions[index].Height;
            }

            // 保存分辨率
            ResolutionWidth = targetW;
            ResolutionHeight = targetH;

            // 应用分辨率
            ApplyResolutionInternal(targetW, targetH);
        }

        /// <summary>
        /// 内部分辨率应用逻辑，跨平台处理
        /// </summary>
        private static void ApplyResolutionInternal(int width, int height)
        {
            #if UNITY_WEBGL && !UNITY_EDITOR
            // WebGL：设置画布渲染分辨率
            Screen.SetResolution(width, height, FullScreenMode.MaximizedWindow);
            #elif UNITY_ANDROID && !UNITY_EDITOR
            // Android：始终全屏，分辨率影响渲染缩放
            Screen.SetResolution(width, height, FullScreenMode.FullScreenWindow);
            #else
            // Windows/macOS/Editor
            if (width >= Screen.currentResolution.width && height >= Screen.currentResolution.height)
            {
                // 选择的分辨率等于或大于屏幕分辨率，使用全屏
                Screen.SetResolution(width, height, FullScreenMode.FullScreenWindow);
                IsFullscreen = true;
            }
            else
            {
                // 选择的分辨率小于屏幕分辨率，使用窗口模式
                Screen.SetResolution(width, height, FullScreenMode.Windowed);
                IsFullscreen = false;
            }
            #endif
        }

        /// <summary>
        /// 向后兼容：应用全屏设置（内部调用分辨率设置）
        /// </summary>
        public static void ApplyFullscreen(bool fullscreen)
        {
            IsFullscreen = fullscreen;
            
            #if UNITY_WEBGL && !UNITY_EDITOR
            Screen.fullScreen = fullscreen;
            #else
            if (fullscreen)
            {
                Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, FullScreenMode.FullScreenWindow);
            }
            else
            {
                var w = ResolutionWidth;
                var h = ResolutionHeight;
                if (w <= 0 || h <= 0) { w = 1280; h = 720; }
                Screen.SetResolution(w, h, FullScreenMode.Windowed);
            }
            #endif
        }

        /// <summary>
        /// 在游戏启动时应用保存的设置
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void ApplySettingsOnStartup()
        {
            EnableAdaptiveMobilePerformance = true;

            if (Application.isMobilePlatform)
            {
                Screen.autorotateToPortrait = false;
                Screen.autorotateToPortraitUpsideDown = false;
                Screen.autorotateToLandscapeLeft = true;
                Screen.autorotateToLandscapeRight = true;
                Screen.orientation = ScreenOrientation.LandscapeLeft;
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = 60;
            }

            // 应用保存的分辨率设置
            var savedIndex = ResolutionIndex;
            if (savedIndex >= 0)
            {
                ApplyResolution(savedIndex);
            }
            else
            {
                // 向后兼容：使用旧的全屏设置
                ApplyFullscreen(IsFullscreen);
            }
            
            // 音频设置会由 AudioKit 自动从 PlayerPrefs 加载
            MobileDebugHud.ApplyStartup();
            PerformanceHud.ApplyStartup();
            PcInstancedEnemyRenderer.ApplyStartup();

            EnsureDifficultyStateInitialized();
        }

        /// <summary>
        /// 退出游戏
        /// </summary>
        public static void QuitGame()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #elif UNITY_WEBGL
            // WebGL 不支持退出，可以显示提示或跳转
            Debug.Log("WebGL 平台不支持退出游戏");
            #else
            Application.Quit();
            #endif
        }

        /// <summary>
        /// 清除分辨率缓存（屏幕变化时调用）
        /// </summary>
        public static void InvalidateResolutionCache()
        {
            _cachedResolutions = null;
        }
    }

    [DisallowMultipleComponent]
    public sealed class MobileDebugHud : MonoBehaviour
    {
        private const int MaxLogLines = 200;
        private static MobileDebugHud _instance;

        private readonly Queue<string> _logLines = new Queue<string>(MaxLogLines);
        private Text _text;
        private float _smoothedDeltaTime = 0.016f;
        private float _nextUpdateTime;
        private bool _visible = true;

        public static void ApplyStartup()
        {
            if (!Application.isMobilePlatform) return;
            if (!Debug.isDebugBuild) return;

            if (GameSettings.EnableMobileDebugHud) Ensure();
            else DestroyIfExists();
        }

        public static void Ensure()
        {
            if (_instance) return;

            var go = new GameObject("MobileDebugHud");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<MobileDebugHud>();
        }

        private static void DestroyIfExists()
        {
            if (!_instance) return;
            Destroy(_instance.gameObject);
            _instance = null;
        }

        private void Awake()
        {
            if (!Application.isMobilePlatform || !Debug.isDebugBuild || !GameSettings.EnableMobileDebugHud)
            {
                Destroy(gameObject);
                return;
            }

            BuildUi();
        }

        private void OnEnable()
        {
            Application.logMessageReceived += OnLogMessageReceived;
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= OnLogMessageReceived;
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        private void Update()
        {
            if (!Application.isMobilePlatform) return;
            if (!Debug.isDebugBuild) return;

            DetectGestures();

            _smoothedDeltaTime = Mathf.Lerp(_smoothedDeltaTime, Time.unscaledDeltaTime, 0.1f);

            if (Time.unscaledTime < _nextUpdateTime) return;
            _nextUpdateTime = Time.unscaledTime + 0.2f;

            if (_text) _text.gameObject.SetActive(_visible);
            if (_visible) RefreshText();
        }

        private void DetectGestures()
        {
            if (Input.touchCount <= 0) return;

            var beganCount = 0;
            var avgY = 0f;
            for (var i = 0; i < Input.touchCount; i++)
            {
                var t = Input.GetTouch(i);
                if (t.phase == TouchPhase.Began) beganCount++;
                avgY += t.position.y;
            }

            avgY /= Mathf.Max(1, Input.touchCount);
            var inTopArea = avgY >= Screen.height * 0.75f;
            if (!inTopArea) return;

            if (beganCount >= 4)
            {
                DumpLogsToFile();
            }
            else if (beganCount >= 3)
            {
                _visible = !_visible;
            }
        }

        private void BuildUi()
        {
            var canvasGo = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);

            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGo.GetComponent<GraphicRaycaster>().enabled = false;

            if (!canvasGo.GetComponent<SafeAreaFitter>()) canvasGo.AddComponent<SafeAreaFitter>();

            var panelGo = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            var panelRt = (RectTransform)panelGo.transform;
            panelRt.SetParent(canvasGo.transform, false);
            panelRt.anchorMin = new Vector2(0f, 1f);
            panelRt.anchorMax = new Vector2(0f, 1f);
            panelRt.pivot = new Vector2(0f, 1f);
            panelRt.anchoredPosition = new Vector2(18f, -18f);
            panelRt.sizeDelta = new Vector2(780f, 420f);

            var panelImg = panelGo.GetComponent<Image>();
            var sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
            panelImg.sprite = sprite;
            panelImg.type = Image.Type.Sliced;
            panelImg.color = new Color(0f, 0f, 0f, 0.45f);

            var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            var textRt = (RectTransform)textGo.transform;
            textRt.SetParent(panelRt, false);
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(16f, 14f);
            textRt.offsetMax = new Vector2(-16f, -14f);

            _text = textGo.GetComponent<Text>();
            _text.alignment = TextAnchor.UpperLeft;
            var fallbackFont = GetBuiltinFallbackFont();
            if (fallbackFont) _text.font = fallbackFont;
            _text.fontSize = 22;
            _text.color = Color.white;
            _text.supportRichText = false;
            _text.horizontalOverflow = HorizontalWrapMode.Wrap;
            _text.verticalOverflow = VerticalWrapMode.Overflow;
        }

        private void RefreshText()
        {
            var fps = 1f / Mathf.Max(0.0001f, _smoothedDeltaTime);
            var move = PlatformInput.GetMoveAxisRaw();
            var safe = Screen.safeArea;

            var sb = new StringBuilder(512);
            sb.Append("FPS: ").Append(fps.ToString("0")).Append('\n');
            sb.Append("Screen: ").Append(Screen.width).Append('x').Append(Screen.height).Append('\n');
            sb.Append("SafeArea: ")
                .Append((int)safe.x).Append(',')
                .Append((int)safe.y).Append(',')
                .Append((int)safe.width).Append('x')
                .Append((int)safe.height).Append('\n');
            sb.Append("Move: ").Append(move.x.ToString("0.00")).Append(',').Append(move.y.ToString("0.00")).Append('\n');
            sb.Append("Back: Esc or 3-finger tap (top) | Dump: 4-finger tap (top)").Append('\n');
            sb.Append("Persistent: ").Append(Application.persistentDataPath).Append('\n');
            sb.Append("---- Logs (latest) ----").Append('\n');

            var lines = _logLines.ToArray();
            var start = Mathf.Max(0, lines.Length - 14);
            for (var i = start; i < lines.Length; i++)
            {
                sb.Append(lines[i]).Append('\n');
            }

            _text.text = sb.ToString();
        }

        private void OnLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            var prefix = type == LogType.Error || type == LogType.Exception ? "E" :
                type == LogType.Warning ? "W" : "I";

            var line = $"{DateTime.Now:HH:mm:ss} [{prefix}] {condition}";
            EnqueueLog(line);
        }

        private void EnqueueLog(string line)
        {
            while (_logLines.Count >= MaxLogLines) _logLines.Dequeue();
            _logLines.Enqueue(line);
        }

        private void DumpLogsToFile()
        {
            try
            {
                var path = Path.Combine(Application.persistentDataPath, $"mobile_debug_log_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
                var sb = new StringBuilder(4096);
                sb.Append("DeviceModel: ").Append(SystemInfo.deviceModel).Append('\n');
                sb.Append("DeviceName: ").Append(SystemInfo.deviceName).Append('\n');
                sb.Append("OS: ").Append(SystemInfo.operatingSystem).Append('\n');
                sb.Append("Unity: ").Append(Application.unityVersion).Append('\n');
                sb.Append("Screen: ").Append(Screen.width).Append('x').Append(Screen.height).Append('\n');
                sb.Append("SafeArea: ").Append(Screen.safeArea).Append('\n');
                sb.Append("---- Logs ----").Append('\n');

                foreach (var l in _logLines) sb.Append(l).Append('\n');
                File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
                EnqueueLog($"{DateTime.Now:HH:mm:ss} [I] Dumped: {path}");
            }
            catch (Exception e)
            {
                EnqueueLog($"{DateTime.Now:HH:mm:ss} [E] Dump failed: {e.Message}");
            }
        }

        private static Font GetBuiltinFallbackFont()
        {
            try
            {
                return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
            catch
            {
                try
                {
                    return Resources.GetBuiltinResource<Font>("Arial.ttf");
                }
                catch
                {
                    return null;
                }
            }
        }
    }
}
