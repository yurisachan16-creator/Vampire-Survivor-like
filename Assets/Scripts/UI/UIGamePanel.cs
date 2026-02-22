using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using QFramework;
using QAssetBundle;

namespace VampireSurvivorLike
{
	public class UIGamePanelData : UIPanelData
	{
	}
	public partial class UIGamePanel : UIPanel
	{
		public static EasyEvent FlashScreen = new EasyEvent(); //屏幕闪烁事件
		public static EasyEvent OpenTreasureChestPanel = new EasyEvent(); //打开宝箱面板事件
		protected override void OnInit(IUIData uiData = null)
		{
			mData = uiData as UIGamePanelData ?? new UIGamePanelData();
			if (Application.isMobilePlatform && !GetComponent<SafeAreaFitter>()) gameObject.AddComponent<SafeAreaFitter>();
			NormalizeHudLayout();
			// please add init code here
			LocalizationManager.PreloadTable("game");
			if (!FindObjectOfType<EventSystem>())
			{
				new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
			}

			MobileTouchOverlay.Ensure(gameObject);

			var tipTransform = transform.Find("Tip");
			if (tipTransform)
			{
				var tooltipView = tipTransform.GetComponent<UITooltipView>();
				if (!tooltipView) tooltipView = tipTransform.gameObject.AddComponent<UITooltipView>();
				tooltipView.Hide();
			}

			var overlayGo = new GameObject("LootGuideOverlay", typeof(RectTransform), typeof(CanvasGroup));
			var overlayRt = (RectTransform)overlayGo.transform;
			overlayRt.SetParent(transform, false);
			overlayRt.anchorMin = Vector2.zero;
			overlayRt.anchorMax = Vector2.one;
			var lootGuideBottomPadding = Mathf.Max(110f, Screen.height * 0.10f);
			overlayRt.offsetMin = new Vector2(0f, lootGuideBottomPadding);
			overlayRt.offsetMax = Vector2.zero;
			overlayRt.pivot = new Vector2(0.5f, 0.5f);
			var cg = overlayGo.GetComponent<CanvasGroup>();
			cg.blocksRaycasts = false;
			cg.interactable = false;

			var lootGuideSystem = gameObject.GetComponent<LootGuideSystem>() ?? gameObject.AddComponent<LootGuideSystem>();
			var canvas = GetComponentInParent<Canvas>();
			lootGuideSystem.Initialize(overlayRt, canvas, Camera.main, Player.Default ? Player.Default.transform : null);
			
			bool IsGameTableReady()
			{
				return LocalizationManager.IsReady && LocalizationManager.TryGet("game.ui.level", out _);
			}

			EnemyGenerator.EnemyCount.RegisterWithInitValue(EnemyCount =>
			{
				if (!IsGameTableReady()) return;
				EnemyCountText.text = LocalizationManager.Format("game.ui.enemy_count", Mathf.Max(0, EnemyCount));
			}).UnRegisterWhenGameObjectDestroyed(gameObject);

			// 时间轴频道信息显示（替代旧波次显示）
			EnemyGenerator.ActiveChannelCount.RegisterWithInitValue(_ =>
			{
				if (!IsGameTableReady()) return;
				EnemyWaveCountText.text = LocalizationManager.Format("game.ui.wave", EnemyGenerator.CurrentMinute.Value, EnemyGenerator.TotalWaveCount.Value);
			}).UnRegisterWhenGameObjectDestroyed(gameObject);

			EnemyGenerator.CurrentMinute.RegisterWithInitValue(_ =>
			{
				if (!IsGameTableReady()) return;
				EnemyWaveCountText.text = LocalizationManager.Format("game.ui.wave", EnemyGenerator.CurrentMinute.Value, EnemyGenerator.TotalWaveCount.Value);
			}).UnRegisterWhenGameObjectDestroyed(gameObject);

			// 剩余时间与活跃频道显示
			EnemyGenerator.GameRemainingTime.RegisterWithInitValue(_ =>
			{
				if (!IsGameTableReady()) return;
				if (string.IsNullOrEmpty(EnemyGenerator.ActiveChannelNames.Value))
				{
					EnemyCountNextTimeText.text = LocalizationManager.T("game.ui.wait_next_wave");
				}
				else
				{
					var remaining = Mathf.CeilToInt(EnemyGenerator.GameRemainingTime.Value);
					EnemyCountNextTimeText.text = LocalizationManager.Format("game.ui.wave_remaining", EnemyGenerator.ActiveChannelNames.Value, remaining);
				}
			}).UnRegisterWhenGameObjectDestroyed(gameObject);

			EnemyGenerator.ActiveChannelNames.RegisterWithInitValue(_ =>
			{
				if (!IsGameTableReady()) return;
				if (string.IsNullOrEmpty(EnemyGenerator.ActiveChannelNames.Value))
				{
					EnemyCountNextTimeText.text = LocalizationManager.T("game.ui.wait_next_wave");
				}
				else
				{
					var remaining = Mathf.CeilToInt(EnemyGenerator.GameRemainingTime.Value);
					EnemyCountNextTimeText.text = LocalizationManager.Format("game.ui.wave_remaining", EnemyGenerator.ActiveChannelNames.Value, remaining);
				}
			}).UnRegisterWhenGameObjectDestroyed(gameObject);

			///时间变化处理
			Global.CurrentSeconds.RegisterWithInitValue((currentSeconds) =>
			{
				if(Time.frameCount %30 == 0)
				{
					if (!IsGameTableReady()) return;
					var secondsInt = Mathf.FloorToInt(currentSeconds);
					var seconds = secondsInt % 60;
					var minutes = secondsInt / 60;
					TimeText.text = LocalizationManager.Format("game.ui.time", $"{minutes:00}:{seconds:00}");
				}
			}).UnRegisterWhenGameObjectDestroyed(gameObject);

			///经验值变化处理
			Global.Exp.RegisterWithInitValue((exp) =>
            {
				//更新经验值显示，使用ExpValue的fillAmount来显示经验值进度
				ExpValue.fillAmount = exp / (float)Global.ExpToNextLevel();
                // ExpText.text = "经验值:(" + exp + "/" + Global.ExpToNextLevel() + ")";
            }).UnRegisterWhenGameObjectDestroyed(gameObject);

			///等级提升处理
			Global.Level.RegisterWithInitValue((level) =>
			{
				if (!IsGameTableReady()) return;
				LevelText.text = LocalizationManager.Format("game.ui.level", level);
				
			}).UnRegisterWhenGameObjectDestroyed(gameObject);

			//默认升级界面隐藏
			ExpUpgradePanel.Hide();
			///等级提升处理：监听升级系统事件，只有在有可选项时才显示面板
			ExpUpgradeSystem.OnUpgradePanelShouldShow.Register((hasItems) =>
			{
				if (hasItems)
				{
					//暂停游戏
					Time.timeScale = 0f;
					//禁用触控覆盖层，避免摇杆拦截升级面板的触摸事件
					MobileTouchOverlay.SetOverlayActive(false);
					//显示升级面板
					ExpUpgradePanel.Show();
					//升级音效
					AudioKit.PlaySound("LevelUp");
				}
				else
				{
					//没有可升级项目，给予补偿奖励
					Global.Coin.Value += 100;
					AudioKit.PlaySound("Retro Event Acute 08");
				}
			}).UnRegisterWhenGameObjectDestroyed(gameObject);

			///经验值转等级处理
			Global.Exp.RegisterWithInitValue((exp) =>
			{
                while (exp >= Global.ExpToNextLevel())
                {
                    Global.Exp.Value -= Global.ExpToNextLevel();
					Global.Level.Value += 1;
					exp = Global.Exp.Value;
                }
			}).UnRegisterWhenGameObjectDestroyed(gameObject);

			FlashScreen.Register(()=>
			{
				//屏幕闪烁效果
				ActionKit
				.Sequence()
				.Lerp(0,0.5f,0.1f
					,alpha=>ScreenColor.ColorAlpha(alpha))
				.Lerp(0.5f,0f,0.1f
					,alpha=>ScreenColor.ColorAlpha(alpha))
				.Start(this);
			}).UnRegisterWhenGameObjectDestroyed(gameObject);

			var enemyGenerator=FindObjectOfType<EnemyGenerator>();
			///游戏时间流逝处理
			ActionKit.OnUpdate.Register(()=>
			{
				Global.CurrentSeconds.Value += Time.deltaTime;
				//敌人全部死亡，通关
				if(enemyGenerator.IsAllWavesFinished && EnemyGenerator.EnemyCount.Value == 0)
				{
					//关闭当前面板
					this.CloseSelf();
					//打开通关面板
					UIKit.OpenPanel<UIGamePassPanel>();
				}

			}).UnRegisterWhenGameObjectDestroyed(gameObject);

			

			
			Global.Coin.RegisterWithInitValue((coin) =>
			{
				if (!IsGameTableReady()) return;
				CoinText.text = LocalizationManager.Format("game.ui.coin", coin);

			}).UnRegisterWhenGameObjectDestroyed(gameObject);

			System.Action refreshHudText = () =>
			{
				if (!LocalizationManager.IsReady) return;
				if (!LocalizationManager.TryGet("game.ui.level", out _)) return;

				EnemyCountText.text = LocalizationManager.Format("game.ui.enemy_count", Mathf.Max(0, EnemyGenerator.EnemyCount.Value));
				EnemyWaveCountText.text = LocalizationManager.Format("game.ui.wave", EnemyGenerator.CurrentMinute.Value, EnemyGenerator.TotalWaveCount.Value);
				if (string.IsNullOrEmpty(EnemyGenerator.ActiveChannelNames.Value))
				{
					EnemyCountNextTimeText.text = LocalizationManager.T("game.ui.wait_next_wave");
				}
				else
				{
					var remaining = Mathf.CeilToInt(EnemyGenerator.GameRemainingTime.Value);
					EnemyCountNextTimeText.text = LocalizationManager.Format("game.ui.wave_remaining", EnemyGenerator.ActiveChannelNames.Value, remaining);
				}

				var secondsInt = Mathf.FloorToInt(Global.CurrentSeconds.Value);
				var seconds = secondsInt % 60;
				var minutes = secondsInt / 60;
				TimeText.text = LocalizationManager.Format("game.ui.time", $"{minutes:00}:{seconds:00}");

				LevelText.text = LocalizationManager.Format("game.ui.level", Global.Level.Value);
				CoinText.text = LocalizationManager.Format("game.ui.coin", Global.Coin.Value);
			};

			LocalizationManager.ReadyChanged.Register(() => refreshHudText()).UnRegisterWhenGameObjectDestroyed(gameObject);
			LocalizationManager.CurrentLanguage.Register(_ => refreshHudText()).UnRegisterWhenGameObjectDestroyed(gameObject);
			refreshHudText();

			OpenTreasureChestPanel.Register(()=>
			{
				Time.timeScale = 0f;
				MobileTouchOverlay.SetOverlayActive(false);
				TreasureChestPanel.Show();
			}).UnRegisterWhenGameObjectDestroyed(gameObject);
		}
		
