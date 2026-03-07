using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace VampireSurvivorLike.Editor
{
    internal static class TapTapReleaseUtility
    {
        private const string ReleaseCompanyName = "Yurisa Project";
        private const string ReleaseProductName = "Nightfall Survivors";
        private const string ReleaseStoreDisplayName = "夜幕幸存者";
        private const string ReleaseBundleVersion = "1.0.0";
        private const int AndroidReleaseVersionCode = 100;
        private const int AndroidMinSdkVersion = 23;
        private const int AndroidTargetSdkVersion = 35;
        private const string AndroidApplicationId = "com.yurisa.nightfallsurvivors";
        private const string StandaloneApplicationId = "com.yurisa.nightfallsurvivors";
        private const string WindowsEntryRelativePath = "Nightfall Survivors.exe";
        private const string AndroidKeyAlias = "nightfallsurvivors-release";

        private static readonly string AndroidKeystorePath = NormalizePath(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".keystores",
            "NightfallSurvivors",
            "nightfallsurvivors-release.keystore"));

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
            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.Android.keystoreName = AndroidKeystorePath;
            PlayerSettings.Android.keyaliasName = AndroidKeyAlias;
            SaveProjectConfiguration();

            EditorUtility.DisplayDialog(
                "TapTap Android Release",
                $"已应用 Android 发布默认值。\n商店展示名继续使用：{ReleaseStoreDisplayName}\n请在 Unity Publishing Settings 中填入 keystore 密码后再出正式包。",
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
                $"已应用 Windows 发布默认值。\n最终启动路径统一为：{WindowsEntryRelativePath}\n请在打包后使用 Tools/Release/Prepare-TapTapWindowsPackage.ps1 生成纯净压缩包。",
                "确定");
        }

        [MenuItem("Tools/TapTap/Validate Release Readiness")]
        private static void ValidateReleaseReadiness()
        {
            var issues = new List<string>();

            ValidateScenes(issues);
            ValidateBranding(issues);
            ValidateBundleVersion(issues);
            ValidateApplicationIds(issues);
            ValidateAndroidSettings(issues);
            ValidateAndroidSigning(issues);

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
            PlayerSettings.companyName = ReleaseCompanyName;
            PlayerSettings.productName = ReleaseProductName;
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

        private static void ValidateBranding(List<string> issues)
        {
            if (PlayerSettings.companyName != ReleaseCompanyName)
            {
                issues.Add($"companyName 应为 {ReleaseCompanyName}，当前是 {PlayerSettings.companyName}");
            }

            if (PlayerSettings.productName != ReleaseProductName)
            {
                issues.Add($"productName 应为 {ReleaseProductName}，当前是 {PlayerSettings.productName}");
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

        private static void ValidateAndroidSigning(List<string> issues)
        {
            if (!PlayerSettings.Android.useCustomKeystore)
            {
                issues.Add("Android 发布包必须启用自定义 keystore");
            }

            var keystoreName = NormalizePath(PlayerSettings.Android.keystoreName);
            if (keystoreName != AndroidKeystorePath)
            {
                issues.Add($"Android keystore 路径应为 {AndroidKeystorePath}，当前是 {keystoreName}");
            }

            if (PlayerSettings.Android.keyaliasName != AndroidKeyAlias)
            {
                issues.Add($"Android key alias 应为 {AndroidKeyAlias}，当前是 {PlayerSettings.Android.keyaliasName}");
            }
        }

        private static string NormalizePath(string path)
        {
            return string.IsNullOrWhiteSpace(path)
                ? string.Empty
                : path.Replace("\\", "/");
        }

        private static void SaveProjectConfiguration()
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
