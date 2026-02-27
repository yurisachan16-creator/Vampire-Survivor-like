using UnityEngine;

namespace VampireSurvivorLike
{
    public sealed class Dissolve : MonoBehaviour, ObjectPoolSystem.IPoolable
    {
        public Material Material;
        public Color DissovleColor;

        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int FadeId = Shader.PropertyToID("_Fade");

        private SpriteRenderer _sr;
        private MaterialPropertyBlock _mpb;
        private float _t;
        private bool _playing;
        private Vector3 _baseScale;

        private const float Duration = 0.5f;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            _mpb = new MaterialPropertyBlock();
            _baseScale = transform.localScale;
        }

        private void Update()
        {
            if (!_playing) return;

            _t += Time.deltaTime;
            var p = Mathf.Clamp01(_t / Duration);
            var fade = Mathf.Lerp(1f, 0f, p);

            if (_sr)
            {
                _sr.GetPropertyBlock(_mpb);
                _mpb.SetColor(ColorId, DissovleColor);
                _mpb.SetFloat(FadeId, fade);
                _sr.SetPropertyBlock(_mpb);
            }

            var scale = 1f + (1f - fade) * 0.5f;
            transform.localScale = _baseScale * scale;

            if (p >= 1f)
            {
                _playing = false;
                ObjectPoolSystem.Despawn(gameObject);
            }
        }

        public void OnSpawned()
        {
            if (!_sr) _sr = GetComponent<SpriteRenderer>();
            if (_mpb == null) _mpb = new MaterialPropertyBlock();

            _t = 0f;
            _playing = true;
            _baseScale = transform.localScale;

            if (_sr && Material) _sr.sharedMaterial = Material;
            if (_sr)
            {
                _sr.GetPropertyBlock(_mpb);
                _mpb.SetColor(ColorId, DissovleColor);
                _mpb.SetFloat(FadeId, 1f);
                _sr.SetPropertyBlock(_mpb);
            }
        }

        public void OnDespawned()
        {
            _playing = false;
            _t = 0f;
            transform.localScale = _baseScale;

            if (_sr && _mpb != null)
            {
                _sr.GetPropertyBlock(_mpb);
                _mpb.SetFloat(FadeId, 1f);
                _sr.SetPropertyBlock(_mpb);
            }
        }
    }
}
