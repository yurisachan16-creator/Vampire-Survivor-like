using UnityEngine;
using QFramework;
using System.Linq;

namespace VampireSurvivorLike
{
	public partial class AbilityController : ViewController,IController
	{
		private MagicWand _magicWand;
		private SimpleBow _simpleBow;

		private void EnsurePhase2Abilities()
		{
			if (!_magicWand)
			{
				_magicWand = GetComponent<MagicWand>();
				if (!_magicWand) _magicWand = gameObject.AddComponent<MagicWand>();
			}

			if (!_simpleBow)
			{
				_simpleBow = GetComponent<SimpleBow>();
				if (!_simpleBow) _simpleBow = gameObject.AddComponent<SimpleBow>();
			}

			var sharedTemplate = SimpleKnife && SimpleKnife.Knife
				? SimpleKnife.Knife.gameObject
				: (SimpleAxe && SimpleAxe.Axe ? SimpleAxe.Axe.gameObject : null);

			_magicWand.SetProjectileSource(sharedTemplate);
			_simpleBow.SetProjectileSource(sharedTemplate);
		}

        void Start()
		{
			EnsurePhase2Abilities();

			Global.SimpleSwordUnlocked.RegisterWithInitValue(unlocked =>
			{
				if (unlocked)
				{
					SimpleSword.Show();
				}
				
			}).UnRegisterWhenGameObjectDestroyed(gameObject);

			Global.SimpleKnifeUnlocked.RegisterWithInitValue(unlocked =>
			{
				if (unlocked)
				{
					SimpleKnife.Show();
				}
				
			}).UnRegisterWhenGameObjectDestroyed(gameObject);

			Global.RotateSwordUnlocked.RegisterWithInitValue(unlocked =>
			{
				if (unlocked)
				{
					RotateSword.Show();
				}
				
			}).UnRegisterWhenGameObjectDestroyed(gameObject);

			Global.BasketBallUnlocked.RegisterWithInitValue(unlocked =>
			{
				if (unlocked)
				{
					BasketBallAbility.Show();
				}
				
			}).UnRegisterWhenGameObjectDestroyed(gameObject);

			Global.SimpleAxeUnlocked.RegisterWithInitValue(unlocked =>
			{
				if (unlocked && SimpleAxe)
				{
					SimpleAxe.Show();
				}
			}).UnRegisterWhenGameObjectDestroyed(gameObject);

			Global.MagicWandUnlocked.RegisterWithInitValue(_ =>
			{
				EnsurePhase2Abilities();
			}).UnRegisterWhenGameObjectDestroyed(gameObject);

			Global.SimpleBowUnlocked.RegisterWithInitValue(_ =>
			{
				EnsurePhase2Abilities();
			}).UnRegisterWhenGameObjectDestroyed(gameObject);

			// 随机解锁一个初始武器
			var expUpgradeSystem = this.GetSystem<ExpUpgradeSystem>();
			expUpgradeSystem.Items.Where(item => item.IsWeapon)
				.ToList()
				.GetRandomItem()
				.Upgrade();

            Global.SuperBomb.RegisterWithInitValue(unlocked =>
            {
                if (unlocked)
                {
                    SuperBomb.Show();
                }
            }).UnRegisterWhenGameObjectDestroyed(gameObject);
		}

		public IArchitecture GetArchitecture()
        {
            return Global.Interface;
        }
	}
}
