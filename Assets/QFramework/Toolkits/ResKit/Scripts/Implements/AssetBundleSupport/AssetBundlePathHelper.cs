
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace QFramework
{
    public class AssetBundlePathHelper
    {
#if UNITY_EDITOR
        private struct EditorAssetBundleFreshnessReport
        {
            public bool ShouldForceSimulationMode;
            public string Message;
        }

        const string kSimulateAssetBundles = "SimulateAssetBundles"; //此处跟editor中保持统一，不能随意更改

        private static readonly string[] EditorStaleCheckRoots =
        {
            "Assets/Scripts",
            "Assets/Art",
            "Assets/Scenes",
            "Assets/QFrameworkData"
        };

        private static readonly string[] EditorStaleCheckExtensions =
        {
            ".cs",
            ".asmdef",
            ".prefab",
            ".unity"
        };

        private static bool? sSessionSimulationModeOverride;
        private static string sSessionSimulationModeOverrideReason;

        public static string GetPlatformForAssetBundles(BuildTarget target)
        {
            switch (target)
            {
                case BuildTarget.Android:
                    return "Android";
                case BuildTarget.WSAPlayer:
                    return "WSAPlayer";
                case BuildTarget.iOS:
                    return "iOS";
                case BuildTarget.WebGL:
                    return "WebGL";
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return "Windows";
#if !UNITY_2019_2_OR_NEWER
                case BuildTarget.StandaloneLinux:
#endif
                case BuildTarget.StandaloneLinux64:
#if !UNITY_2019_2_OR_NEWER
                case BuildTarget.StandaloneLinuxUniversal:
#endif
                    return "Linux";
#if !UNITY_2017_3_OR_NEWER
			case BuildTarget.StandaloneOSXIntel:
			case BuildTarget.StandaloneOSXIntel64:
#elif UNITY_5
			case BuildTarget.StandaloneOSXUniversal:
#else
                case BuildTarget.StandaloneOSX:
#endif
                    return "OSX";
                // Add more build targets for your own.
                // If you add more targets, don't forget to add the same platforms to GetPlatformForAssetBundles(RuntimePlatform) function.
                default:
                    return target.ToString();
            }
        }
#endif
        
        public static string GetStreamingAssetBundleDirectory(string platformName = null)
        {
            var effectivePlatformName = string.IsNullOrEmpty(platformName) ? GetPlatformName() : platformName;
            return Path.Combine(Application.streamingAssetsPath, "AssetBundles", effectivePlatformName).Replace("\\", "/");
        }

        public static string GetPersistentAssetBundleDirectory(string platformName = null)
        {
            var effectivePlatformName = string.IsNullOrEmpty(platformName) ? GetPlatformName() : platformName;
            return Path.Combine(Application.persistentDataPath, "AssetBundles", effectivePlatformName).Replace("\\", "/");
        }

        public static string GetStreamingAssetBundleConfigFilePath(string platformName = null)
        {
            var effectivePlatformName = string.IsNullOrEmpty(platformName) ? GetPlatformName() : platformName;
#if UNITY_ANDROID && !UNITY_EDITOR
            return (Application.dataPath + "!/assets/AssetBundles/" + effectivePlatformName + "/" + ResDatas.FileName)
                .Replace("\\", "/");
#else
            return Path.Combine(Application.streamingAssetsPath, "AssetBundles", effectivePlatformName, ResDatas.FileName)
                .Replace("\\", "/");
#endif
        }

        public static string GetPersistentAssetBundleConfigFilePath(string platformName = null)
        {
            var effectivePlatformName = string.IsNullOrEmpty(platformName) ? GetPlatformName() : platformName;
            return Path.Combine(Application.persistentDataPath, "AssetBundles", effectivePlatformName, ResDatas.FileName)
                .Replace("\\", "/");
        }

        public static string GetStreamingAssetBundleConfigFileUrl(string platformName = null)
        {
            var effectivePlatformName = string.IsNullOrEmpty(platformName) ? GetPlatformName() : platformName;
            var configPath = Path.Combine(Application.streamingAssetsPath, "AssetBundles", effectivePlatformName, ResDatas.FileName)
                .Replace("\\", "/");
#if UNITY_EDITOR || UNITY_IOS
            return PathPrefix + configPath;
#else
            return configPath;
#endif
        }

        public static string GetPersistentAssetBundleConfigFileUrl(string platformName = null)
        {
            var effectivePlatformName = string.IsNullOrEmpty(platformName) ? GetPlatformName() : platformName;
            var configPath = Path.Combine(Application.persistentDataPath, "AssetBundles", effectivePlatformName, ResDatas.FileName)
                .Replace("\\", "/");
#if UNITY_EDITOR || UNITY_IOS
            return PathPrefix + configPath;
#else
            return configPath;
#endif
        }

        // 资源路径，优先返回外存资源路径
        public static string GetResPathInPersistentOrStream(string relativePath)
        {
            string resPersistentPath = string.Format("{0}{1}", PersistentDataPath4Res, relativePath);
            if (File.Exists(resPersistentPath))
            {
                return resPersistentPath;
            }
            else
            {
                return StreamingAssetsPath + relativePath;
            }
        }

        
        private static string mPersistentDataPath;
        private static string mStreamingAssetsPath;
        private static string mPersistentDataPath4Res;
        private static string mPersistentDataPath4Photo;

        // 外部目录  
        public static string PersistentDataPath
        {
            get
            {
                if (null == mPersistentDataPath)
                {
                    mPersistentDataPath = Application.persistentDataPath + "/";
                }

                return mPersistentDataPath;
            }
        }

        // 内部目录
        public static string StreamingAssetsPath
        {
            get
            {
                if (null == mStreamingAssetsPath)
                {
#if UNITY_IPHONE && !UNITY_EDITOR
					mStreamingAssetsPath = Application.streamingAssetsPath + "/";
#elif UNITY_ANDROID && !UNITY_EDITOR
					mStreamingAssetsPath = Application.streamingAssetsPath + "/";
#elif (UNITY_STANDALONE_WIN) && !UNITY_EDITOR
					mStreamingAssetsPath =
 Application.streamingAssetsPath + "/";//GetParentDir(Application.dataPath, 2) + "/BuildRes/";
#elif UNITY_STANDALONE_OSX && !UNITY_EDITOR
					mStreamingAssetsPath = Application.streamingAssetsPath + "/";
#else
                    mStreamingAssetsPath = Application.streamingAssetsPath + "/";
#endif
                }

                return mStreamingAssetsPath;
            }
        }


        // 外部头像缓存目录
        public static string PersistentDataPath4Photo
        {
            get
            {
                if (null == mPersistentDataPath4Photo)
                {
                    mPersistentDataPath4Photo = PersistentDataPath + "Photos\\";

                    if (!Directory.Exists(mPersistentDataPath4Photo))
                    {
                        Directory.CreateDirectory(mPersistentDataPath4Photo);
                    }
                }

                return mPersistentDataPath4Photo;
            }
        }

        // 外部资源目录
        public static string PersistentDataPath4Res
        {
            get
            {
                if (null == mPersistentDataPath4Res)
                {
                    mPersistentDataPath4Res = PersistentDataPath + "Res/";

                    if (!Directory.Exists(mPersistentDataPath4Res))
                    {
                        Directory.CreateDirectory(mPersistentDataPath4Res);
#if UNITY_IPHONE && !UNITY_EDITOR
						UnityEngine.iOS.Device.SetNoBackupFlag(mPersistentDataPath4Res);
#endif
                    }
                }

                return mPersistentDataPath4Res;
            }
        }
        public static string GetPlatformName()
        {
#if UNITY_EDITOR
            return GetPlatformForAssetBundles(EditorUserBuildSettings.activeBuildTarget);
#else
			return GetPlatformForAssetBundles(UnityEngine.Application.platform);
#endif
        }
        
        public static string GetPlatformForAssetBundles(RuntimePlatform platform)
        {
            switch (platform)
            {
                case RuntimePlatform.Android:
                    return "Android";
                case RuntimePlatform.WSAPlayerARM:
                case RuntimePlatform.WSAPlayerX64:
                case RuntimePlatform.WSAPlayerX86:
                    return "WSAPlayer";
                case RuntimePlatform.IPhonePlayer:
                    return "iOS";
                case RuntimePlatform.WebGLPlayer:
                    return "WebGL";
                case RuntimePlatform.WindowsPlayer:
                    return "Windows";
                case RuntimePlatform.OSXPlayer:
                    return "OSX";
                case RuntimePlatform.LinuxPlayer:
                    return "Linux";
                // Add more build targets for your own.
                // If you add more targets, don't forget to add the same platforms to GetPlatformForAssetBundles(RuntimePlatform) function.
                default:
                    return platform.ToString().RemoveString("Player");
            }
        }
        
        public static string[] GetAssetPathsFromAssetBundleAndAssetName(string abRAssetName, string assetName)
        {
#if UNITY_EDITOR
            return AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(abRAssetName, assetName);
#else
            return null;
#endif
        }

        public static Object LoadAssetAtPath(string assetPath, Type assetType)
        {
#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath(assetPath, assetType);
#else
            return null;
#endif
        }

        public static T LoadAssetAtPath<T>(string assetPath) where T : Object
        {
#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<T>(assetPath);
#else
            return null;
#endif
        }
        
        
        
// 上一级目录
        public static string GetParentDir(string dir, int floor = 1)
        {
            string subDir = dir;

            for (int i = 0; i < floor; ++i)
            {
                int last = subDir.LastIndexOf('/');
                subDir = subDir.Substring(0, last);
            }

            return subDir;
        }

        public static void GetFileInFolder(string dirName, string fileName, List<string> outResult)
        {
            if (outResult == null)
            {
                return;
            }

            var dir = new DirectoryInfo(dirName);

            if (null != dir.Parent && dir.Attributes.ToString().IndexOf("System", StringComparison.Ordinal) > -1)
            {
                return;
            }

            var fileInfos = dir.GetFiles(fileName);
            outResult.AddRange(fileInfos.Select(fileInfo => fileInfo.FullName));

            var dirInfos = dir.GetDirectories();
            foreach (var dinfo in dirInfos)
            {
                GetFileInFolder(dinfo.FullName, fileName, outResult);
            }
        }

        public static string PathPrefix
        {
            get
            {
#if UNITY_EDITOR || UNITY_IOS
                return "file://";
#else
                return string.Empty;
#endif
            }
        }
#if UNITY_EDITOR
        public static bool PersistentSimulationMode
        {
            get { return UnityEditor.EditorPrefs.GetBool(kSimulateAssetBundles, true); }
            set { UnityEditor.EditorPrefs.SetBool(kSimulateAssetBundles, value); }
        }

        public static bool HasSessionSimulationModeOverride => sSessionSimulationModeOverride.HasValue;

        public static string SessionSimulationModeOverrideReason => sSessionSimulationModeOverrideReason;

        public static void SetSessionSimulationModeOverride(bool value, string reason = null)
        {
            sSessionSimulationModeOverride = value;
            sSessionSimulationModeOverrideReason = reason;
        }

        public static void ClearSessionSimulationModeOverride()
        {
            sSessionSimulationModeOverride = null;
            sSessionSimulationModeOverrideReason = null;
        }

        public static void ApplyEditorPlayModeSimulationFallbackIfNeeded()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode && !Application.isPlaying)
            {
                return;
            }

            if (PersistentSimulationMode)
            {
                ClearSessionSimulationModeOverride();
                return;
            }

            var report = EvaluateEditorPlayModeAssetBundleFreshness();
            if (!report.ShouldForceSimulationMode)
            {
                ClearSessionSimulationModeOverride();
                return;
            }

            if (sSessionSimulationModeOverride == true &&
                string.Equals(sSessionSimulationModeOverrideReason, report.Message, StringComparison.Ordinal))
            {
                return;
            }

            SetSessionSimulationModeOverride(true, report.Message);
            Debug.LogWarning(report.Message);
        }

        public static bool SimulationMode
        {
            get { return sSessionSimulationModeOverride ?? PersistentSimulationMode; }
            set { PersistentSimulationMode = value; }
        }

        private static EditorAssetBundleFreshnessReport EvaluateEditorPlayModeAssetBundleFreshness()
        {
            var platformName = GetPlatformForAssetBundles(EditorUserBuildSettings.activeBuildTarget);
            var bundleDirectory = GetStreamingAssetBundleDirectory(platformName);
            if (!Directory.Exists(bundleDirectory))
            {
                return CreateForcedSimulationReport(
                    $"[ResKit] AssetBundle fallback enabled for this Play session because '{bundleDirectory}' does not exist. " +
                    "The editor will use Simulation Mode until tracked bundles are rebuilt.");
            }

            var configPath = Path.Combine(bundleDirectory, ResDatas.FileName).Replace("\\", "/");
            if (!File.Exists(configPath))
            {
                return CreateForcedSimulationReport(
                    $"[ResKit] AssetBundle fallback enabled for this Play session because the current platform config '{configPath}' is missing. " +
                    "The editor will use Simulation Mode until tracked bundles are rebuilt.");
            }

            var latestBundleWriteTimeUtc = GetLatestNonMetaWriteTimeUtc(bundleDirectory);
            if (!latestBundleWriteTimeUtc.HasValue)
            {
                return CreateForcedSimulationReport(
                    $"[ResKit] AssetBundle fallback enabled for this Play session because '{bundleDirectory}' contains no bundle payloads. " +
                    "The editor will use Simulation Mode until tracked bundles are rebuilt.");
            }

            var latestSourceWriteTimeUtc = GetLatestRelevantProjectWriteTimeUtc();
            if (!latestSourceWriteTimeUtc.HasValue)
            {
                return default;
            }

            if (latestSourceWriteTimeUtc.Value <= latestBundleWriteTimeUtc.Value)
            {
                return default;
            }

            return CreateForcedSimulationReport(
                $"[ResKit] AssetBundle fallback enabled for this Play session because current platform bundles for '{platformName}' are stale. " +
                $"Newest tracked source timestamp: {FormatTimestamp(latestSourceWriteTimeUtc.Value)}; newest bundle timestamp: {FormatTimestamp(latestBundleWriteTimeUtc.Value)}. " +
                "This usually means prefabs or scripts changed after the last AssetBundle rebuild.");
        }

        private static EditorAssetBundleFreshnessReport CreateForcedSimulationReport(string message)
        {
            return new EditorAssetBundleFreshnessReport
            {
                ShouldForceSimulationMode = true,
                Message = message
            };
        }

        private static DateTime? GetLatestRelevantProjectWriteTimeUtc()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            DateTime? latestWriteTimeUtc = null;

            for (var i = 0; i < EditorStaleCheckRoots.Length; i++)
            {
                var root = Path.Combine(projectRoot, EditorStaleCheckRoots[i]);
                if (!Directory.Exists(root))
                {
                    continue;
                }

                foreach (var filePath in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
                {
                    if (!ShouldTrackForEditorBundleFreshness(filePath))
                    {
                        continue;
                    }

                    var writeTimeUtc = File.GetLastWriteTimeUtc(filePath);
                    if (!latestWriteTimeUtc.HasValue || writeTimeUtc > latestWriteTimeUtc.Value)
                    {
                        latestWriteTimeUtc = writeTimeUtc;
                    }
                }
            }

            return latestWriteTimeUtc;
        }

        private static bool ShouldTrackForEditorBundleFreshness(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return false;
            }

            var extension = Path.GetExtension(filePath);
            if (string.IsNullOrEmpty(extension) ||
                string.Equals(extension, ".meta", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            for (var i = 0; i < EditorStaleCheckExtensions.Length; i++)
            {
                if (string.Equals(extension, EditorStaleCheckExtensions[i], StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static DateTime? GetLatestNonMetaWriteTimeUtc(string directoryPath)
        {
            DateTime? latestWriteTimeUtc = null;

            foreach (var filePath in Directory.EnumerateFiles(directoryPath, "*", SearchOption.AllDirectories))
            {
                if (string.Equals(Path.GetExtension(filePath), ".meta", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var writeTimeUtc = File.GetLastWriteTimeUtc(filePath);
                if (!latestWriteTimeUtc.HasValue || writeTimeUtc > latestWriteTimeUtc.Value)
                {
                    latestWriteTimeUtc = writeTimeUtc;
                }
            }

            return latestWriteTimeUtc;
        }

        private static string FormatTimestamp(DateTime timestampUtc)
        {
            return timestampUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
        }
#else
         public static bool SimulationMode
         {
             get { return false; }
             set {  }
         }
#endif
    }
}
