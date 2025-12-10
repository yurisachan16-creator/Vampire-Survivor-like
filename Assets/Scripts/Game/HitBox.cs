using UnityEngine;
using QFramework;
using UnityEngine.PlayerLoop;

namespace VampireSurvivorLike
{
	public partial class HitBox : GameplayObject
	{
		public GameObject Owner;

        void Awake()
        {
            _mCollider2D = GetComponent<Collider2D>();
        }

        void Start()
        {
            if (Owner == null)
            {
                Owner = transform.parent.gameObject;
            }
            
        }

        private Collider2D _mCollider2D;
        protected override Collider2D Collider2D => _mCollider2D;
    }
}
