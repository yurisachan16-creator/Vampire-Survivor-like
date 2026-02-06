using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace VampireSurvivorLike
{
    public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private float LongPressSeconds = 0.35f;

        private UITooltipView _tooltipView;
        private RectTransform _target;
        private string _title;
        private string _description;

        private Coroutine _longPressCoroutine;
        private bool _isPointerOver;
        private bool _shownByLongPress;

        public void SetTooltipView(UITooltipView tooltipView)
        {
            _tooltipView = tooltipView;
        }

        public void SetTarget(RectTransform target)
        {
            _target = target;
        }

        public void SetContent(string title, string description)
        {
            _title = title;
            _description = description;
            if ((_title == null && _description == null) && _tooltipView)
            {
                _tooltipView.Hide();
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isPointerOver = true;
            if (eventData != null && eventData.pointerId >= 0) return;
            Show();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isPointerOver = false;
            CancelLongPress();
            Hide();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_isPointerOver && eventData != null && eventData.pointerId < 0) return;
            if (_longPressCoroutine != null) return;
            _longPressCoroutine = StartCoroutine(LongPressRoutine());
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            CancelLongPress();
            if (eventData != null && eventData.pointerId >= 0)
            {
                Hide();
            }
            else
            {
                if (_shownByLongPress && !_isPointerOver) Hide();
            }
        }

        private IEnumerator LongPressRoutine()
        {
            var elapsed = 0f;
            while (elapsed < LongPressSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            _shownByLongPress = true;
            Show();
            _longPressCoroutine = null;
        }

        private void Show()
        {
            if (!_target) _target = transform as RectTransform;
            if (!_tooltipView) _tooltipView = Object.FindObjectOfType<UITooltipView>(true);
            if (!_tooltipView) return;
            if (_title == null && _description == null) return;

            _tooltipView.ShowFor(_target, _title ?? string.Empty, _description ?? string.Empty);
        }

        private void Hide()
        {
            _shownByLongPress = false;
            if (_tooltipView) _tooltipView.Hide();
        }

        private void CancelLongPress()
        {
            if (_longPressCoroutine != null)
            {
                StopCoroutine(_longPressCoroutine);
                _longPressCoroutine = null;
            }
        }
    }
}

