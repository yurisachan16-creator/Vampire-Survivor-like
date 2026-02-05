using UnityEngine;

namespace VampireSurvivorLike
{
    public sealed class LocalizationRunner : MonoBehaviour
    {
        private static LocalizationRunner _instance;

        public static LocalizationRunner Instance
        {
            get
            {
                if (_instance) return _instance;
                var go = new GameObject(nameof(LocalizationRunner));
                DontDestroyOnLoad(go);
                _instance = go.AddComponent<LocalizationRunner>();
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