		protected override void OnOpen(IUIData uiData = null)
		{
		}
		
		protected override void OnShow()
		{
			NormalizeHudLayout();
		}
		
		protected override void OnHide()
		{
		}
		
		protected override void OnClose()
		{
		}

		private void NormalizeHudLayout()
		{
			var root = transform as RectTransform;
			if (root)
			{
				root.offsetMin = Vector2.zero;
				root.offsetMax = Vector2.zero;
				root.anchoredPosition3D = Vector3.zero;
				root.anchorMin = Vector2.zero;
				root.anchorMax = Vector2.one;
				root.localScale = Vector3.one;
			}
		}
	}

	public static class PlatformInput
	{
		private static Vector2 _moveOverride;
		private static bool _hasMoveOverride;
		private static int _backRequestedFrame = -1;

		public static void SetMoveOverride(Vector2 move)
		{
			_moveOverride = move;
			_hasMoveOverride = move.sqrMagnitude > 0.0001f;
		}

		public static Vector2 GetMoveAxisRaw()
		{
			if (_hasMoveOverride) return _moveOverride;
			return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
		}

		public static void RequestBack()
		{
			_backRequestedFrame = Time.frameCount;
		}

		public static bool GetBackDown()
		{
			return Input.GetKeyDown(KeyCode.Escape) || _backRequestedFrame == Time.frameCount;
		}
	}

