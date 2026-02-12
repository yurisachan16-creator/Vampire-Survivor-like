using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace VampireSurvivorLike
{
    [DisallowMultipleComponent]
    public sealed class FloatingTextItem : MonoBehaviour, ObjectPoolSystem.IPoolable
    {
        private Text _text;
        private float _t;
        private float _baseY;
        private bool _playing;

        private const float ScaleInSeconds = 0.5f;
        private const float HoldSeconds = 0.5f;
        private const float FadeOutSeconds = 0.3f;

        public void Play(Vector3 worldPosition, string text, bool critical)
        {
            EnsureRefs();
            transform.position = worldPosition;
            _baseY = worldPosition.y;

            _text.text = text;
            _text.color = critical ? Color.red : Color.white;
            _text.ColorAlpha(1f);
            _text.transform.localScale = Vector3.zero;

            _t = 0f;
            _playing = true;
        }

        private void Update()
        {
            if (!_playing) return;

            _t += Time.deltaTime;

            if (_t < ScaleInSeconds)
            {
                var p = Mathf.Clamp01(_t / ScaleInSeconds);
                transform.position = new Vector3(transform.position.x, _baseY + p * 0.25f, transform.position.z);
                var s = Mathf.Clamp01(p * 4f);
                _text.transform.localScale = new Vector3(s, s, 1f);
                return;
            }

            var afterScale = _t - ScaleInSeconds;
            if (afterScale < HoldSeconds) return;

            var afterHold = afterScale - HoldSeconds;
            var fadeP = Mathf.Clamp01(afterHold / FadeOutSeconds);
            _text.ColorAlpha(1f - fadeP);

            if (fadeP >= 1f)
            {
                _playing = false;
                ObjectPoolSystem.Despawn(gameObject);
            }
        }

        private void EnsureRefs()
        {
            if (_text) return;
            var textTransform = transform.Find("Text");
            _text = textTransform ? textTransform.GetComponent<Text>() : GetComponentInChildren<Text>(true);
        }

        public void OnSpawned()
        {
            _playing = false;
            _t = 0f;
            EnsureRefs();
            if (_text) _text.ColorAlpha(1f);
        }

        public void OnDespawned()
        {
            _playing = false;
        }
    }
}
