using UnityEngine;

namespace VampireSurvivorLike
{
    [DisallowMultipleComponent]
    public sealed class AttackRangeVisualizer : MonoBehaviour
    {
        private const int SegmentCount = 72;
        private const float LineWidth = 0.02f;

        private readonly Vector3[] _points = new Vector3[SegmentCount + 1];
        private LineRenderer _lineRenderer;
        private float _lastRadius = -1f;

        private void Awake()
        {
            var ringGo = new GameObject("AttackRangeRing");
            ringGo.transform.SetParent(transform, false);
            ringGo.transform.localPosition = Vector3.zero;
            ringGo.transform.localRotation = Quaternion.identity;

            _lineRenderer = ringGo.AddComponent<LineRenderer>();
            _lineRenderer.useWorldSpace = false;
            _lineRenderer.loop = false;
            _lineRenderer.positionCount = SegmentCount + 1;
            _lineRenderer.startWidth = LineWidth;
            _lineRenderer.endWidth = LineWidth;
            _lineRenderer.numCapVertices = 8;
            _lineRenderer.numCornerVertices = 8;
            _lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _lineRenderer.receiveShadows = false;
            _lineRenderer.sortingOrder = 2;
            _lineRenderer.sharedMaterial = CreateMaterial();
            _lineRenderer.startColor = new Color(0.96f, 0.98f, 1f, 0.08f);
            _lineRenderer.endColor = new Color(0.96f, 0.98f, 1f, 0.08f);
            _lineRenderer.enabled = false;
        }

        private static Material CreateMaterial()
        {
            var shader = Shader.Find("Sprites/Default");
            if (!shader) shader = Shader.Find("Unlit/Color");
            return new Material(shader);
        }

        private void LateUpdate()
        {
            if (!_lineRenderer) return;

            var shouldShow = !Global.IsGameOver.Value && Global.SimpleSwordUnlocked.Value;
            if (!shouldShow)
            {
                _lineRenderer.enabled = false;
                return;
            }

            var radius = GetCurrentMeleeRangeRadius();
            if (radius <= 0.01f)
            {
                _lineRenderer.enabled = false;
                return;
            }

            _lineRenderer.enabled = true;

            if (Mathf.Abs(radius - _lastRadius) > 0.001f)
            {
                UpdateCircle(radius);
                _lastRadius = radius;
            }
        }

        private static float GetCurrentMeleeRangeRadius()
        {
            var area = Mathf.Max(1f, Global.AreaMultiplier.Value);
            return Global.SimpleSwordUnlocked.Value
                ? Global.SimpleSwordRange.Value * area * (Global.SuperSword.Value ? 2f : 1f)
                : 0f;
        }

        private void UpdateCircle(float radius)
        {
            var step = Mathf.PI * 2f / SegmentCount;
            for (var i = 0; i <= SegmentCount; i++)
            {
                var angle = step * i;
                _points[i] = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
            }

            _lineRenderer.SetPositions(_points);
        }
    }
}
