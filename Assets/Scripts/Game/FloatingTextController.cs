using UnityEngine;
using QFramework;
using UnityEngine.UI;

namespace VampireSurvivorLike
{
	public partial class FloatingTextController : ViewController
	{
		private const int MaxCachedDamageText = 8192;
		private const int MaxActiveTextsDesktop = 96;
		private const int MaxActiveTextsMobile = 48;
		private static readonly string[] DamageTextCache = BuildDamageTextCache();
		private static int _activeTextCount;

		void Start()
		{
			FloatingText.Hide();
		}

		public static void PlayDamage(Vector2 position, float damage, bool critical = false)
		{
			if (!_mDefault || !_mDefault.FloatingText) return;
			if (!critical && _activeTextCount >= GetMaxActiveTextCount()) return;

			Play(position, GetDamageText(damage), critical);
		}

		public static void Play(Vector2 position,string text,bool critical=false)
        {
			if (!_mDefault || !_mDefault.FloatingText) return;

			var go = ObjectPoolSystem.Spawn(_mDefault.FloatingText.gameObject, _mDefault.transform, true);
			if (!go) return;
			go.transform.position = position;

			var item = go.GetComponent<FloatingTextItem>();
			var addedItem = false;
			if (!item)
			{
				item = go.AddComponent<FloatingTextItem>();
				ObjectPoolSystem.RefreshPoolableCache(go);
				addedItem = true;
			}
			if (addedItem) item.OnSpawned();
			item.Play(position, text, critical);
        }

		private static FloatingTextController _mDefault;

		internal static void NotifyItemSpawned()
		{
			_activeTextCount++;
		}

		internal static void NotifyItemDespawned()
		{
			if (_activeTextCount > 0) _activeTextCount--;
		}

        void Awake()
        {
            _mDefault = this;
			_activeTextCount = 0;
        }

        void OnDestroy()
        {
            _mDefault = null;
        }

		private static int GetMaxActiveTextCount()
		{
			return Application.isMobilePlatform ? MaxActiveTextsMobile : MaxActiveTextsDesktop;
		}

		private static string GetDamageText(float damage)
		{
			var rounded = Mathf.Max(0, Mathf.RoundToInt(damage));
			if (rounded < DamageTextCache.Length) return DamageTextCache[rounded];
			return rounded.ToString();
		}

		private static string[] BuildDamageTextCache()
		{
			var cache = new string[MaxCachedDamageText];
			for (var i = 0; i < cache.Length; i++)
			{
				cache[i] = i.ToString();
			}
			return cache;
		}
    }
}
