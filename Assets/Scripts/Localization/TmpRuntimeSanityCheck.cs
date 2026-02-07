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
        }
    }
}
