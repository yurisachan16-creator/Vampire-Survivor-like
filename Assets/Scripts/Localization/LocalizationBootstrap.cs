using UnityEngine;

namespace VampireSurvivorLike
{
    public static class LocalizationBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            LocalizationManager.Initialize();
        }
    }
}
