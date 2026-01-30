using UnityEngine;
using QFramework;
using System.Collections;
using QAssetBundle;

namespace VampireSurvivorLike
{
    /// <summary>
    /// WebGL 平台资源预加载器
    /// 在游戏开始时异步加载所有 AssetBundle，
    /// 之后同步加载调用可以直接从缓存中获取
    /// </summary>
    public static class WebGLPreloader
    {
        private static ResLoader _resLoader;
        private static bool _isPreloaded = false;

        public static bool IsPreloaded => _isPreloaded;

        /// <summary>
        /// 预加载所有游戏资源（仅在 WebGL 平台使用）
        /// </summary>
        public static IEnumerator PreloadAllAssets()
        {
            if (_isPreloaded)
            {
                yield break;
            }

            Debug.Log("[WebGLPreloader] Starting asset preload...");

            _resLoader = ResLoader.Allocate();

            // 添加所有需要的资源到加载队列
            // UI Panels
            _resLoader.Add2Load<GameObject>(Uigamestartpanel_prefab.BundleName, Uigamestartpanel_prefab.UIGAMESTARTPANEL);
            _resLoader.Add2Load<GameObject>(Uigamepanel_prefab.BundleName, Uigamepanel_prefab.UIGAMEPANEL);
            _resLoader.Add2Load<GameObject>(Uigameoverpanel_prefab.BundleName, Uigameoverpanel_prefab.UIGAMEOVERPANEL);
            _resLoader.Add2Load<GameObject>(Uigamepasspanel_prefab.BundleName, Uigamepasspanel_prefab.UIGAMEPASSPANEL);
            _resLoader.Add2Load<GameObject>(Uigamesettingspanel_prefab.BundleName, Uigamesettingspanel_prefab.UIGAMESETTINGSPANEL);
            
            // 字体
            _resLoader.Add2Load<Font>(Fusionpixel12pxproportionalzh_hans_ttf.BundleName, 
                Fusionpixel12pxproportionalzh_hans_ttf.FUSIONPIXEL12PXPROPORTIONALZH_HANS);
            
            // 图标 SpriteAtlas
            _resLoader.Add2Load<UnityEngine.U2D.SpriteAtlas>(Icon_spriteatlasv2.BundleName, Icon_spriteatlasv2.ICON);

            // 开始异步加载
            bool loadComplete = false;
            _resLoader.LoadAsync(() =>
            {
                loadComplete = true;
                Debug.Log("[WebGLPreloader] Asset preload complete!");
            });

            // 等待加载完成
            while (!loadComplete)
            {
                yield return null;
            }

            _isPreloaded = true;
        }

        /// <summary>
        /// 释放预加载的资源（通常不需要调用，除非要完全重置）
        /// </summary>
        public static void Release()
        {
            if (_resLoader != null)
            {
                _resLoader.Recycle2Cache();
                _resLoader = null;
            }
            _isPreloaded = false;
        }
    }
}
