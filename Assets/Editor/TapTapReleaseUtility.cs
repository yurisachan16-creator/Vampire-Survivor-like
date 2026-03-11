using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.Linq;
using System.Xml.Linq;
using UnityEngine.Localization.Platform.Android;
using UnityLocalizationSettings = UnityEngine.Localization.Settings.LocalizationSettings;

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
        private const int AndroidTargetSdkVersion = 34;
        private const string AndroidApplicationId = "com.yurisa.nightfallsurvivors";
        private const string StandaloneApplicationId = "com.yurisa.nightfallsurvivors";
        private const string WindowsEntryRelativePath = "Nightfall Survivors.exe";
        private const string AndroidKeyAlias = "nightfallsurvivors-release";
        private const string UnityLocalizationConfigName = "com.unity.localization.settings";
        private const string UnityLocalizationSettingsAssetPath = "Assets/Settings/UnityLocalizationSettings.asset";

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
            TrySyncAndroidPrivacyManifest();
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
            ValidateAndroidPrivacy(issues);
            ValidateAndroidLocalization(issues);

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

        internal static bool TryValidateAndroidReleaseReadiness(out string issueSummary)
        {
            var issues = new List<string>();
            ValidateScenes(issues);
            ValidateBranding(issues);
            ValidateBundleVersion(issues);
            ValidateApplicationIds(issues);
            ValidateAndroidSettings(issues);
            ValidateAndroidSigning(issues);
            ValidateAndroidPrivacy(issues);
            ValidateAndroidLocalization(issues);

            issueSummary = string.Join("\n- ", issues);
            return issues.Count == 0;
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

            if (!File.Exists(PlayerSettings.Android.keystoreName))
            {
                issues.Add($"Android keystore 文件不存在：{PlayerSettings.Android.keystoreName}");
            }

            if (string.IsNullOrWhiteSpace(PlayerSettings.Android.keystorePass))
            {
                issues.Add("Android keystore password 为空，请在 Publishing Settings 重新填写");
            }

            if (string.IsNullOrWhiteSpace(PlayerSettings.Android.keyaliasPass))
            {
                issues.Add("Android key alias password 为空，请在 Publishing Settings 重新填写");
            }

            if (issues.Count == 0 &&
                !TryValidateKeystoreCredentials(
                    PlayerSettings.Android.keystoreName,
                    PlayerSettings.Android.keyaliasName,
                    PlayerSettings.Android.keystorePass,
                    PlayerSettings.Android.keyaliasPass,
                    out var signingError))
            {
                issues.Add($"Android keystore 凭据校验失败：{signingError}");
            }
        }

        private static void ValidateAndroidPrivacy(List<string> issues)
        {
            if (!AndroidPrivacyManifestSync.TryGetConfiguredPrivacyPolicyUrl(out var privacyUrl, out var error))
            {
                issues.Add(error);
                return;
            }

            try
            {
                AndroidPrivacyManifestSync.SyncManifestOrThrow();
            }
            catch (Exception ex)
            {
                issues.Add($"Android 隐私弹窗配置同步失败：{ex.Message}");
                return;
            }

            if (string.IsNullOrWhiteSpace(privacyUrl))
            {
                issues.Add("Android 隐私政策 URL 不能为空");
            }
        }

        private static void TrySyncAndroidPrivacyManifest()
        {
            if (!AndroidPrivacyManifestSync.TryGetConfiguredPrivacyPolicyUrl(out _, out var error))
            {
                Debug.LogWarning($"[TapTap] Android 隐私政策 URL 未配置：{error}");
                return;
            }

            try
            {
                AndroidPrivacyManifestSync.SyncManifestOrThrow();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[TapTap] AndroidManifest 隐私链接同步失败：{ex.Message}");
            }
        }

        private static void ValidateAndroidLocalization(List<string> issues)
        {
            try
            {
                EnsureAndroidLocalizationConfiguredOrThrow();
            }
            catch (Exception ex)
            {
                issues.Add($"Android Localization 配置失败：{ex.Message}");
            }
        }

        internal static void EnsureAndroidLocalizationConfiguredOrThrow()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Settings"))
            {
                AssetDatabase.CreateFolder("Assets", "Settings");
            }

            var settings = UnityLocalizationSettings.GetInstanceDontCreateDefault();
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<UnityLocalizationSettings>();
                AssetDatabase.CreateAsset(settings, UnityLocalizationSettingsAssetPath);
            }

            EditorBuildSettings.AddConfigObject(UnityLocalizationConfigName, settings, true);

            var appInfo = UnityLocalizationSettings.Metadata.GetMetadata<AppInfo>();
            if (appInfo == null)
            {
                appInfo = new AppInfo();
                UnityLocalizationSettings.Metadata.AddMetadata(appInfo);
            }

            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
        }

        private static string NormalizePath(string path)
        {
            return string.IsNullOrWhiteSpace(path)
                ? string.Empty
                : path.Replace("\\", "/");
        }

        private static bool TryValidateKeystoreCredentials(
            string keystorePath,
            string alias,
            string keystorePass,
            string aliasPass,
            out string error)
        {
            error = string.Empty;

            var keytoolPath = ResolveKeytoolPath();
            if (string.IsNullOrWhiteSpace(keytoolPath))
            {
                error = "未找到 keytool，请检查 JAVA_HOME 或 Unity Android OpenJDK 配置";
                return false;
            }

            var args = $"-list -v -keystore {QuoteArg(keystorePath)} -alias {QuoteArg(alias)} -storepass {QuoteArg(keystorePass)}";
            if (!string.IsNullOrEmpty(aliasPass))
            {
                args += $" -keypass {QuoteArg(aliasPass)}";
            }

            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = keytoolPath,
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            try
            {
                using var process = System.Diagnostics.Process.Start(psi);
                if (process == null)
                {
                    error = "无法启动 keytool 进程";
                    return false;
                }

                var stdout = process.StandardOutput.ReadToEnd();
                var stderr = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    return true;
                }

                var normalizedError = string.IsNullOrWhiteSpace(stderr) ? stdout : stderr;
                normalizedError = normalizedError?.Trim();
                if (string.IsNullOrWhiteSpace(normalizedError))
                {
                    normalizedError = $"keytool 返回退出码 {process.ExitCode}";
                }

                error = normalizedError;
                return false;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private static string ResolveKeytoolPath()
        {
            var keytoolName = Application.platform == RuntimePlatform.WindowsEditor ? "keytool.exe" : "keytool";

            var javaHome = Environment.GetEnvironmentVariable("JAVA_HOME");
            if (!string.IsNullOrWhiteSpace(javaHome))
            {
                var fromJavaHome = Path.Combine(javaHome, "bin", keytoolName);
                if (File.Exists(fromJavaHome))
                {
                    return fromJavaHome;
                }
            }

            var editorPath = EditorApplication.applicationPath;
            if (!string.IsNullOrWhiteSpace(editorPath))
            {
                var editorDir = Path.GetDirectoryName(editorPath);
                if (!string.IsNullOrWhiteSpace(editorDir))
                {
                    var unityJdk = Path.GetFullPath(Path.Combine(editorDir, "..", "Data", "PlaybackEngines", "AndroidPlayer", "OpenJDK", "bin", keytoolName));
                    if (File.Exists(unityJdk))
                    {
                        return unityJdk;
                    }
                }
            }

            return keytoolName;
        }

        private static string QuoteArg(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "\"\"";
            }

            return $"\"{value.Replace("\"", "\\\"")}\"";
        }

        private static void SaveProjectConfiguration()
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    internal sealed class AndroidPrivacyManifestSync : IPreprocessBuildWithReport
    {
        internal const string PrivacyPolicyUrlPath = "Release/TapTap/Assets/Policies/privacy-policy-url.txt";
        private const string AndroidManifestPath = "Assets/Plugins/Android/AndroidManifest.xml";
        private const string PrivacyActivityName = "com.unity3d.player.PrivacyActivity";

        public int callbackOrder => -100;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.Android)
            {
                return;
            }

            SyncManifestOrThrow();
        }

        internal static string GetConfiguredPrivacyPolicyUrlOrThrow()
        {
            if (!TryGetConfiguredPrivacyPolicyUrl(out var url, out var error))
            {
                throw new BuildFailedException(error);
            }

            return url;
        }

        internal static bool TryGetConfiguredPrivacyPolicyUrl(out string url, out string error)
        {
            url = string.Empty;
            error = string.Empty;

            var absolutePath = Path.GetFullPath(PrivacyPolicyUrlPath);
            if (!File.Exists(absolutePath))
            {
                error = $"缺少隐私政策 URL 配置文件：{PrivacyPolicyUrlPath}";
                return false;
            }

            var raw = File.ReadAllText(absolutePath).Trim();
            if (string.IsNullOrWhiteSpace(raw))
            {
                error = $"隐私政策 URL 不能为空：{PrivacyPolicyUrlPath}";
                return false;
            }

            if (!Uri.TryCreate(raw, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
            {
                error = $"隐私政策 URL 非法：{raw}";
                return false;
            }

            url = uri.AbsoluteUri;
            return true;
        }

        internal static void SyncManifestOrThrow()
        {
            var privacyUrl = GetConfiguredPrivacyPolicyUrlOrThrow();
            var absoluteManifestPath = Path.GetFullPath(AndroidManifestPath);

            if (!File.Exists(absoluteManifestPath))
            {
                throw new BuildFailedException($"缺少 AndroidManifest 文件：{AndroidManifestPath}");
            }

            var document = XDocument.Load(absoluteManifestPath, LoadOptions.PreserveWhitespace);
            var androidNs = XNamespace.Get("http://schemas.android.com/apk/res/android");

            var activity = document
                .Descendants("activity")
                .FirstOrDefault(element => string.Equals(
                    (string)element.Attribute(androidNs + "name"),
                    PrivacyActivityName,
                    StringComparison.Ordinal));

            if (activity == null)
            {
                throw new BuildFailedException($"AndroidManifest 中缺少 activity：{PrivacyActivityName}");
            }

            var privacyMeta = activity
                .Elements("meta-data")
                .FirstOrDefault(element => string.Equals(
                    (string)element.Attribute(androidNs + "name"),
                    "privacyUrl",
                    StringComparison.Ordinal));

            if (privacyMeta == null)
            {
                privacyMeta = new XElement("meta-data");
                privacyMeta.SetAttributeValue(androidNs + "name", "privacyUrl");
                activity.Add(privacyMeta);
            }

            var currentValue = (string)privacyMeta.Attribute(androidNs + "value") ?? string.Empty;
            if (string.Equals(currentValue, privacyUrl, StringComparison.Ordinal))
            {
                return;
            }

            privacyMeta.SetAttributeValue(androidNs + "value", privacyUrl);
            document.Save(absoluteManifestPath);
        }
    }

    internal sealed class AndroidLocalizationBuildSync : IPreprocessBuildWithReport
    {
        public int callbackOrder => -95;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.Android)
            {
                return;
            }

            TapTapReleaseUtility.EnsureAndroidLocalizationConfiguredOrThrow();
        }
    }
}
