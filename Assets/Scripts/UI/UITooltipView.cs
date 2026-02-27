using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace VampireSurvivorLike
{
    public class UITooltipView : MonoBehaviour
    {
        [SerializeField] private float YOffset = 18f;

        private RectTransform _tipRect;
        private Canvas _canvas;
        private RectTransform _canvasRect;
        private Text _titleText;
        private Text _descriptionText;

        private void Awake()
        {
            _tipRect = transform as RectTransform;
            _canvas = GetComponentInParent<Canvas>(true);
            _canvasRect = _canvas ? _canvas.transform as RectTransform : null;

            var texts = GetComponentsInChildren<Text>(true);
            _titleText = texts.FirstOrDefault(t => t && t.gameObject.name == "Title");
            _descriptionText = texts.FirstOrDefault(t => t && t.gameObject.name == "Description");

            EnsureNonBlockingRaycast();
            Hide();
        }

        public void ShowFor(RectTransform target, string title, string description)
        {
            if (!_tipRect || !_canvasRect || !_canvas) return;
            if (!target) return;

            if (_titleText) _titleText.text = title ?? string.Empty;
            if (_descriptionText) _descriptionText.text = description ?? string.Empty;

            gameObject.SetActive(true);
            transform.SetAsLastSibling();

            var cam = _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera;

            var corners = new Vector3[4];
            target.GetWorldCorners(corners);
            var worldTopCenter = (corners[1] + corners[2]) * 0.5f;
            var screenPos = RectTransformUtility.WorldToScreenPoint(cam, worldTopCenter);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect, screenPos, cam, out var localPos);
            localPos += new Vector2(0f, YOffset);

            _tipRect.anchoredPosition = ClampToCanvas(localPos);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private Vector2 ClampToCanvas(Vector2 desiredLocalPos)
        {
            var canvasMin = _canvasRect.rect.min;
            var canvasMax = _canvasRect.rect.max;

            var size = _tipRect.rect.size;
            var pivot = _tipRect.pivot;

            var minAllowedX = canvasMin.x + size.x * pivot.x;
            var maxAllowedX = canvasMax.x - size.x * (1f - pivot.x);
            var minAllowedY = canvasMin.y + size.y * pivot.y;
            var maxAllowedY = canvasMax.y - size.y * (1f - pivot.y);

            var clampedX = Mathf.Clamp(desiredLocalPos.x, minAllowedX, maxAllowedX);
            var clampedY = Mathf.Clamp(desiredLocalPos.y, minAllowedY, maxAllowedY);
            return new Vector2(clampedX, clampedY);
        }

        private void EnsureNonBlockingRaycast()
        {
            var cg = GetComponent<CanvasGroup>();
            if (!cg) cg = gameObject.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = false;
            cg.interactable = false;

            foreach (var graphic in GetComponentsInChildren<Graphic>(true))
            {
                if (graphic) graphic.raycastTarget = false;
            }
        }
    }
}

