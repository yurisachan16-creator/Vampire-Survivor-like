using QFramework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;

namespace VampireSurvivorLike
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Image))]
    public sealed class LocalizedSpriteFromAtlas : MonoBehaviour
    {
        [SerializeField] public string AtlasBaseName = "icon";
        [SerializeField] public string SpriteName;

        private ResLoader _resLoader;
        private Image _image;

        private void Awake()
        {
            _image = GetComponent<Image>();
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
            if (!_image) return;
            if (string.IsNullOrWhiteSpace(SpriteName)) return;
            if (string.IsNullOrWhiteSpace(AtlasBaseName)) return;

            var atlas = (SpriteAtlas)null;
            foreach (var candidate in LocalizationManager.AssetResolver.GetCandidates(AtlasBaseName, LocalizationManager.CurrentLanguage.Value))
            {
                atlas = _resLoader.LoadSync<SpriteAtlas>(candidate);
                if (atlas) break;
            }

            if (!atlas) return;
            var sprite = atlas.GetSprite(SpriteName);
            if (sprite) _image.sprite = sprite;
        }
    }
}
