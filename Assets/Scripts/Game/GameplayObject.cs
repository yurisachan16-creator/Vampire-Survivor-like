using System.Collections.Generic;
using QFramework;
using QAssetBundle;
using UnityEngine;
using UnityEngine.UI;

namespace VampireSurvivorLike
{
    public abstract class GameplayObject : ViewController
    {
        public bool InScreen { get; set; }
        protected abstract Collider2D Collider2D { get; }
        /// <summary>
        /// 当物体进入摄像机视野时启用碰撞器，离开视野时禁用碰撞器
        /// </summary>
        private void OnBecameVisible()
        {
            Collider2D.enabled = true;
            InScreen = true;
        }

        /// <summary>
        /// 当物体离开摄像机视野时禁用碰撞器
        /// </summary>
        private void OnBecameInvisible()
        {
            Collider2D.enabled = false;
            InScreen = false;
        }
    }

	public enum LootGuideKind
	{
		Exp,
		Coin,
		RecoverHP,
		Wine,
		LemonBuff,
		GetAllExp,
		Bomb,
		TreasureChest
	}

	public sealed class LootGuideSystem : MonoBehaviour
	{
		public static LootGuideSystem Current { get; private set; }

		private const int MaxArrowCount = 16;
		private const float MinGuideDistance = 8f;
		private const float BlinkPeriodSeconds = 0.8f;
		private const float PulseDurationSeconds = 1.2f;

		private readonly Dictionary<int, TargetInfo> _targets = new Dictionary<int, TargetInfo>(64);
		private readonly List<TargetInfo> _visibleTargets = new List<TargetInfo>(64);
		private readonly List<ArrowEntry> _arrowPool = new List<ArrowEntry>(MaxArrowCount);
		private readonly Dictionary<string, AudioClip> _cached3DSfx = new Dictionary<string, AudioClip>(8);

		private RectTransform _overlayRoot;
		private Canvas _overlayCanvas;
		private Camera _worldCamera;
		private Transform _playerTransform;
		private ResLoader _sfxLoader;

		private static readonly int ShaderColorId = Shader.PropertyToID("_Color");
		private static readonly int ShaderProgressId = Shader.PropertyToID("_Progress");

		private void Awake()
		{
			Current = this;
			_sfxLoader = ResLoader.Allocate();
		}

		private void OnDestroy()
		{
			if (ReferenceEquals(Current, this))
			{
				Current = null;
			}

			for (var i = 0; i < _arrowPool.Count; i++)
			{
				if (_arrowPool[i]?.Root)
				{
					Destroy(_arrowPool[i].Root.gameObject);
				}
			}

			_targets.Clear();
			_visibleTargets.Clear();
			_arrowPool.Clear();
			_cached3DSfx.Clear();

			if (_sfxLoader != null)
			{
				_sfxLoader.Recycle2Cache();
				_sfxLoader = null;
			}
		}

		public void Initialize(RectTransform overlayRoot, Canvas overlayCanvas, Camera worldCamera, Transform playerTransform)
		{
			_overlayRoot = overlayRoot;
			_overlayCanvas = overlayCanvas;
			_worldCamera = worldCamera;
			_playerTransform = playerTransform;
		}

		public void Register(GameplayObject target, LootGuideKind kind, Sprite icon)
		{
			if (!target) return;
			_targets[target.GetInstanceID()] = new TargetInfo(target, kind, icon);
		}

		public void Unregister(GameplayObject target)
		{
			if (!target) return;
			_targets.Remove(target.GetInstanceID());
		}

		public void TryPlayDropFeedback(Vector3 worldPosition, LootGuideKind kind)
		{
			if (!GameSettings.EnableLootGuide) return;

			var color = GetRarityColor(kind);
			SpawnPulseRing(worldPosition, color);

			var audioKey = kind == LootGuideKind.Bomb ? Sfx.RETRO_EVENT_UI_01 : Sfx.EXP;
			Spawn3DSound(worldPosition, audioKey, kind == LootGuideKind.Bomb ? 0.25f : 0.2f);
		}

