using UnityEngine;
using UnityEngine.UI;
using QFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VampireSurvivorLike
{
    /// <summary>
    /// 游戏设置管理类
    /// 处理显示设置（全屏/窗口化）和音频设置的持久化
    /// </summary>
    public static class GameSettings
    {
        private const string KEY_FULLSCREEN = "GameSettings_Fullscreen";
        private const string KEY_RESOLUTION_WIDTH = "GameSettings_ResWidth";
        private const string KEY_RESOLUTION_HEIGHT = "GameSettings_ResHeight";
        private const string KEY_LOOT_GUIDE = "GameSettings_LootGuide";
        private const string KEY_MOBILE_DEBUG_HUD = "GameSettings_MobileDebugHud";

        /// <summary>
        /// 是否全屏
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

        /// <summary>
        /// 应用全屏设置
        /// </summary>
        public static void ApplyFullscreen(bool fullscreen)
        {
            IsFullscreen = fullscreen;
            
            #if UNITY_WEBGL && !UNITY_EDITOR
            // WebGL 运行时：使用 Screen.fullScreen 切换全屏
            Screen.fullScreen = fullscreen;
            #else
            if (fullscreen)
            {
                // 全屏模式使用当前屏幕分辨率
                Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, FullScreenMode.FullScreenWindow);
            }
            else
            {
                // 窗口模式使用 1280x720
                Screen.SetResolution(1280, 720, FullScreenMode.Windowed);
            }
            #endif
        }

        /// <summary>
        /// 在游戏启动时应用保存的设置
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void ApplySettingsOnStartup()
        {
            if (Application.isMobilePlatform)
            {
                Screen.autorotateToPortrait = false;
                Screen.autorotateToPortraitUpsideDown = false;
                Screen.autorotateToLandscapeLeft = true;
                Screen.autorotateToLandscapeRight = true;
                Screen.orientation = ScreenOrientation.LandscapeLeft;
            }

            // 应用全屏设置（WebGL 和其他平台都支持）
            ApplyFullscreen(IsFullscreen);
            
            // 音频设置会由 AudioKit 自动从 PlayerPrefs 加载
            MobileDebugHud.ApplyStartup();
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
            _text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
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
    }
}