	[DisallowMultipleComponent]
	public class SafeAreaFitter : MonoBehaviour
	{
		private RectTransform _rect;
		private Rect _lastSafeArea;
		private Vector2Int _lastScreenSize;

		private void Awake()
		{
			_rect = transform as RectTransform;
			Apply();
		}

		private void Update()
		{
			if (_rect == null) return;
			if (_lastSafeArea != Screen.safeArea || _lastScreenSize.x != Screen.width || _lastScreenSize.y != Screen.height)
			{
				Apply();
			}
		}

		private void Apply()
		{
			if (_rect == null) return;
			if (!Application.isMobilePlatform)
			{
				_rect.anchorMin = Vector2.zero;
				_rect.anchorMax = Vector2.one;
				_rect.offsetMin = Vector2.zero;
				_rect.offsetMax = Vector2.zero;
				enabled = false;
				return;
			}

			_lastSafeArea = Screen.safeArea;
			_lastScreenSize = new Vector2Int(Screen.width, Screen.height);

			var anchorMin = _lastSafeArea.position;
			var anchorMax = _lastSafeArea.position + _lastSafeArea.size;
			anchorMin.x /= Screen.width;
			anchorMin.y /= Screen.height;
			anchorMax.x /= Screen.width;
			anchorMax.y /= Screen.height;

			_rect.anchorMin = anchorMin;
			_rect.anchorMax = anchorMax;
			_rect.offsetMin = Vector2.zero;
			_rect.offsetMax = Vector2.zero;
		}
	}