		private void LateUpdate()
		{
			if (!_worldCamera) _worldCamera = Camera.main;
			if (!_playerTransform && Player.Default) _playerTransform = Player.Default.transform;
			if (!_overlayCanvas && _overlayRoot) _overlayCanvas = _overlayRoot.GetComponentInParent<Canvas>();

			if (!GameSettings.EnableLootGuide || !_overlayRoot || !_overlayCanvas || !_worldCamera || !_playerTransform)
			{
				SetArrowActiveCount(0);
				return;
			}

			_visibleTargets.Clear();

			foreach (var kv in _targets)
			{
				var t = kv.Value;
				if (!t.Target) continue;
				if (t.Target.InScreen) continue;

				var distance = Vector2.Distance(_playerTransform.position, t.Target.transform.position);
				if (distance < MinGuideDistance) continue;

				t.Distance = distance;
				_visibleTargets.Add(t);
			}

			_visibleTargets.Sort(TargetInfoComparer.Instance);

			var showCount = Mathf.Min(MaxArrowCount, _visibleTargets.Count);
			EnsureArrowPool(showCount);

			for (var i = 0; i < showCount; i++)
			{
				UpdateArrow(_arrowPool[i], _visibleTargets[i]);
			}

			SetArrowActiveCount(showCount);
		}

		private void EnsureArrowPool(int required)
		{
			while (_arrowPool.Count < required)
			{
				_arrowPool.Add(CreateArrowEntry(_overlayRoot));
			}
		}

		private void SetArrowActiveCount(int activeCount)
		{
			for (var i = 0; i < _arrowPool.Count; i++)
			{
				var shouldBeActive = i < activeCount;
				if (_arrowPool[i].Root && _arrowPool[i].Root.gameObject.activeSelf != shouldBeActive)
				{
					_arrowPool[i].Root.gameObject.SetActive(shouldBeActive);
				}
			}
		}

		private void UpdateArrow(ArrowEntry entry, TargetInfo target)
		{
			var worldPos = target.Target.transform.position;
			var viewportPos = _worldCamera.WorldToViewportPoint(worldPos);

			var dir = new Vector2(viewportPos.x - 0.5f, viewportPos.y - 0.5f);
			if (dir.sqrMagnitude < 0.00001f) dir = Vector2.up;
			dir.Normalize();

			var edgeViewport = ClampToViewportEdge(dir, 0.06f);
			var edgeScreen = _worldCamera.ViewportToScreenPoint(edgeViewport);

			var uiCamera = _overlayCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _overlayCanvas.worldCamera;
			if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_overlayRoot, edgeScreen, uiCamera, out var local))
			{
				entry.Root.anchoredPosition = ClampToOverlay(local, entry.Root.sizeDelta);
			}

			var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
			entry.Root.localRotation = Quaternion.Euler(0f, 0f, angle);

			var size = ComputeArrowSize(target.Distance, _overlayCanvas);
			entry.Root.sizeDelta = new Vector2(size, size);
			entry.Icon.rectTransform.sizeDelta = new Vector2(size * 0.55f, size * 0.55f);

			entry.Icon.sprite = target.Icon;

