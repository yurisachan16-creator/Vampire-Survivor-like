using UnityEngine;
using QFramework;

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

        /// <summary>
        /// 应用全屏设置
        /// </summary>
        public static void ApplyFullscreen(bool fullscreen)
        {
            IsFullscreen = fullscreen;
            
            #if !UNITY_WEBGL
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
            #if !UNITY_WEBGL
            ApplyFullscreen(IsFullscreen);
            #endif
            
            // 音频设置会由 AudioKit 自动从 PlayerPrefs 加载
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
}
