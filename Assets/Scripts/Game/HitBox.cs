using UnityEngine;
using QFramework;

namespace VampireSurvivorLike
{
	public partial class HitBox : ViewController
	{
		public GameObject Owner;

        void Start()
        {
            if (Owner == null)
            {
                Owner = transform.parent.gameObject;
            }
        }
    }
}
