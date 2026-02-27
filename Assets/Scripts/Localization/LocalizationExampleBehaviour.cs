using UnityEngine;

namespace VampireSurvivorLike
{
    public sealed class LocalizationExampleBehaviour : MonoBehaviour
    {
        [SerializeField] private string Key = "ui.settings.fullscreen";

        private void Start()
        {
            LocalizationManager.Initialize();
            var value = LocalizationManager.T(Key);
            Debug.Log(value);
        }
    }
}
