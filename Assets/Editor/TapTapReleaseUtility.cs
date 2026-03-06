using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace VampireSurvivorLike.Editor
{
    internal static class TapTapReleaseUtility
    {
        private const string ReleaseBundleVersion = "0.12.0";
        private const int AndroidReleaseVersionCode = 12;
        private const int AndroidMinSdkVersion = 23;
        private const int AndroidTargetSdkVersion = 35;
        private const string AndroidApplicationId = "com.obsidian.vampiresurvivorlike";
        private const string StandaloneApplicationId = "com.obsidian.vampiresurvivorlike";
        private const string WindowsEntryRelativePath = "Vampire Survivor-like.exe";

        private static readonly string[] RequiredScenes =
        {
            "Assets/Scenes/GameStart.unity",
            "Assets/Scenes/Game.unity"
        };

        [MenuItem("Tools/TapTap/Apply Android Release Settings")]
        private static void ApplyAndroidReleaseSettings()
        {
            ApplySharedReleaseSettings();
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, AndroidApplicationId);
            PlayerSettings.Android.bundleVersionCode = AndroidReleaseVersionCode;
            PlayerSettings.Android.minSdkVersion = (AndroidSdkVersions)AndroidMinSdkVersion;
            PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)AndroidTargetSdkVersion;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            SaveProjectConfiguration();

            EditorUtility.DisplayDialog(
                "TapTap Android Release",
                "已应用 Android 发布默认值。\n请在提交前手动配置正式 keystore 并重新构建 Android AssetBundle。",
                "确定");
        }

        [MenuItem("Tools/TapTap/Apply Windows Release Settings")]
        private static void ApplyWindowsReleaseSettings()
        {
            ApplySharedReleaseSettings();
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Standalone, StandaloneApplicationId);
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.IL2CPP);
            SaveProjectConfiguration();

            EditorUtility.DisplayDialog(
                "TapTap Windows Release",
                "已应用 Windows 发布默认值。\n请在打包后使用 Tools/Release/Prepare-TapTapWindowsPackage.ps1 生成纯净压缩包。",
                "确定");
        }

        [MenuItem("Tools/TapTap/Validate Release Readiness")]
        private static void ValidateReleaseReadiness()
        {
            var issues = new List<string>();

            ValidateScenes(issues);
            ValidateBundleVersion(issues);
            ValidateApplicationIds(issues);
            ValidateAndroidSettings(issues);

            if (issues.Count == 0)
            {
                Debug.Log("[TapTap] Release readiness validation passed.");
                EditorUtility.DisplayDialog(
                    "TapTap Release Validation",
                    $"未发现配置问题。\nWindows 启动相对路径应填写：{WindowsEntryRelativePath}",
                    "确定");
                return;
            }

            Debug.LogWarning("[TapTap] Release readiness validation found issues:\n- " + string.Join("\n- ", issues));
            EditorUtility.DisplayDialog(
                "TapTap Release Validation",
                "发现以下问题：\n- " + string.Join("\n- ", issues),
                "确定");
        }

        [MenuItem("Tools/TapTap/Open Release Docs")]
        private static void OpenReleaseDocs()
        {
            var docsPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Docs"));
            EditorUtility.RevealInFinder(docsPath);
        }

        private static void ApplySharedReleaseSettings()
        {
            EnsureBuildScenes();
            PlayerSettings.bundleVersion = ReleaseBundleVersion;
            EditorUserBuildSettings.development = false;
            EditorUserBuildSettings.connectProfiler = false;
            EditorUserBuildSettings.allowDebugging = false;
        }

        private static void EnsureBuildScenes()
        {
            var scenes = new List<EditorBuildSettingsScene>();
            foreach (var existingScene in EditorBuildSettings.scenes)
            {
                scenes.Add(existingScene);
            }

            foreach (var requiredScene in RequiredScenes)
            {
                var index = scenes.FindIndex(scene => scene.path == requiredScene);
                if (index >= 0)
                {
                    scenes[index].enabled = true;
                    continue;
                }

                scenes.Add(new EditorBuildSettingsScene(requiredScene, true));
            }

            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void ValidateScenes(List<string> issues)
        {
            foreach (var requiredScene in RequiredScenes)
            {
                var scene = FindScene(requiredScene);
                if (scene == null)
                {
                    issues.Add($"Build Settings 缺少场景：{requiredScene}");
                }
                else if (!scene.enabled)
                {
                    issues.Add($"Build Settings 中场景未启用：{requiredScene}");
                }
            }
        }

        private static EditorBuildSettingsScene FindScene(string path)
        {
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (scene.path == path)
                {
                    return scene;
                }
            }

            return null;
        }

        private static void ValidateBundleVersion(List<string> issues)
        {
            if (PlayerSettings.bundleVersion != ReleaseBundleVersion)
            {
                issues.Add($"bundleVersion 应为 {ReleaseBundleVersion}，当前是 {PlayerSettings.bundleVersion}");
            }
        }

        private static void ValidateApplicationIds(List<string> issues)
        {
            var androidId = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android);
            if (androidId != AndroidApplicationId)
            {
                issues.Add($"Android applicationId 应为 {AndroidApplicationId}，当前是 {androidId}");
            }

            var standaloneId = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Standalone);
            if (standaloneId != StandaloneApplicationId)
            {
                issues.Add($"Standalone applicationId 应为 {StandaloneApplicationId}，当前是 {standaloneId}");
            }
        }

        private static void ValidateAndroidSettings(List<string> issues)
        {
            if (PlayerSettings.Android.bundleVersionCode != AndroidReleaseVersionCode)
            {
                issues.Add($"Android versionCode 应为 {AndroidReleaseVersionCode}，当前是 {PlayerSettings.Android.bundleVersionCode}");
            }

            if ((int)PlayerSettings.Android.targetSdkVersion != AndroidTargetSdkVersion)
            {
                issues.Add($"Android Target SDK 应固定为 {AndroidTargetSdkVersion}，当前是 {(int)PlayerSettings.Android.targetSdkVersion}");
            }

            if ((int)PlayerSettings.Android.minSdkVersion != AndroidMinSdkVersion)
            {
                issues.Add($"Android Min SDK 应为 {AndroidMinSdkVersion}，当前是 {(int)PlayerSettings.Android.minSdkVersion}");
            }

            if (PlayerSettings.Android.targetArchitectures != AndroidArchitecture.ARM64)
            {
                issues.Add("Android Target Architectures 应仅启用 ARM64");
            }
        }

        private static void SaveProjectConfiguration()
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
