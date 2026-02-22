using UnityEngine;
using QFramework;
using System.Linq;

namespace VampireSurvivorLike
{
	public partial class AbilityController : ViewController,IController
	{
        

        void Start()
		{
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

			// 随机解锁一个初始武器
			var expUpgradeSystem = this.GetSystem<ExpUpgradeSystem>();
			expUpgradeSystem.Items.Where(item => item.IsWeapon)
				.ToList()
				.GetRandomItem()
				.Upgrade();

			// 保底解锁铜剑，避免测试阶段被随机池掩盖
			if (expUpgradeSystem.Dictionary.TryGetValue("simple_axe", out var simpleAxeItem)
			    && simpleAxeItem.CurrentLevel.Value == 0)
			{
				simpleAxeItem.Upgrade();
			}

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
