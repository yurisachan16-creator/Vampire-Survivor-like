using QFramework;
using TMPro;
using UnityEngine;

namespace VampireSurvivorLike
{
    [DisallowMultipleComponent]
    public sealed class LocalizedTMPText : MonoBehaviour
    {
        [SerializeField] public string Key;
        [SerializeField] public string[] FormatArgs;

        private TMP_Text _text;
        private Color _defaultColor;

        private void Awake()
        {
            _text = GetComponent<TMP_Text>();
            if (_text) FontManager.Register(_text);
            if (_text) _defaultColor = _text.color;

            LocalizationManager.CurrentLanguage.Register(_ => Refresh()).UnRegisterWhenGameObjectDestroyed(gameObject);
            LocalizationManager.ReadyChanged.Register(Refresh).UnRegisterWhenGameObjectDestroyed(gameObject);
            LocalizationDebug.ShowKeys.Register(_ => Refresh()).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnEnable()
        {
            Refresh();
        }

        public void Refresh()
        {
            if (!_text) return;
            if (string.IsNullOrWhiteSpace(Key))
            {
                _text.text = string.Empty;
                return;
            }

            if (FormatArgs != null && FormatArgs.Length > 0)
            {
                var has = LocalizationManager.TryGet(Key, out var template);
                var value = has ? string.Format(template, FormatArgs) : Key;
                if (LocalizationDebug.ShowKeys.Value)
                {
                    _text.text = has ? $"[{Key}] {value}" : $"[{Key}]";
                    _text.color = has ? _defaultColor : Color.red;
                }
                else
                {
                    _text.text = value;
                    _text.color = _defaultColor;
                }
            }
            else
            {
                var has = LocalizationManager.TryGet(Key, out var value);
                if (LocalizationDebug.ShowKeys.Value)
                {
                    _text.text = has ? $"[{Key}] {value}" : $"[{Key}]";
                    _text.color = has ? _defaultColor : Color.red;
                }
                else
                {
                    _text.text = has ? value : Key;
                    _text.color = _defaultColor;
                }
            }
        }
    }
}