	/// <summary>
	/// 王者荣耀风格虚拟摇杆：左半屏触控区，按下时在触摸点显示，松手后1.5秒自动淡出隐藏
	/// </summary>
	public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
	{
		private RectTransform _touchArea;   // 触控区（左半屏，接收事件的区域）
		private RectTransform _joystickRoot; // 摇杆根节点（动态移动到触摸点）
		private RectTransform _background;
		private RectTransform _handle;
		private CanvasGroup _canvasGroup;
		private float _radius = 140f;
		private Camera _eventCamera;

		private int _activePointerId = int.MinValue;
		private float _hideTimer;
		private const float HideDelay = 1.5f;
		private const float FadeSpeed = 4f;

		public Vector2 Value { get; private set; }

		public void Initialize(RectTransform touchArea, RectTransform joystickRoot,
			RectTransform background, RectTransform handle, CanvasGroup canvasGroup, float radius)
		{
			_touchArea = touchArea;
			_joystickRoot = joystickRoot;
			_background = background;
			_handle = handle;
			_canvasGroup = canvasGroup;
			_radius = Mathf.Max(1f, radius);

			// 初始隐藏
			if (_canvasGroup) _canvasGroup.alpha = 0f;
			ResetState();
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			if (_activePointerId != int.MinValue) return;
			_activePointerId = eventData.pointerId;
			_eventCamera = eventData.pressEventCamera;

			// 将触摸点转换为触控区的局部坐标，将摇杆根节点移动到该位置
			if (_touchArea && _joystickRoot)
			{
				RectTransformUtility.ScreenPointToLocalPointInRectangle(
					_touchArea, eventData.position, _eventCamera, out var localPos);

				// 边界保护：确保摇杆不会超出触控区边界
				var rect = _touchArea.rect;
				localPos.x = Mathf.Clamp(localPos.x, rect.xMin + _radius, rect.xMax - _radius);
				localPos.y = Mathf.Clamp(localPos.y, rect.yMin + _radius, rect.yMax - _radius);

				_joystickRoot.anchoredPosition = localPos;
			}

			// 显示摇杆
			_hideTimer = -1f; // 标记为活跃状态，不计时
			if (_canvasGroup) _canvasGroup.alpha = 1f;

			OnDrag(eventData);
		}

		public void OnDrag(PointerEventData eventData)
		{
			if (_background == null || _handle == null) return;
			if (_activePointerId != eventData.pointerId) return;

			RectTransformUtility.ScreenPointToLocalPointInRectangle(
				_background,
				eventData.position,
				_eventCamera,
				out var localPoint
			);

			var clamped = Vector2.ClampMagnitude(localPoint, _radius);
			_handle.anchoredPosition = clamped;
			Value = clamped / _radius;
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			if (_activePointerId != eventData.pointerId) return;
			ResetState();
			// 开始隐藏计时
			_hideTimer = HideDelay;
		}

		private void Update()
		{
			if (_canvasGroup == null) return;

			if (_hideTimer > 0f)
			{
				_hideTimer -= Time.unscaledDeltaTime;
				if (_hideTimer <= 0f)
				{
					_hideTimer = 0f;
				}
			}

			// 淡出动画
			if (_activePointerId == int.MinValue && _hideTimer <= 0f && _canvasGroup.alpha > 0f)
			{
				_canvasGroup.alpha = Mathf.MoveTowards(_canvasGroup.alpha, 0f, FadeSpeed * Time.unscaledDeltaTime);
			}
		}

		private void OnDisable()
		{
			ResetState();
			if (_canvasGroup) _canvasGroup.alpha = 0f;
		}

		private void ResetState()
		{
			_activePointerId = int.MinValue;
			Value = Vector2.zero;
			if (_handle != null) _handle.anchoredPosition = Vector2.zero;
		}
	}