			var baseColor = GetRarityColor(target.Kind);
			var alpha = Mathf.Lerp(0.35f, 1f,
				0.5f + 0.5f * Mathf.Sin((Time.unscaledTime / BlinkPeriodSeconds) * (2f * Mathf.PI)));
			baseColor.a = alpha;
			entry.Arrow.color = baseColor;
			entry.Icon.color = baseColor;
		}

		private static float ComputeArrowSize(float distance, Canvas canvas)
		{
			var scaleFactor = Mathf.Max(0.0001f, canvas.scaleFactor);
			var maxSize = (Screen.height * 0.08f) / scaleFactor;
			var minSize = maxSize * 0.55f;
			var t = Mathf.InverseLerp(MinGuideDistance, 30f, distance);
			return Mathf.Lerp(minSize, maxSize, t);
		}

		private static Vector2 ClampToViewportEdge(Vector2 dirFromCenter, float margin)
		{
			var center = new Vector2(0.5f, 0.5f);
			var min = new Vector2(margin, margin);
			var max = new Vector2(1f - margin, 1f - margin);

			var t = float.PositiveInfinity;
			if (Mathf.Abs(dirFromCenter.x) > 0.00001f)
			{
				var tx = dirFromCenter.x > 0f ? (max.x - center.x) / dirFromCenter.x : (min.x - center.x) / dirFromCenter.x;
				t = Mathf.Min(t, tx);
			}

			if (Mathf.Abs(dirFromCenter.y) > 0.00001f)
			{
				var ty = dirFromCenter.y > 0f ? (max.y - center.y) / dirFromCenter.y : (min.y - center.y) / dirFromCenter.y;
				t = Mathf.Min(t, ty);
			}

			var p = center + dirFromCenter * t;
			p.x = Mathf.Clamp(p.x, min.x, max.x);
			p.y = Mathf.Clamp(p.y, min.y, max.y);
			return p;
		}

		private Vector2 ClampToOverlay(Vector2 localPos, Vector2 entrySize)
		{
			var rect = _overlayRoot.rect;
			var halfW = entrySize.x * 0.5f;
			var halfH = entrySize.y * 0.5f;
			return new Vector2(
				Mathf.Clamp(localPos.x, rect.xMin + halfW, rect.xMax - halfW),
				Mathf.Clamp(localPos.y, rect.yMin + halfH, rect.yMax - halfH)
			);
		}

		private static ArrowEntry CreateArrowEntry(RectTransform parent)
		{
			var go = new GameObject("LootGuideArrow", typeof(RectTransform));
			var rt = (RectTransform)go.transform;
			rt.SetParent(parent, false);
			rt.anchorMin = new Vector2(0.5f, 0.5f);
			rt.anchorMax = new Vector2(0.5f, 0.5f);
			rt.pivot = new Vector2(0.5f, 0.5f);
			rt.sizeDelta = new Vector2(64f, 64f);

			var arrowGo = new GameObject("Arrow", typeof(RectTransform));
			var arrowRt = (RectTransform)arrowGo.transform;
			arrowRt.SetParent(rt, false);
			arrowRt.anchorMin = Vector2.zero;
			arrowRt.anchorMax = Vector2.one;
			arrowRt.offsetMin = Vector2.zero;
			arrowRt.offsetMax = Vector2.zero;
			arrowRt.pivot = new Vector2(0.5f, 0.5f);

			var arrowGraphic = arrowGo.AddComponent<LootGuideArrowGraphic>();
			arrowGraphic.raycastTarget = false;

			var iconGo = new GameObject("Icon", typeof(RectTransform));
			var iconRt = (RectTransform)iconGo.transform;
			iconRt.SetParent(rt, false);
			iconRt.anchorMin = new Vector2(0.5f, 0.28f);
			iconRt.anchorMax = new Vector2(0.5f, 0.28f);
			iconRt.pivot = new Vector2(0.5f, 0.5f);
			iconRt.sizeDelta = new Vector2(32f, 32f);

			var icon = iconGo.AddComponent<Image>();
			icon.raycastTarget = false;
			icon.preserveAspect = true;

			return new ArrowEntry(rt, arrowGraphic, icon);
		}

		private static Color GetRarityColor(LootGuideKind kind)
		{
			return new Color(0.2f, 0.85f, 1f, 1f);
		}

		private static int GetGuidePriority(LootGuideKind kind)
		{
			switch (kind)
			{
				case LootGuideKind.GetAllExp:
					return 500;
				case LootGuideKind.RecoverHP:
					return 400;
				case LootGuideKind.Wine:
					return 380;
				case LootGuideKind.LemonBuff:
					return 360;
				case LootGuideKind.TreasureChest:
					return 350;
				case LootGuideKind.Bomb:
					return 300;
				case LootGuideKind.Coin:
					return 200;
				default:
					return 100;
			}
		}

		private static void SpawnPulseRing(Vector3 worldPosition, Color color)
		{
			var shader = Shader.Find("VSL/LootPulseRing");
			if (!shader) return;

			var go = new GameObject("LootPulseRing");
			go.transform.position = worldPosition;

			var sr = go.AddComponent<SpriteRenderer>();
			sr.sortingOrder = 50;
			sr.sprite = LootPulseRingSprite.Shared;

			var block = new MaterialPropertyBlock();
			block.SetColor(ShaderColorId, color);
			block.SetFloat(ShaderProgressId, 0f);
			sr.SetPropertyBlock(block);
			sr.sharedMaterial = LootPulseRingMaterial.Get(shader);

			var runner = go.AddComponent<LootPulseRingRunner>();
			runner.Initialize(sr, block, PulseDurationSeconds, ShaderProgressId);
		}

		private static int _active3DSoundCount;
		private const int Max3DSounds = 8;

		private static void Spawn3DSound(Vector3 worldPosition, string audioKey, float volume)
		{
			if (Current == null) return;
			if (string.IsNullOrWhiteSpace(audioKey)) return;
			if (_active3DSoundCount >= Max3DSounds) return;
			if (!SfxThrottle.CanPlay("3D_" + audioKey)) return;

			var clip = Current.GetCached3DSfx(audioKey);
			if (!clip) return;

			_active3DSoundCount++;

			var go = new GameObject("LootGuide3DAudio");
			go.transform.position = worldPosition;

			var source = go.AddComponent<AudioSource>();
			source.playOnAwake = false;
			source.spatialBlend = 1f;
			source.rolloffMode = AudioRolloffMode.Linear;
			source.minDistance = 2f;
			source.maxDistance = 25f;
			source.volume = Mathf.Clamp01(volume);
			source.clip = clip;
			source.Play();

			var lifetime = Mathf.Min(clip.length + 0.2f, 3f);
			Destroy(go, lifetime);

			// 延迟递减并发计数
			if (Current != null)
				Current.StartCoroutine(DecrementAfter(lifetime));
		}

		private AudioClip GetCached3DSfx(string audioKey)
		{
			if (string.IsNullOrWhiteSpace(audioKey)) return null;

			if (_cached3DSfx.TryGetValue(audioKey, out var cached) && cached)
			{
				return cached;
			}

			if (_sfxLoader == null)
			{
				_sfxLoader = ResLoader.Allocate();
			}

			var clip = _sfxLoader.LoadSync<AudioClip>(Sfx.BundleName, audioKey);
			if (clip)
			{
				_cached3DSfx[audioKey] = clip;
			}

			return clip;
		}

		private static System.Collections.IEnumerator DecrementAfter(float delay)
		{
			yield return new WaitForSeconds(delay);
			_active3DSoundCount = Mathf.Max(0, _active3DSoundCount - 1);
		}

		private sealed class LootPulseRingRunner : MonoBehaviour
		{
			private SpriteRenderer _renderer;
			private MaterialPropertyBlock _block;
			private float _duration;
			private float _startTime;
			private int _progressId;

			public void Initialize(SpriteRenderer renderer, MaterialPropertyBlock block, float duration, int progressId)
			{
				_renderer = renderer;
				_block = block;
				_duration = Mathf.Max(0.01f, duration);
				_startTime = Time.unscaledTime;
				_progressId = progressId;
			}

			private void Update()
			{
				if (!_renderer)
				{
					Destroy(gameObject);
					return;
				}

				var t = Mathf.Clamp01((Time.unscaledTime - _startTime) / _duration);
				var scale = Mathf.Lerp(0.75f, 3.2f, t);
				transform.localScale = new Vector3(scale, scale, 1f);

				_block.SetFloat(_progressId, t);
				_renderer.SetPropertyBlock(_block);

				if (t >= 1f)
				{
					Destroy(gameObject);
				}
			}
		}

		private static class LootPulseRingMaterial
		{
			private static Material _shared;

			public static Material Get(Shader shader)
			{
				if (_shared && _shared.shader == shader) return _shared;
				_shared = new Material(shader);
				return _shared;
			}
		}

		private static class LootPulseRingSprite
		{
			private static Sprite _shared;

			public static Sprite Shared
			{
				get
				{
					if (_shared) return _shared;
					var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
					tex.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
					tex.Apply(false, true);
					_shared = Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 100f);
					return _shared;
				}
			}
		}

		private sealed class TargetInfo
		{
			public GameplayObject Target;
			public LootGuideKind Kind;
			public Sprite Icon;
			public float Distance;

			public TargetInfo(GameplayObject target, LootGuideKind kind, Sprite icon)
			{
				Target = target;
				Kind = kind;
				Icon = icon;
				Distance = 0f;
			}
		}

		private sealed class TargetInfoComparer : IComparer<TargetInfo>
		{
			public static readonly TargetInfoComparer Instance = new TargetInfoComparer();

			public int Compare(TargetInfo a, TargetInfo b)
			{
				var kindCompare = GetGuidePriority(b.Kind).CompareTo(GetGuidePriority(a.Kind));
				if (kindCompare != 0) return kindCompare;
				return a.Distance.CompareTo(b.Distance);
			}
		}

		private sealed class ArrowEntry
		{
			public RectTransform Root;
			public LootGuideArrowGraphic Arrow;
			public Image Icon;

			public ArrowEntry(RectTransform root, LootGuideArrowGraphic arrow, Image icon)
			{
				Root = root;
				Arrow = arrow;
				Icon = icon;
			}
		}
	}

	public sealed class LootGuideArrowGraphic : MaskableGraphic
	{
		protected override void OnPopulateMesh(VertexHelper vh)
		{
			vh.Clear();

			var r = GetPixelAdjustedRect();
			var bottomLeft = new Vector2(r.xMin, r.yMin);
			var bottomRight = new Vector2(r.xMax, r.yMin);
			var top = new Vector2((r.xMin + r.xMax) * 0.5f, r.yMax);

			var c = color;
			var v0 = UIVertex.simpleVert;
			v0.color = c;
			v0.position = bottomLeft;
			v0.uv0 = new Vector2(0f, 0f);

			var v1 = UIVertex.simpleVert;
			v1.color = c;
			v1.position = bottomRight;
			v1.uv0 = new Vector2(1f, 0f);

			var v2 = UIVertex.simpleVert;
			v2.color = c;
			v2.position = top;
			v2.uv0 = new Vector2(0.5f, 1f);

			vh.AddVert(v0);
			vh.AddVert(v1);
			vh.AddVert(v2);
			vh.AddTriangle(0, 1, 2);
		}
	}

}
