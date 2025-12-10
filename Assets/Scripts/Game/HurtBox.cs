using UnityEngine;
using QFramework;

namespace VampireSurvivorLike
{
	public partial class HurtBox : GameplayObject
	{
		public GameObject Owner;

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

		private Collider2D _mCollider2D;
        protected override Collider2D Collider2D => _mCollider2D;
	}
}