	[DisallowMultipleComponent]
	public class MobileTouchOverlay : MonoBehaviour
	{
		private VirtualJoystick _joystick;
		private GameObject _overlayRoot;
		private static MobileTouchOverlay _instance;

		/// <summary>
		/// 启用/禁用触控覆盖层（摇杆+暂停按钮），在弹出面板时应禁用以避免事件拦截
		/// </summary>
		public static void SetOverlayActive(bool active)
		{
			if (_instance != null && _instance._overlayRoot != null)
			{
				_instance._overlayRoot.SetActive(active);
				if (!active) PlatformInput.SetMoveOverride(Vector2.zero);
			}
		}

		public static void Ensure(GameObject host)
		{
			if (host == null) return;
			if (!host.GetComponent<MobileTouchOverlay>()) host.AddComponent<MobileTouchOverlay>();
		}

		private void Awake()
		{
			if (!Application.isMobilePlatform)
			{
				Destroy(this);
				return;
			}

			var hostRect = transform as RectTransform;
			if (hostRect && !GetComponent<SafeAreaFitter>()) gameObject.AddComponent<SafeAreaFitter>();

			var overlay = new GameObject("MobileTouchOverlay", typeof(RectTransform));
			var overlayRt = (RectTransform)overlay.transform;
			overlayRt.SetParent(transform, false);
			overlayRt.anchorMin = Vector2.zero;
			overlayRt.anchorMax = Vector2.one;
			overlayRt.offsetMin = Vector2.zero;
			overlayRt.offsetMax = Vector2.zero;

			_overlayRoot = overlay;
			_instance = this;
			_joystick = CreateJoystick(overlayRt);
			CreatePauseButton(overlayRt);
		}

		private void Update()
		{
			if (_joystick != null) PlatformInput.SetMoveOverride(_joystick.Value);
		}

		private void OnDisable()
		{
			PlatformInput.SetMoveOverride(Vector2.zero);
		}

		private void OnDestroy()
		{
			if (_instance == this) _instance = null;
		}

