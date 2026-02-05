using System.Collections.Generic;
using QFramework;
using UnityEngine;

namespace VampireSurvivorLike
{
    public static class LocalizationDebug
    {
        public static BindableProperty<bool> ShowKeys { get; } = new BindableProperty<bool>(false);
        public static HashSet<string> MissingKeys { get; } = new HashSet<string>();

        public static void ToggleShowKeys()
        {
            ShowKeys.Value = !ShowKeys.Value;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            var go = new GameObject(nameof(LocalizationDebugOverlay));
            Object.DontDestroyOnLoad(go);
            go.AddComponent<LocalizationDebugOverlay>();
        }
    }
}
