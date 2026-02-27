using QFramework;
using UnityEngine;

namespace VampireSurvivorLike
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AudioSource))]
    public sealed class LocalizedAudioClip : MonoBehaviour
    {
        [SerializeField] public string AudioKey;
        [SerializeField] public bool PlayOnSet;

        private ResLoader _resLoader;
        private AudioSource _source;

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
            _resLoader = ResLoader.Allocate();

            LocalizationManager.CurrentLanguage.Register(_ => Refresh()).UnRegisterWhenGameObjectDestroyed(gameObject);
            LocalizationManager.ReadyChanged.Register(Refresh).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnEnable()
        {
            Refresh();
        }

        private void OnDestroy()
        {
            if (_resLoader != null)
            {
                _resLoader.Recycle2Cache();
                _resLoader = null;
            }
        }

        public void Refresh()
        {
            if (!_source) return;
            if (string.IsNullOrWhiteSpace(AudioKey)) return;

            AudioClip clip = null;
            foreach (var candidate in LocalizationManager.AssetResolver.GetCandidates(AudioKey, LocalizationManager.CurrentLanguage.Value))
            {
                clip = _resLoader.LoadSync<AudioClip>(candidate);
                if (clip) break;
            }
            if (!clip) return;
            if (_source.clip == clip) return;
            _source.clip = clip;
            if (PlayOnSet) _source.Play();
        }
    }
}