		private VirtualJoystick CreateJoystick(RectTransform parent)
		{
			// === 左半屏触控区：anchorMin(0,0) → anchorMax(0.5,1) ===
			var touchArea = new GameObject("JoystickTouchArea", typeof(RectTransform), typeof(Image));
			var touchRt = (RectTransform)touchArea.transform;
			touchRt.SetParent(parent, false);
			touchRt.anchorMin = Vector2.zero;
			touchRt.anchorMax = new Vector2(0.5f, 1f);
			touchRt.offsetMin = Vector2.zero;
			touchRt.offsetMax = Vector2.zero;

			// 触控区透明但可接收触摸
			var touchImg = touchArea.GetComponent<Image>();
			touchImg.color = Color.clear;
			touchImg.raycastTarget = true;

			// === 摇杆根节点（动态定位到触摸点，带 CanvasGroup 控制淡入淡出） ===
			var joystickRoot = new GameObject("JoystickRoot", typeof(RectTransform), typeof(CanvasGroup));
			var rootRt = (RectTransform)joystickRoot.transform;
			rootRt.SetParent(touchRt, false);
			rootRt.anchorMin = new Vector2(0.5f, 0.5f);
			rootRt.anchorMax = new Vector2(0.5f, 0.5f);
			rootRt.pivot = new Vector2(0.5f, 0.5f);
			rootRt.sizeDelta = new Vector2(280f, 280f);
			rootRt.anchoredPosition = Vector2.zero;

			var canvasGroup = joystickRoot.GetComponent<CanvasGroup>();
			canvasGroup.alpha = 0f;
			canvasGroup.blocksRaycasts = false; // 不阻挡触控区的事件
			canvasGroup.interactable = false;

			// === Background ===
			var bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
			var bgRt = (RectTransform)bg.transform;
			bgRt.SetParent(rootRt, false);
			bgRt.anchorMin = new Vector2(0.5f, 0.5f);
			bgRt.anchorMax = new Vector2(0.5f, 0.5f);
			bgRt.pivot = new Vector2(0.5f, 0.5f);
			bgRt.sizeDelta = new Vector2(280f, 280f);

			// === Handle ===
			var handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
			var handleRt = (RectTransform)handle.transform;
			handleRt.SetParent(bgRt, false);
			handleRt.anchorMin = new Vector2(0.5f, 0.5f);
			handleRt.anchorMax = new Vector2(0.5f, 0.5f);
			handleRt.pivot = new Vector2(0.5f, 0.5f);
			handleRt.sizeDelta = new Vector2(140f, 140f);

			var sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
			var bgImg = bg.GetComponent<Image>();
			bgImg.sprite = sprite;
			bgImg.type = Image.Type.Sliced;
			bgImg.color = new Color(1f, 1f, 1f, 0.18f);
			bgImg.raycastTarget = false;

			var handleImg = handle.GetComponent<Image>();
			handleImg.sprite = sprite;
			handleImg.type = Image.Type.Sliced;
			handleImg.color = new Color(1f, 1f, 1f, 0.35f);
			handleImg.raycastTarget = false;

			// === 将 VirtualJoystick 组件挂在触控区上（接收整个左半屏的触摸事件） ===
			var joy = touchArea.AddComponent<VirtualJoystick>();
			joy.Initialize(touchRt, rootRt, bgRt, handleRt, canvasGroup, 140f);
			return joy;
		}

		private void CreatePauseButton(RectTransform parent)
		{
			var buttonGo = new GameObject("PauseButton", typeof(RectTransform), typeof(Image), typeof(Button));
			var rt = (RectTransform)buttonGo.transform;
			rt.SetParent(parent, false);
			rt.anchorMin = Vector2.one;
			rt.anchorMax = Vector2.one;
			rt.pivot = Vector2.one;
			rt.anchoredPosition = new Vector2(-40f, -40f);
			rt.sizeDelta = new Vector2(120f, 80f);

			var sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
			var img = buttonGo.GetComponent<Image>();
			img.sprite = sprite;
			img.type = Image.Type.Sliced;
			img.color = new Color(0f, 0f, 0f, 0.25f);

			var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
			var textRt = (RectTransform)textGo.transform;
			textRt.SetParent(rt, false);
			textRt.anchorMin = Vector2.zero;
			textRt.anchorMax = Vector2.one;
			textRt.offsetMin = Vector2.zero;
			textRt.offsetMax = Vector2.zero;

			var text = textGo.GetComponent<Text>();
			text.alignment = TextAnchor.MiddleCenter;
			text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			text.text = "Ⅱ";
			text.fontSize = 42;
			text.color = Color.white;

			var button = buttonGo.GetComponent<Button>();
			button.onClick.AddListener(() =>
			{
				AudioKit.PlaySound(Sfx.BUTTONCLICK);
				PlatformInput.RequestBack();
			});
		}
	}
}
