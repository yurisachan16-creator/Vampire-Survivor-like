using UnityEngine;
using QFramework;

namespace VampireSurvivorLike
{
	public partial class SimpleAxe : ViewController
	{
		void Start()
		{
			// Code Here
		}

		private float _mCurrentSecond=0;

        void Update()
        {
            _mCurrentSecond+=Time.deltaTime;

            if (_mCurrentSecond >= 1.0f)
            {
				var go = ObjectPoolSystem.Spawn(Axe.gameObject, null, true);
				if (go)
				{
					go.transform.position = this.Position();
					var randomX = RandomUtility.Choose(-8, -5, -3, 3, 5, 8);
					var randomY = RandomUtility.Choose(3, 5, 8);
					var projectile = go.GetComponent<PooledAxeProjectile>();
					if (!projectile) projectile = go.AddComponent<PooledAxeProjectile>();
					projectile.Configure(new Vector2(randomX, randomY), 2f, 15f);
				}

				_mCurrentSecond=0;
            }
        }
    }
}
