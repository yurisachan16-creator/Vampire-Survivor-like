using UnityEngine;
using QFramework;

namespace VampireSurvivorLike
{
	public partial class Exp : PowerUp
	{
		public int ExpValue = 1;

		private bool _hasDefaultScale;
		private Vector3 _defaultScale;

		private void CacheDefaultScale()
		{
			if (_hasDefaultScale) return;
			_defaultScale = transform.localScale;
			_hasDefaultScale = true;
		}

		public void SetExpValue(int value)
		{
			CacheDefaultScale();
			ExpValue = Mathf.Max(1, value);
			ApplyValueVisual();
		}

		private void ApplyValueVisual()
		{
			if (!_hasDefaultScale) return;
			var multiplier = 1f + Mathf.Clamp(Mathf.Log(ExpValue + 1f), 0f, 4f) * 0.18f;
			transform.localScale = _defaultScale * multiplier;
		}

		private void OnEnable()
		{
			PowerUpRegistry.RegisterExp(this);
			var sr = GetComponent<SpriteRenderer>();
			LootGuideSystem.Current?.Register(this, LootGuideKind.Exp, sr ? sr.sprite : null);
			LootGuideSystem.Current?.TryPlayDropFeedback(transform.position, LootGuideKind.Exp);
		}

		private void OnDisable()
		{
			PowerUpRegistry.UnregisterExp(this);
			LootGuideSystem.Current?.Unregister(this);
		}
        
        
        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<CollectableAera>())
            {
                FlyingToPalyer = true;
                
            }
        }

        
        

        //执行方法
        protected override void Execute()
        {
            if (SfxThrottle.CanPlay("Exp", 0.1f))
                AudioKit.PlaySound("Exp");
            Global.Exp.Value += Mathf.Max(1, ExpValue);
			ObjectPoolSystem.Despawn(gameObject);
        }

		public override void OnSpawned()
		{
			base.OnSpawned();
			CacheDefaultScale();
			ExpValue = 1;
			if (_hasDefaultScale) transform.localScale = _defaultScale;
		}

		public override void OnDespawned()
		{
			ExpValue = 1;
			if (_hasDefaultScale) transform.localScale = _defaultScale;
			base.OnDespawned();
		}

        protected override Collider2D Collider2D => SelfCircleCollider2D;
    }
}
