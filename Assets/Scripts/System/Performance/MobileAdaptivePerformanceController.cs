using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace VampireSurvivorLike
{
    [DisallowMultipleComponent]
    public sealed class MobileAdaptivePerformanceController : MonoBehaviour
    {
        private enum AdaptiveMode
        {
            Normal,
            Degraded
        }

        private const float LowFpsThreshold = 55f;
        private const float HighFpsThreshold = 58f;
        private const float LowFpsDurationSeconds = 3f;
        private const float HighFpsDurationSeconds = 8f;
        private const float CameraRefreshIntervalSeconds = 0.5f;

        private static MobileAdaptivePerformanceController _instance;

        private AdaptiveMode _mode = AdaptiveMode.Normal;
        private float _smoothedDeltaTime = 1f / 60f;
        private float _lowFpsTimer;
        private float _highFpsTimer;

        private Camera _cachedMainCamera;
        private UniversalAdditionalCameraData _cachedCameraData;
        private float _nextCameraRefreshTime;
        private bool _hasBaselinePostProcessing;
        private bool _baselinePostProcessingEnabled = true;

        public static bool IsDegraded => _instance && _instance._mode == AdaptiveMode.Degraded;
        public static string CurrentModeLabel => IsDegraded ? "Degraded" : "Normal";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (_instance) return;

            var go = new GameObject("MobileAdaptivePerformanceController");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<MobileAdaptivePerformanceController>();
        }

        private void Update()
        {
            RefreshCameraReference();

            if (!IsAndroidAdaptivePerformanceEnabled())
            {
                if (_mode != AdaptiveMode.Normal)
                {
                    EnterNormalMode();
                }
                return;
            }

            _smoothedDeltaTime = Mathf.Lerp(_smoothedDeltaTime, Time.unscaledDeltaTime, 0.08f);
            var fps = 1f / Mathf.Max(0.0001f, _smoothedDeltaTime);

            if (_mode == AdaptiveMode.Normal)
            {
                _highFpsTimer = 0f;
                if (fps < LowFpsThreshold)
                {
                    _lowFpsTimer += Time.unscaledDeltaTime;
                    if (_lowFpsTimer >= LowFpsDurationSeconds)
                    {
                        EnterDegradedMode();
                    }
                }
                else
                {
                    _lowFpsTimer = 0f;
                }
            }
            else
            {
                _lowFpsTimer = 0f;
                if (fps > HighFpsThreshold)
                {
                    _highFpsTimer += Time.unscaledDeltaTime;
                    if (_highFpsTimer >= HighFpsDurationSeconds)
                    {
                        EnterNormalMode();
                    }
                }
                else
                {
                    _highFpsTimer = 0f;
                }
            }

            ApplyModeEffects();
        }

        private void OnDisable()
        {
            EnsureNormalMode();
        }

        private void OnDestroy()
        {
            EnsureNormalMode();
            if (_instance == this) _instance = null;
        }

        private static bool IsAndroidAdaptivePerformanceEnabled()
        {
            if (!Application.isMobilePlatform) return false;
            if (Application.platform != RuntimePlatform.Android) return false;
            return GameSettings.EnableAdaptiveMobilePerformance;
        }

        private void EnsureNormalMode()
        {
            if (_mode == AdaptiveMode.Normal) return;
            EnterNormalMode();
        }

        private void EnterDegradedMode()
        {
            if (_mode == AdaptiveMode.Degraded) return;

            _mode = AdaptiveMode.Degraded;
            _lowFpsTimer = 0f;
            _highFpsTimer = 0f;
            ApplyModeEffects();
        }

        private void EnterNormalMode()
        {
            if (_mode == AdaptiveMode.Normal) return;

            _mode = AdaptiveMode.Normal;
            _lowFpsTimer = 0f;
            _highFpsTimer = 0f;
            ApplyModeEffects();
        }

        private void ApplyModeEffects()
        {
            LootGuideSystem.SetExpDropPulseFeedbackEnabled(_mode == AdaptiveMode.Normal);

            if (!_cachedCameraData) return;

            var targetPostProcessing = _mode == AdaptiveMode.Degraded
                ? false
                : (_hasBaselinePostProcessing ? _baselinePostProcessingEnabled : true);

            if (_cachedCameraData.renderPostProcessing != targetPostProcessing)
            {
                _cachedCameraData.renderPostProcessing = targetPostProcessing;
            }
        }

        private void RefreshCameraReference()
        {
            if (_cachedMainCamera && _cachedMainCamera.isActiveAndEnabled && Time.unscaledTime < _nextCameraRefreshTime)
            {
                return;
            }

            if (Time.unscaledTime < _nextCameraRefreshTime && _cachedMainCamera == null)
            {
                return;
            }

            _nextCameraRefreshTime = Time.unscaledTime + CameraRefreshIntervalSeconds;
            var mainCamera = Camera.main;
            if (!mainCamera)
            {
                _cachedMainCamera = null;
                _cachedCameraData = null;
                _hasBaselinePostProcessing = false;
                return;
            }

            var cameraChanged = _cachedMainCamera != mainCamera;
            _cachedMainCamera = mainCamera;

            if (!cameraChanged && _cachedCameraData) return;

            if (_cachedMainCamera.TryGetComponent<UniversalAdditionalCameraData>(out var cameraData))
            {
                _cachedCameraData = cameraData;
                _baselinePostProcessingEnabled = cameraData.renderPostProcessing;
                _hasBaselinePostProcessing = true;
            }
            else
            {
                _cachedCameraData = null;
                _hasBaselinePostProcessing = false;
            }
        }
    }
}
