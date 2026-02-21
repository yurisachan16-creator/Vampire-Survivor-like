using UnityEngine;
using QFramework;

namespace VampireSurvivorLike
{
	public partial class SimpleAxe : ViewController
	{
		private float _mCurrentSecond = 0f;
		private const float DespawnAbovePlayerDistance = 15f;

		void Start()
		{
			if (Axe)
			{
				Axe.Hide();
			}
		}

        void Update()
        {
            _mCurrentSecond += Time.deltaTime;

            if (_mCurrentSecond >= Global.SimpleAxeDuration.Value)
            {
				var projectileCount = Mathf.Max(1, Global.SimpleAxeCount.Value + Global.AdditionalFlyThingCount.Value);
				for (var i = 0; i < projectileCount; i++)
				{
					var go = ObjectPoolSystem.Spawn(Axe.gameObject, null, true);
					if (!go) continue;

					go.transform.position = this.Position();
					var randomX = RandomUtility.Choose(-8, -5, -3, 3, 5, 8);
					var randomY = RandomUtility.Choose(3, 5, 8);
					var projectile = go.GetComponent<PooledAxeProjectile>();
					if (!projectile) projectile = go.AddComponent<PooledAxeProjectile>();

					var superAxe = Global.SuperAxe.Value;
					var damage = Global.SimpleAxeDamage.Value * (superAxe ? 2f : 1f);
					var maxPierce = superAxe ? int.MaxValue : Mathf.Max(1, Global.SimpleAxePierce.Value);
					projectile.Configure(new Vector2(randomX, randomY), damage, DespawnAbovePlayerDistance, maxPierce, superAxe);
				}

				_mCurrentSecond = 0f;
            }
        }
    }
}
