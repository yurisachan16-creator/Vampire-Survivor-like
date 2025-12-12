using UnityEngine;
using QFramework;

namespace VampireSurvivorLike
{
	public partial class HitHurtBox : GameplayObject
	{
		public GameObject Owner;

		private Collider2D _mCollider2D;

		void Awake()
		{
			_mCollider2D = GetComponent<Collider2D>();
		}

		void Start()
		{
			if (!Owner)
			{
				Owner = transform.parent.gameObject;
			}
		}

		protected override Collider2D Collider2D => _mCollider2D;
	}
}
