using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;

namespace VampireSurvivorLike
{
    public static class BuildTool
    {
        private const string ProductName = "Vampire Survivor-like";

        private static string[] GetBuildScenes()
        {
            return EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .ToArray();
        }

        private static string GetVersion()
        {
            return PlayerSettings.bundleVersion;
        }

        // ==================== Android ====================

        [MenuItem("Build/Android/Development Build", priority = 100)]
        public static void BuildAndroidDev()
        {
            BuildAndroid(isDevelopment: true);
        }

        [MenuItem("Build/Android/Release Build", priority = 101)]
        public static void BuildAndroidRelease()
        {
            BuildAndroid(isDevelopment: false);
        }

        private static void BuildAndroid(bool isDevelopment)
        {
            var subFolder = isDevelopment ? "Development" : "Release";
            var outputDir = Path.Combine("Build", "Android", subFolder);
            Directory.CreateDirectory(outputDir);

            var fileName = $"{ProductName}_v{GetVersion()}_{(isDevelopment ? "dev" : "release")}.apk";
            var outputPath = Path.Combine(outputDir, fileName);

            // 先构建 AssetBundle
            BuildAssetBundles(BuildTarget.Android);

            var options = new BuildPlayerOptions
            {
                scenes = GetBuildScenes(),
                locationPathName = outputPath,
                target = BuildTarget.Android,
                options = isDevelopment
                    ? BuildOptions.Development | BuildOptions.ConnectWithProfiler
                    : BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.Log($"[BuildTool] Android {subFolder} 构建成功: {outputPath} ({report.summary.totalSize / (1024 * 1024):F1} MB)");
                EditorUtility.RevealInFinder(outputPath);
            }
            else
            {
                Debug.LogError($"[BuildTool] Android {subFolder} 构建失败: {report.summary.result}");
            }
        }

        // ==================== Windows ====================

        [MenuItem("Build/Windows/Development Build", priority = 200)]
        public static void BuildWindowsDev()
        {
            BuildWindows(isDevelopment: true);
        }

        [MenuItem("Build/Windows/Release Build", priority = 201)]
        public static void BuildWindowsRelease()
        {
            BuildWindows(isDevelopment: false);
        }

        private static void BuildWindows(bool isDevelopment)
        {
            var subFolder = isDevelopment ? "Development" : "Release";
            var outputDir = Path.Combine("Build", "Windows", subFolder);
            Directory.CreateDirectory(outputDir);

            var fileName = $"{ProductName}.exe";
            var outputPath = Path.Combine(outputDir, fileName);

            BuildAssetBundles(BuildTarget.StandaloneWindows64);

            var options = new BuildPlayerOptions
            {
                scenes = GetBuildScenes(),
                locationPathName = outputPath,
                target = BuildTarget.StandaloneWindows64,
                options = isDevelopment
                    ? BuildOptions.Development | BuildOptions.ConnectWithProfiler
                    : BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.Log($"[BuildTool] Windows {subFolder} 构建成功: {outputPath} ({report.summary.totalSize / (1024 * 1024):F1} MB)");
                EditorUtility.RevealInFinder(outputPath);
            }
            else
            {
                Debug.LogError($"[BuildTool] Windows {subFolder} 构建失败: {report.summary.result}");
            }
        }

        // ==================== WebGL ====================

        [MenuItem("Build/WebGL/Development Build", priority = 300)]
        public static void BuildWebGLDev()
        {
            BuildWebGL(isDevelopment: true);
        }

        [MenuItem("Build/WebGL/Release Build", priority = 301)]
        public static void BuildWebGLRelease()
        {
            BuildWebGL(isDevelopment: false);
        }

        private static void BuildWebGL(bool isDevelopment)
        {
            var subFolder = isDevelopment ? "Development" : "Release";
            var outputDir = Path.Combine("Build", "WebGL", subFolder);
            Directory.CreateDirectory(outputDir);

            BuildAssetBundles(BuildTarget.WebGL);

            var options = new BuildPlayerOptions
            {
                scenes = GetBuildScenes(),
                locationPathName = outputDir,
                target = BuildTarget.WebGL,
                options = isDevelopment
                    ? BuildOptions.Development | BuildOptions.ConnectWithProfiler
                    : BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.Log($"[BuildTool] WebGL {subFolder} 构建成功: {outputDir} ({report.summary.totalSize / (1024 * 1024):F1} MB)");
                EditorUtility.RevealInFinder(outputDir);
            }
            else
            {
                Debug.LogError($"[BuildTool] WebGL {subFolder} 构建失败: {report.summary.result}");
            }
        }

        // ==================== AssetBundle ====================

        [MenuItem("Build/Build AssetBundles (Current Platform)", priority = 400)]
        public static void BuildAssetBundlesCurrentPlatform()
        {
            BuildAssetBundles(EditorUserBuildSettings.activeBuildTarget);
        }

        private static void BuildAssetBundles(BuildTarget target)
        {
            var platformName = GetPlatformFolderName(target);
            var outputPath = Path.Combine("AssetBundles", platformName);
            Directory.CreateDirectory(outputPath);

            Debug.Log($"[BuildTool] 构建 AssetBundle: {target} → {outputPath}");
            BuildPipeline.BuildAssetBundles(outputPath, BuildAssetBundleOptions.None, target);
            Debug.Log($"[BuildTool] AssetBundle 构建完成: {outputPath}");
        }

        private static string GetPlatformFolderName(BuildTarget target)
        {
            switch (target)
            {
                case BuildTarget.Android: return "Android";
                case BuildTarget.WebGL: return "WebGL";
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64: return "Windows";
                case BuildTarget.StandaloneOSX: return "MacOS";
                case BuildTarget.StandaloneLinux64: return "Linux";
                default: return target.ToString();
            }
        }

        // ==================== Utility ====================

        [MenuItem("Build/Open Build Folder", priority = 500)]
        public static void OpenBuildFolder()
        {
            var path = Path.GetFullPath("Build");
            Directory.CreateDirectory(path);
            EditorUtility.RevealInFinder(path);
        }

        [MenuItem("Build/Clean Build Folder", priority = 501)]
        public static void CleanBuildFolder()
        {
            if (!EditorUtility.DisplayDialog("清理构建目录",
                "确定要删除 Build/ 目录下的所有构建产物吗？\n此操作不可撤销。",
                "确定删除", "取消"))
            {
                return;
            }

            var path = Path.GetFullPath("Build");
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
                Debug.Log("[BuildTool] 构建目录已清理");
            }

            Directory.CreateDirectory(path);
        }
    }
}
