using UnityEngine;
using QFramework;
using QAssetBundle;

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
            var cooldownReduction = Mathf.Clamp(Global.CooldownReduction.Value, 0f, 0.75f);
            var attackInterval = Mathf.Max(0.08f, Global.SimpleAxeDuration.Value * (1f - cooldownReduction));

            if (_mCurrentSecond >= attackInterval)
            {
				var projectileCount = Mathf.Max(1, Global.SimpleAxeCount.Value + Global.AdditionalFlyThingCount.Value);
				var superAxe = Global.SuperAxe.Value;
				var damage = Global.SimpleAxeDamage.Value * (superAxe ? 2f : 1f);
				var maxPierce = superAxe ? int.MaxValue : Mathf.Max(1, Global.SimpleAxePierce.Value);

				if (SfxThrottle.CanPlay(Sfx.KNIFE))
				{
					AudioKit.PlaySound(Sfx.KNIFE);
				}

				for (var i = 0; i < projectileCount; i++)
				{
					var go = ObjectPoolSystem.Spawn(Axe.gameObject, null, true);
					if (!go) continue;

					go.transform.position = this.Position();
					go.transform.localScale = Vector3.one * (superAxe ? 1.35f : 1.2f);
					var randomX = RandomUtility.Choose(-6, -4, -2, 2, 4, 6);
					var randomY = RandomUtility.Choose(8, 10, 12);
					var projectile = go.GetComponent<PooledAxeProjectile>();
					if (!projectile) projectile = go.AddComponent<PooledAxeProjectile>();

					projectile.Configure(new Vector2(randomX, randomY), damage, DespawnAbovePlayerDistance, maxPierce, superAxe);
				}

				_mCurrentSecond = 0f;
            }
        }
    }
}
