using System.Collections;
using System.Collections.Generic;
using QAssetBundle;
using QFramework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace VampireSurvivorLike
{
    /// <summary>
    /// 见证模式运行时控制器。
    /// 负责标题空闲触发、AI 演示、玩家接管、见证结算与持久化限制。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class WitnessModeRuntime : MonoBehaviour
    {
        private const string WitnessWatchSecondsKey = "WitnessWatchSeconds";
        private const float TitleIdleTriggerSeconds = 30f;
        private const float ResultStaySeconds = 3f;
        private const float WatchSaveIntervalSeconds = 1f;
        private const float MistakeCheckIntervalSeconds = 10f;
        private const float MistakeChance = 0.08f;
        private const float MistakeLowHpThreshold = 0.30f;
        private const float TakeoverLowHpThreshold = 0.15f;
        private const float TakeoverSurviveSeconds = 60f;
        private const float UpgradeHighlightSeconds = 0.5f;
        private const float UpgradeMistakeDelaySeconds = 1.5f;
        private const float TreasureConfirmDelaySeconds = 0.8f;
        private const float RushMistakeDurationSeconds = 0.8f;
        private const float WobbleMistakeDurationSeconds = 1.0f;
        private const float WobbleSwitchIntervalSeconds = 0.16f;
        private const float TakeoverMoveThreshold = 0.35f;
        private const float AiRetargetIntervalSeconds = 0.12f;
        private const float BossMistakeTimeThresholdSeconds = 15f * 60f;
        private const float SafeZoneRadius = 18f;
        private const float BoundaryPredictionDistance = 4f;
        private const float EnemyThreatDistance = 6f;
        private const float BossThreatWeight = 5f;

        private static readonly Vector2[] CandidateDirections =
        {
            Vector2.right,
            (Vector2.right + Vector2.up).normalized,
            Vector2.up,
            (Vector2.left + Vector2.up).normalized,
            Vector2.left,
            (Vector2.left + Vector2.down).normalized,
            Vector2.down,
            (Vector2.right + Vector2.down).normalized
        };

        private static WitnessModeRuntime _instance;
        private static readonly List<Transform> EnemyBuffer = new List<Transform>(1024);
        private static readonly List<Button> UpgradeButtonBuffer = new List<Button>(8);
        private static Texture2D s_hudBorderTexture;
        private static Sprite s_hudBorderSprite;

        private bool _isLoadingWitnessScene;
        private bool _cancelWitnessOnReady;
        private bool _isWitnessRunActive;
        private bool _isWitnessDemoActive;
        private bool _isTakenOver;
        private bool _resultResolved;
        private bool _lastResultWasClear;
        private bool _takeoverLowHpQualified;
        private bool _takeoverClearQualified;
        private bool _takeoverSurviveQualified;
        private bool _observeBossQualified;
        private bool _upgradeMistakePending;
        private float _titleIdleSeconds;
        private float _watchSecondsTotal;
        private float _watchSaveAccumulator;
        private float _takeoverElapsedSeconds;
        private float _resultCountdownSeconds;
        private float _nextMistakeCheckTime;
        private float _aiRetargetAt;
        private float _errorEndTime;
        private float _wobbleSwitchAt;
        private int _coinBaseline;
        private Vector3 _lastMousePosition;
        private Vector2 _currentSafeDirection = Vector2.right;
        private Vector2 _currentDangerDirection = Vector2.left;
        private Vector2 _errorPrimaryDirection = Vector2.right;
        private Vector2 _errorSecondaryDirection = Vector2.up;
        private WitnessMistakeMode _mistakeMode = WitnessMistakeMode.None;
        private WitnessHudView _hudView;
        private Coroutine _loadCoroutine;
        private Coroutine _upgradeCoroutine;
        private Coroutine _treasureCoroutine;

        private enum WitnessMistakeMode
        {
            None,
            RushIntoDanger,
            Wobble
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Bootstrap()
        {
            EnsureInstance();
        }

        private static void EnsureInstance()
        {
            if (_instance) return;

            var runtimeObject = new GameObject(nameof(WitnessModeRuntime));
            DontDestroyOnLoad(runtimeObject);
            _instance = runtimeObject.AddComponent<WitnessModeRuntime>();
        }

        public static bool IsWitnessRunActive => _instance && _instance._isWitnessRunActive;
        public static bool IsWitnessDemoActive => _instance && _instance._isWitnessRunActive && _instance._isWitnessDemoActive && !_instance._isTakenOver;
        public static bool IsWitnessTakenOver => _instance && _instance._isWitnessRunActive && _instance._isTakenOver;
        public static bool AllowCoinAutoPersistence => !_instance || !_instance._isWitnessRunActive;
        public static bool ShouldRecordLeaderboard => !_instance || !_instance._isWitnessRunActive;

        public static float TotalWatchSeconds
        {
            get
            {
                if (_instance) return _instance._watchSecondsTotal;
                return PlayerPrefs.GetFloat(WitnessWatchSecondsKey, 0f);
            }
        }

        public static bool CanWritePlayerPrefsKey(string key)
        {
            if (!_instance || !_instance._isWitnessRunActive) return true;
            if (string.IsNullOrWhiteSpace(key)) return true;
            if (key == WitnessWatchSecondsKey) return true;
            if (key.StartsWith("achievement_first_witness", System.StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        public static string GetResultTitle(bool isClear, string defaultTitle)
        {
            if (!_instance || !_instance._isWitnessRunActive) return defaultTitle;
            if (_instance._isTakenOver) return defaultTitle;
            return isClear ? "见证通关" : "见证结算";
        }

        public static string GetResultSummaryPrefix()
        {
            if (!_instance || !_instance._isWitnessRunActive) return string.Empty;
            return _instance._isTakenOver
                ? "见证接管局（不计入排行榜）"
                : "AI 见证演示局";
        }

        public static bool ShouldAutoReturnFromResult()
        {
            return _instance && _instance._isWitnessRunActive && !_instance._isTakenOver;
        }

        public static bool IsWitnessTakeoverSurviveQualified()
        {
            return _instance && _instance._takeoverSurviveQualified;
        }

        public static bool IsWitnessTakeoverClearQualified()
        {
            return _instance && _instance._takeoverClearQualified;
        }

        public static bool IsWitnessObserveBossQualified()
        {
            return _instance && _instance._observeBossQualified;
        }

        public static void NotifyBossDefeatedByPlayerSide()
        {
            if (!_instance) return;
            if (!_instance._isWitnessRunActive || _instance._isTakenOver) return;
            _instance._observeBossQualified = true;
        }

        private void Awake()
        {
            if (_instance && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            _watchSecondsTotal = PlayerPrefs.GetFloat(WitnessWatchSecondsKey, 0f);
            _lastMousePosition = Input.mousePosition;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
                _instance = null;
            }
        }

        private void Update()
        {
            if (IsTitleScene())
            {
                UpdateTitleIdle();
                return;
            }

            if (IsGameScene())
            {
                UpdateWitnessGameLoop();
                return;
            }

            DestroyHud();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _lastMousePosition = Input.mousePosition;

            if (scene.name == "GameStart")
            {
                ResetTitleIdle();
                if (!_isLoadingWitnessScene)
                {
                    ClearWitnessRunState();
                }
            }
            else if (scene.name == "Game" && _isLoadingWitnessScene)
            {
                CleanupPersistentTitleUi();
            }

            if (scene.name != "Game")
            {
                DestroyHud();
                PlatformInput.ClearWitnessMoveOverride();
            }
        }

        private static bool IsTitleScene()
        {
            return SceneManager.GetActiveScene().name == "GameStart";
        }

        private static bool IsGameScene()
        {
            return SceneManager.GetActiveScene().name == "Game";
        }

        private void UpdateTitleIdle()
        {
            var startPanel = UIKit.GetPanel<UIGameStartPanel>();
            if (!startPanel)
            {
                ResetTitleIdle();
                return;
            }

            if (_isLoadingWitnessScene)
            {
                if (DetectTitleInteraction())
                {
                    _cancelWitnessOnReady = true;
                }
                return;
            }

            if (UIKit.GetPanel<UIGameSettingsPanel>() != null)
            {
                ResetTitleIdle();
                return;
            }

            if ((startPanel.CoinUpgradePanel && startPanel.CoinUpgradePanel.gameObject.activeInHierarchy)
                || (startPanel.AchievementPanel && startPanel.AchievementPanel.gameObject.activeInHierarchy))
            {
                ResetTitleIdle();
                return;
            }

            if (DetectTitleInteraction())
            {
                ResetTitleIdle();
                return;
            }

            _titleIdleSeconds += Time.unscaledDeltaTime;
            if (_titleIdleSeconds < TitleIdleTriggerSeconds) return;

            BeginWitnessLoad();
        }

        private void BeginWitnessLoad()
        {
            if (_isLoadingWitnessScene || _isWitnessRunActive) return;

            ResetTitleIdle();
            _isLoadingWitnessScene = true;
            _cancelWitnessOnReady = false;
            _coinBaseline = Global.Coin.Value;
            _watchSaveAccumulator = 0f;
            _takeoverElapsedSeconds = 0f;
            _takeoverLowHpQualified = false;
            _takeoverClearQualified = false;
            _takeoverSurviveQualified = false;
            _observeBossQualified = false;
            _resultResolved = false;
            _lastResultWasClear = false;
            _mistakeMode = WitnessMistakeMode.None;
            PlatformInput.ClearWitnessMoveOverride();

            Global.ResetData();
            GameSettings.CaptureRunDifficulty();
            AudioKit.PlaySound(Sfx.BUTTONCLICK);

            if (_loadCoroutine != null)
            {
                StopCoroutine(_loadCoroutine);
            }

            _loadCoroutine = StartCoroutine(LoadWitnessSceneRoutine());
        }

        private IEnumerator LoadWitnessSceneRoutine()
        {
            var loadOperation = SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
            if (loadOperation == null)
            {
                _isLoadingWitnessScene = false;
                yield break;
            }

            while (!loadOperation.isDone)
            {
                yield return null;
            }

            _isLoadingWitnessScene = false;

            var deadline = Time.unscaledTime + 10f;
            while (Time.unscaledTime < deadline)
            {
                CleanupPersistentTitleUi();

                if (_cancelWitnessOnReady)
                {
                    ReturnToTitleImmediately();
                    yield break;
                }

                if (Player.Default && UIKit.GetPanel<UIGamePanel>() != null)
                {
                    _loadCoroutine = null;
                    ActivateWitnessDemo();
                    yield break;
                }

                yield return null;
            }

            _loadCoroutine = null;
            ReturnToTitleImmediately();
        }

        private void ActivateWitnessDemo()
        {
            _isWitnessRunActive = true;
            _isWitnessDemoActive = true;
            _isTakenOver = false;
            _resultResolved = false;
            _lastResultWasClear = false;
            _resultCountdownSeconds = ResultStaySeconds;
            _takeoverElapsedSeconds = 0f;
            _nextMistakeCheckTime = Time.unscaledTime + MistakeCheckIntervalSeconds;
            _mistakeMode = WitnessMistakeMode.None;
            _aiRetargetAt = 0f;
            _errorEndTime = 0f;
            _watchSaveAccumulator = 0f;
            PlatformInput.SetWitnessMoveOverride(Vector2.right);
            AudioKit.PlaySound(Sfx.BUTTONCLICK);
        }

        private void UpdateWitnessGameLoop()
        {
            if (!_isWitnessRunActive) return;

            EnsureHud();
            UpdateResultFlow();

            if (_resultResolved)
            {
                UpdateHud();
                return;
            }

            if (!_isTakenOver)
            {
                UpdateWatchSeconds();

                if (TryTakeover())
                {
                    UpdateHud();
                    return;
                }

                if (TryAutoHandleTreasureChest())
                {
                    UpdateHud();
                    return;
                }

                if (TryAutoHandleUpgradePanel())
                {
                    UpdateHud();
                    return;
                }

                UpdateWitnessMovement();
            }
            else
            {
                PlatformInput.ClearWitnessMoveOverride();
                _takeoverElapsedSeconds += Time.unscaledDeltaTime;
                if (_takeoverLowHpQualified && !_takeoverSurviveQualified && _takeoverElapsedSeconds >= TakeoverSurviveSeconds)
                {
                    _takeoverSurviveQualified = true;
                }
            }

            UpdateHud();
        }

        private void UpdateWitnessMovement()
        {
            if (!Player.Default)
            {
                PlatformInput.SetWitnessMoveOverride(Vector2.zero);
                return;
            }

            if (Time.unscaledTime >= _aiRetargetAt)
            {
                _aiRetargetAt = Time.unscaledTime + AiRetargetIntervalSeconds;
                RecalculateThreatDirections(Player.Default.transform.position);
            }

            UpdateMistakeState();
            PlatformInput.SetWitnessMoveOverride(GetCurrentAiDirection());
        }

        private void RecalculateThreatDirections(Vector2 playerPosition)
        {
            EnemyRegistry.AddAllEnemyTransformsTo(EnemyBuffer);

            var bestThreat = float.PositiveInfinity;
            var worstThreat = float.NegativeInfinity;
            var bestDirection = _currentSafeDirection;
            var worstDirection = _currentDangerDirection;

            for (var i = 0; i < CandidateDirections.Length; i++)
            {
                var direction = CandidateDirections[i];
                var threat = EvaluateDirectionThreat(playerPosition, direction);

                if (threat < bestThreat)
                {
                    bestThreat = threat;
                    bestDirection = direction;
                }

                if (threat > worstThreat)
                {
                    worstThreat = threat;
                    worstDirection = direction;
                }
            }

            _currentSafeDirection = bestDirection;
            _currentDangerDirection = worstDirection;
        }

        private static float EvaluateDirectionThreat(Vector2 playerPosition, Vector2 direction)
        {
            var threat = 0f;
            var projectedPosition = playerPosition + direction * BoundaryPredictionDistance;
            var boundaryPenalty = Mathf.Max(0f, projectedPosition.magnitude - SafeZoneRadius);
            threat += boundaryPenalty * 1.4f;

            for (var i = 0; i < EnemyBuffer.Count; i++)
            {
                var enemyTransform = EnemyBuffer[i];
                if (!enemyTransform) continue;

                var offset = (Vector2)enemyTransform.position - playerPosition;
                var distance = offset.magnitude;
                if (distance <= 0.001f) continue;

                var alignment = Vector2.Dot(direction, offset / distance);
                if (alignment <= 0.2f || distance > EnemyThreatDistance) continue;

                var baseThreat = (EnemyThreatDistance - distance) * (0.8f + alignment);
                if (enemyTransform.GetComponent<EnemyMiniBoss>())
                {
                    baseThreat *= BossThreatWeight;
                }

                threat += baseThreat;
            }

            return threat;
        }

        private void UpdateMistakeState()
        {
            if (_mistakeMode != WitnessMistakeMode.None && Time.unscaledTime >= _errorEndTime)
            {
                _mistakeMode = WitnessMistakeMode.None;
            }

            if (Time.unscaledTime < _nextMistakeCheckTime) return;
            _nextMistakeCheckTime = Time.unscaledTime + MistakeCheckIntervalSeconds;

            if (!CanTriggerMistake()) return;
            if (Random.value > MistakeChance) return;

            var roll = Random.Range(0, 3);
            if (roll == 0)
            {
                _mistakeMode = WitnessMistakeMode.RushIntoDanger;
                _errorPrimaryDirection = _currentDangerDirection;
                _errorEndTime = Time.unscaledTime + RushMistakeDurationSeconds;
                return;
            }

            if (roll == 1)
            {
                _upgradeMistakePending = true;
                return;
            }

            _mistakeMode = WitnessMistakeMode.Wobble;
            _errorPrimaryDirection = _currentSafeDirection;
            _errorSecondaryDirection = CandidateDirections[Random.Range(0, CandidateDirections.Length)];
            if (Vector2.Dot(_errorPrimaryDirection, _errorSecondaryDirection) < 0.4f)
            {
                var primaryIndex = System.Array.IndexOf(CandidateDirections, _errorPrimaryDirection);
                _errorSecondaryDirection = CandidateDirections[(primaryIndex + 1 + CandidateDirections.Length) % CandidateDirections.Length];
            }
            _errorEndTime = Time.unscaledTime + WobbleMistakeDurationSeconds;
            _wobbleSwitchAt = Time.unscaledTime + WobbleSwitchIntervalSeconds;
        }

        private bool CanTriggerMistake()
        {
            if (!Player.Default) return false;
            if (EnemyRegistry.BossCount > 0) return false;
            if (Global.CurrentSeconds.Value >= BossMistakeTimeThresholdSeconds) return false;
            if (Global.MaxHP.Value > 0 && Global.HP.Value <= Mathf.CeilToInt(Global.MaxHP.Value * MistakeLowHpThreshold)) return false;

            var gamePanel = UIKit.GetPanel<UIGamePanel>();
            if (gamePanel && gamePanel.ExpUpgradePanel && gamePanel.ExpUpgradePanel.gameObject.activeInHierarchy) return false;
            return true;
        }

        private Vector2 GetCurrentAiDirection()
        {
            switch (_mistakeMode)
            {
                case WitnessMistakeMode.RushIntoDanger:
                    return _errorPrimaryDirection;
                case WitnessMistakeMode.Wobble:
                    if (Time.unscaledTime >= _wobbleSwitchAt)
                    {
                        _wobbleSwitchAt = Time.unscaledTime + WobbleSwitchIntervalSeconds;
                        var swap = _errorPrimaryDirection;
                        _errorPrimaryDirection = _errorSecondaryDirection;
                        _errorSecondaryDirection = swap;
                    }
                    return _errorPrimaryDirection;
                default:
                    return _currentSafeDirection;
            }
        }

        private bool TryTakeover()
        {
            if (!Player.Default) return false;

            var hasTakeoverInput = Application.isMobilePlatform
                ? PlatformInput.GetPlayerMoveAxisRaw().magnitude >= TakeoverMoveThreshold
                : DetectDesktopTakeoverInput();

            if (!hasTakeoverInput) return false;

            _isTakenOver = true;
            _isWitnessDemoActive = false;
            _takeoverElapsedSeconds = 0f;
            _takeoverLowHpQualified = Global.MaxHP.Value > 0 && Global.HP.Value <= Mathf.CeilToInt(Global.MaxHP.Value * TakeoverLowHpThreshold);
            PlatformInput.ClearWitnessMoveOverride();
            AudioKit.PlaySound("Retro Event Acute 08");
            return true;
        }

        private bool TryAutoHandleUpgradePanel()
        {
            var gamePanel = UIKit.GetPanel<UIGamePanel>();
            if (!gamePanel || !gamePanel.ExpUpgradePanel || !gamePanel.ExpUpgradePanel.gameObject.activeInHierarchy)
            {
                StopUpgradeRoutine();
                return false;
            }

            if (_upgradeCoroutine != null) return true;

            gamePanel.ExpUpgradePanel.GetVisibleUpgradeButtons(UpgradeButtonBuffer);
            if (UpgradeButtonBuffer.Count == 0) return true;

            var selectedButton = UpgradeButtonBuffer[Random.Range(0, UpgradeButtonBuffer.Count)];
            var delay = _upgradeMistakePending ? UpgradeMistakeDelaySeconds : UpgradeHighlightSeconds;
            _upgradeMistakePending = false;
            _upgradeCoroutine = StartCoroutine(AutoPickUpgradeRoutine(gamePanel.ExpUpgradePanel, selectedButton, delay));
            return true;
        }

        private IEnumerator AutoPickUpgradeRoutine(ExpUpgradePanel panel, Button button, float delaySeconds)
        {
            panel.SetAutoPickHighlight(button, true);
            yield return new WaitForSecondsRealtime(delaySeconds);

            if (panel && button && button.gameObject.activeInHierarchy && !_isTakenOver && _isWitnessRunActive && !_resultResolved)
            {
                button.onClick.Invoke();
            }

            if (panel && button)
            {
                panel.SetAutoPickHighlight(button, false);
            }

            _upgradeCoroutine = null;
        }

        private bool TryAutoHandleTreasureChest()
        {
            var gamePanel = UIKit.GetPanel<UIGamePanel>();
            if (!gamePanel || !gamePanel.TreasureChestPanel || !gamePanel.TreasureChestPanel.gameObject.activeInHierarchy)
            {
                if (_treasureCoroutine != null)
                {
                    StopCoroutine(_treasureCoroutine);
                    _treasureCoroutine = null;
                }
                return false;
            }

            if (_treasureCoroutine != null) return true;
            _treasureCoroutine = StartCoroutine(AutoConfirmTreasureRoutine(gamePanel.TreasureChestPanel));
            return true;
        }

        private IEnumerator AutoConfirmTreasureRoutine(TreasureChestPanel panel)
        {
            yield return new WaitForSecondsRealtime(TreasureConfirmDelaySeconds);
            if (panel && panel.gameObject.activeInHierarchy && !_isTakenOver && _isWitnessRunActive && !_resultResolved)
            {
                panel.BtnSure.onClick.Invoke();
            }

            _treasureCoroutine = null;
        }

        private void UpdateWatchSeconds()
        {
            _watchSecondsTotal += Time.unscaledDeltaTime;
            _watchSaveAccumulator += Time.unscaledDeltaTime;
            if (_watchSaveAccumulator < WatchSaveIntervalSeconds) return;

            _watchSaveAccumulator = 0f;
            PersistWatchSeconds();
        }

        private void PersistWatchSeconds()
        {
            PlayerPrefs.SetFloat(WitnessWatchSecondsKey, _watchSecondsTotal);
            PlayerPrefs.Save();
        }

        private void UpdateResultFlow()
        {
            if (_resultResolved)
            {
                if (!_isTakenOver)
                {
                    _resultCountdownSeconds -= Time.unscaledDeltaTime;
                    if (_resultCountdownSeconds <= 0f)
                    {
                        ReturnToTitleImmediately();
                    }
                }
                return;
            }

            if (UIKit.GetPanel<UIGamePassPanel>() != null)
            {
                ResolveRunResult(true);
                return;
            }

            if (Global.IsGameOver.Value && UIKit.GetPanel<UIGameOverPanel>() != null)
            {
                ResolveRunResult(false);
            }
        }

        private void ResolveRunResult(bool isClear)
        {
            if (_resultResolved) return;

            _resultResolved = true;
            _lastResultWasClear = isClear;
            PlatformInput.ClearWitnessMoveOverride();
            PersistWatchSeconds();

            if (_isTakenOver)
            {
                if (isClear)
                {
                    _takeoverClearQualified = true;
                    PersistFinalCoinValue(Global.Coin.Value);
                }
                else
                {
                    RestoreCoinBaseline();
                }
            }
            else
            {
                RestoreCoinBaseline();
                _resultCountdownSeconds = ResultStaySeconds;
            }
        }

        private void PersistFinalCoinValue(int coinValue)
        {
            PlayerPrefs.SetInt(nameof(Global.Coin), Mathf.Max(0, coinValue));
            PlayerPrefs.Save();
        }

        private void RestoreCoinBaseline()
        {
            Global.Coin.Value = _coinBaseline;
            PlayerPrefs.SetInt(nameof(Global.Coin), _coinBaseline);
            PlayerPrefs.Save();
        }

        private void ReturnToTitleImmediately()
        {
            if (_loadCoroutine != null)
            {
                StopCoroutine(_loadCoroutine);
                _loadCoroutine = null;
            }

            StopUpgradeRoutine();
            if (_treasureCoroutine != null)
            {
                StopCoroutine(_treasureCoroutine);
                _treasureCoroutine = null;
            }

            PlatformInput.ClearWitnessMoveOverride();
            CleanupPersistentTitleUi();
            DestroyHud();
            ClearWitnessRunState();
            Global.ResetData();
            GameSettings.ClearActiveRunDifficulty();
            SceneManager.LoadScene("GameStart");
        }

        private void ClearWitnessRunState()
        {
            _isLoadingWitnessScene = false;
            _cancelWitnessOnReady = false;
            _isWitnessRunActive = false;
            _isWitnessDemoActive = false;
            _isTakenOver = false;
            _resultResolved = false;
            _lastResultWasClear = false;
            _takeoverLowHpQualified = false;
            _takeoverClearQualified = false;
            _takeoverSurviveQualified = false;
            _observeBossQualified = false;
            _upgradeMistakePending = false;
            _resultCountdownSeconds = ResultStaySeconds;
            _takeoverElapsedSeconds = 0f;
            _mistakeMode = WitnessMistakeMode.None;
            _watchSaveAccumulator = 0f;
            _nextMistakeCheckTime = 0f;
            _aiRetargetAt = 0f;
            _errorEndTime = 0f;
            _wobbleSwitchAt = 0f;
        }

        private void StopUpgradeRoutine()
        {
            if (_upgradeCoroutine != null)
            {
                StopCoroutine(_upgradeCoroutine);
                _upgradeCoroutine = null;
            }

            var gamePanel = UIKit.GetPanel<UIGamePanel>();
            if (gamePanel && gamePanel.ExpUpgradePanel)
            {
                gamePanel.ExpUpgradePanel.ClearAutoPickHighlight();
            }
        }

        private void ResetTitleIdle()
        {
            _titleIdleSeconds = 0f;
            _lastMousePosition = Input.mousePosition;
        }

        private bool DetectTitleInteraction()
        {
            var mousePosition = Input.mousePosition;
            var mouseMoved = (mousePosition - _lastMousePosition).sqrMagnitude > 1f;
            _lastMousePosition = mousePosition;

            if (mouseMoved) return true;
            if (Input.anyKeyDown) return true;
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) return true;
            if (Input.touchCount > 0) return true;
            return false;
        }

        private static bool DetectDesktopTakeoverInput()
        {
            if (Input.anyKeyDown) return true;
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2)) return true;
            return false;
        }

        private void EnsureHud()
        {
            var gamePanel = UIKit.GetPanel<UIGamePanel>();
            if (!gamePanel) return;

            if (_hudView != null && _hudView.Root && _hudView.Root.parent == gamePanel.transform)
            {
                return;
            }

            DestroyHud();

            var rootGo = new GameObject("WitnessHud", typeof(RectTransform), typeof(CanvasGroup));
            var root = rootGo.GetComponent<RectTransform>();
            root.SetParent(gamePanel.transform, false);
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero;
            root.offsetMax = Vector2.zero;

            var group = rootGo.GetComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            group.interactable = false;

            var borderColor = new Color(1f, 0.82f, 0.32f, 0.5f);
            var topBorder = CreateBorder(root, "TopBorder", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -18f), new Vector2(0f, 0f), borderColor);
            var bottomBorder = CreateBorder(root, "BottomBorder", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(0f, 18f), borderColor);
            var leftBorder = CreateBorder(root, "LeftBorder", new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 18f), new Vector2(18f, -18f), borderColor);
            var rightBorder = CreateBorder(root, "RightBorder", new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(-18f, 18f), new Vector2(0f, -18f), borderColor);

            var stateLabel = CreateLabel(root, "WitnessStateLabel", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -24f), TextAnchor.UpperLeft, 28);
            var actionLabel = CreateLabel(root, "WitnessActionLabel", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-24f, 24f), TextAnchor.LowerRight, 26);
            var actionGroup = actionLabel.gameObject.AddComponent<CanvasGroup>();

            _hudView = new WitnessHudView
            {
                Root = root,
                StateLabel = stateLabel,
                ActionLabel = actionLabel,
                ActionGroup = actionGroup,
                Borders = new[] { topBorder, bottomBorder, leftBorder, rightBorder }
            };
        }

        private void UpdateHud()
        {
            if (_hudView == null || !_hudView.Root) return;

            var shouldShow = _isWitnessRunActive && IsGameScene() && !_resultResolved;
            _hudView.Root.gameObject.SetActive(shouldShow);
            if (!shouldShow) return;

            var pulseAlpha = 0.45f + Mathf.PingPong(Time.unscaledTime, 1f) * 0.45f;

            if (!_isTakenOver)
            {
                SetBorderActive(true);
                _hudView.StateLabel.gameObject.SetActive(true);
                _hudView.StateLabel.text = "轮回见证中";
                _hudView.ActionLabel.text = Application.isMobilePlatform ? "拨动摇杆接管" : "按任意键接管";
                _hudView.ActionGroup.alpha = pulseAlpha;
            }
            else
            {
                SetBorderActive(false);
                _hudView.StateLabel.gameObject.SetActive(false);
                _hudView.ActionLabel.text = "见证接管局（不计入排行榜）";
                _hudView.ActionGroup.alpha = 0.92f;
            }
        }

        private void SetBorderActive(bool active)
        {
            if (_hudView?.Borders == null) return;
            for (var i = 0; i < _hudView.Borders.Length; i++)
            {
                if (_hudView.Borders[i]) _hudView.Borders[i].gameObject.SetActive(active);
            }
        }

        private void DestroyHud()
        {
            if (_hudView != null && _hudView.Root)
            {
                Destroy(_hudView.Root.gameObject);
            }

            _hudView = null;
        }

        private static Image CreateBorder(RectTransform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;

            var image = go.GetComponent<Image>();
            image.sprite = GetHudBorderSprite();
            image.type = Image.Type.Simple;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static Sprite GetHudBorderSprite()
        {
            if (s_hudBorderSprite) return s_hudBorderSprite;

            if (!s_hudBorderTexture)
            {
                s_hudBorderTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
                {
                    name = "WitnessHudBorderTexture",
                    hideFlags = HideFlags.HideAndDontSave
                };
                s_hudBorderTexture.SetPixel(0, 0, Color.white);
                s_hudBorderTexture.Apply(false, true);
            }

            s_hudBorderSprite = Sprite.Create(
                s_hudBorderTexture,
                new Rect(0f, 0f, 1f, 1f),
                new Vector2(0.5f, 0.5f),
                1f,
                0u,
                SpriteMeshType.FullRect,
                new Vector4(0f, 0f, 0f, 0f));
            s_hudBorderSprite.name = "WitnessHudBorderSprite";
            return s_hudBorderSprite;
        }

        private static void CleanupPersistentTitleUi()
        {
            UIKit.ClosePanel<UIGamePassPanel>();
            UIKit.ClosePanel<UIGameOverPanel>();
            UIKit.ClosePanel<UIGameSettingsPanel>();
            UIKit.ClosePanel<UIGameLocalLeaderboardPanel>();
            UIKit.ClosePanel<UIGameStartPanel>();
        }

        private static Text CreateLabel(RectTransform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, TextAnchor alignment, int fontSize)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(anchorMax.x, anchorMin.y);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(540f, 72f);

            var text = go.GetComponent<Text>();
            text.font = GetBuiltinFallbackFont();
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = new Color(1f, 0.98f, 0.88f, 0.92f);
            text.raycastTarget = false;
            return text;
        }

        private static Font GetBuiltinFallbackFont()
        {
            try
            {
                return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
            catch
            {
                try
                {
                    return Resources.GetBuiltinResource<Font>("Arial.ttf");
                }
                catch
                {
                    return null;
                }
            }
        }

        private sealed class WitnessHudView
        {
            public RectTransform Root;
            public Text StateLabel;
            public Text ActionLabel;
            public CanvasGroup ActionGroup;
            public Image[] Borders;
        }
    }
}
