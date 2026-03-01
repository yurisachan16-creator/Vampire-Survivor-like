using UnityEngine;
using QFramework;

namespace VampireSurvivorLike
{
	public partial class HitHurtBox : GameplayObject
	{
		public GameObject Owner;

		public bool IsEnemyOwner
		{
			get
			{
				EnsureOwnerCacheFresh();
				return _isEnemyOwner;
			}
		}

		public IEnemy CachedEnemy
		{
			get
			{
				EnsureOwnerCacheFresh();
				return _cachedEnemy;
			}
		}

		public EnemyMiniBoss CachedMiniBoss
		{
			get
			{
				EnsureOwnerCacheFresh();
				return _cachedMiniBoss;
			}
		}

		public Rigidbody2D CachedOwnerRigidbody
		{
			get
			{
				EnsureOwnerCacheFresh();
				return _cachedOwnerRigidbody;
			}
		}

		private Collider2D _mCollider2D;
		private GameObject _cachedOwner;
		private bool _isEnemyOwner;
		private IEnemy _cachedEnemy;
		private EnemyMiniBoss _cachedMiniBoss;
		private Rigidbody2D _cachedOwnerRigidbody;

		void Awake()
		{
			_mCollider2D = GetComponent<Collider2D>();
			if (!Owner && transform.parent)
			{
				Owner = transform.parent.gameObject;
			}

			RefreshOwnerCache();
		}

		void OnEnable()
		{
			if (!Owner && transform.parent)
			{
				Owner = transform.parent.gameObject;
			}

			RefreshOwnerCache();
		}

		void Start()
		{
			if (!Owner && transform.parent)
			{
				Owner = transform.parent.gameObject;
				RefreshOwnerCache();
			}
		}

		public void SetOwner(GameObject owner)
		{
			if (Owner == owner) return;
			Owner = owner;
			RefreshOwnerCache();
		}

		public void RefreshOwnerCache()
		{
			_cachedOwner = Owner;
			_isEnemyOwner = _cachedOwner && _cachedOwner.CompareTag("Enemy");

			if (_cachedOwner)
			{
				_cachedEnemy = _cachedOwner.GetComponent<IEnemy>();
				_cachedMiniBoss = _cachedOwner.GetComponent<EnemyMiniBoss>();
				_cachedOwnerRigidbody = _cachedOwner.GetComponent<Rigidbody2D>();
			}
			else
			{
				_cachedEnemy = null;
				_cachedMiniBoss = null;
				_cachedOwnerRigidbody = null;
			}
		}

		public bool TryGetEnemy(out IEnemy enemy)
		{
			EnsureOwnerCacheFresh();
			enemy = _cachedEnemy;
			return enemy != null;
		}

		private void EnsureOwnerCacheFresh()
		{
			if (_cachedOwner == Owner) return;
			RefreshOwnerCache();
		}

		protected override Collider2D Collider2D => _mCollider2D;
	}
}
