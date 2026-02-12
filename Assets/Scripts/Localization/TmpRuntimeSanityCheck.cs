using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace VampireSurvivorLike
{
    public static class TmpRuntimeSanityCheck
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (!Debug.isDebugBuild) return;

            var sdf = Shader.Find("TextMeshPro/Distance Field");
            var sdfMobile = Shader.Find("TextMeshPro/Mobile/Distance Field");
            if (!sdf && !sdfMobile)
            {
                Debug.LogError("TMP shader missing. Import TMP Essential Resources and ensure TMP shaders are included for the current render pipeline.");
            }

            var settings = TMP_Settings.instance;
            if (!settings)
            {
                Debug.LogError("TMP_Settings missing. Import TMP Essential Resources.");
                return;
            }

            if (!TMP_Settings.defaultFontAsset)
            {
                Debug.LogWarning("TMP defaultFontAsset is null. Assign the generated Fusion Pixel TMP_FontAsset to TMP Settings or via a bootstrap script.");
            }

            if (IsInvalidFont(TMP_Settings.defaultFontAsset))
            {
                Debug.LogWarning("TMP defaultFontAsset is invalid or missing. Please assign a valid font in Project Settings > TextMeshPro.");
            }

            // Note: fallbackFontAssets is read-only at runtime and must be configured in the editor
            // Validation only - do not attempt to modify at runtime
            if (TMP_Settings.fallbackFontAssets != null && TMP_Settings.fallbackFontAssets.Count > 0)
            {
                for (var i = 0; i < TMP_Settings.fallbackFontAssets.Count; i++)
                {
                    var f = TMP_Settings.fallbackFontAssets[i];
                    if (!f)
                    {
                        Debug.LogWarning($"TMP fallbackFontAssets contains null at index {i}. Please clean up in Project Settings > TextMeshPro.");
                    }
                    else if (IsInvalidFont(f))
                    {
                        Debug.LogWarning($"TMP fallbackFontAssets contains invalid font at index {i}. Please clean up in Project Settings > TextMeshPro.");
                    }
                }
            }
        }

        private static bool IsInvalidFont(TMP_FontAsset font)
        {
            if (!font) return true;
            var atlas = font.atlasTextures;
            if (atlas == null || atlas.Length == 0) return true;
            for (var i = 0; i < atlas.Length; i++)
            {
                if (!atlas[i]) return true;
            }
            return false;
        }
    }
}
