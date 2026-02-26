using UnityEngine;
using QFramework;

namespace VampireSurvivorLike
{
	public partial class Coin : PowerUp
	{
		public int CoinValue = 1;

		private bool _hasDefaultScale;
		private Vector3 _defaultScale;

		private void CacheDefaultScale()
		{
			if (_hasDefaultScale) return;
			_defaultScale = transform.localScale;
			_hasDefaultScale = true;
		}

		public void SetCoinValue(int value)
		{
			CacheDefaultScale();
			CoinValue = Mathf.Max(1, value);
			ApplyValueVisual();
		}

		private void ApplyValueVisual()
		{
			if (!_hasDefaultScale) return;
			var multiplier = 1f + Mathf.Clamp(Mathf.Log(CoinValue + 1f), 0f, 4f) * 0.15f;
			transform.localScale = _defaultScale * multiplier;
		}

		private void OnEnable()
		{
			PowerUpRegistry.RegisterCoin(this);
			var sr = GetComponent<SpriteRenderer>();
			LootGuideSystem.Current?.Register(this, LootGuideKind.Coin, sr ? sr.sprite : null);
		}

		private void OnDisable()
		{
			PowerUpRegistry.UnregisterCoin(this);
			LootGuideSystem.Current?.Unregister(this);
		}

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<CollectableAera>())
            {
                FlyingToPalyer = true;
            }
        }

        

        protected override Collider2D Collider2D => SelfCircleCollider2D;

        protected override void Execute()
        {
            AudioKit.PlaySound("Coin");
            Global.Coin.Value += Mathf.Max(1, CoinValue);
			ObjectPoolSystem.Despawn(gameObject);
        }

		public override void OnSpawned()
		{
			base.OnSpawned();
			CacheDefaultScale();
			CoinValue = 1;
			if (_hasDefaultScale) transform.localScale = _defaultScale;
		}

		public override void OnDespawned()
		{
			CoinValue = 1;
			if (_hasDefaultScale) transform.localScale = _defaultScale;
			base.OnDespawned();
		}

	}
}
