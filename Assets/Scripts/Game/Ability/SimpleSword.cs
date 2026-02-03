using UnityEngine;
using QFramework;
using MoonSharp.VsCodeDebugger.SDK;
using System.Linq;

namespace VampireSurvivorLike
{
	public partial class SimpleSword : ViewController
	{
		private float _mCurrentSecond=0;

		

		void Start()
		{
			// Code Here
		}

        void Update()
        {
            _mCurrentSecond += Time.deltaTime;
			if(_mCurrentSecond>=Global.SimpleAbilityDuration.Value)
			{
                _mCurrentSecond = 0;

				//计算数量和伤害倍数
				var countTimes=Global.SuperSword.Value ? 2 : 1;
				var damageTimes=Global.SuperSword.Value ? Random.Range(2,3+1) : 1;
				var distanceTimes=Global.SuperSword.Value ? 2 : 1;

				var enemies = FindObjectsByType<Enemy>(FindObjectsInactive.Exclude,FindObjectsSortMode.None);

				foreach(var enemy in enemies
									.OrderBy(e=>e.Direction2DFrom(Player.Default).magnitude)
									.Where(e=>e.Direction2DFrom(Player.Default).magnitude<=Global.SimpleSwordRange.Value * distanceTimes)
									.Take((Global.SimpleSwordCount.Value + Global.AdditionalFlyThingCount.Value) * countTimes))
								
				{
					
						Sword.Instantiate()
						.Position(enemy.Position()+Vector3.left*0.25f)
						.Show()
                        .Self(self =>
                        {
                            var selfCache=self;
							var hasHit = false;
							
                            selfCache.OnTriggerEnter2DEvent(collider2D =>
                            {
								if(hasHit) return;
								
								var hitHurtBox = collider2D.GetComponent<HitHurtBox>();

								if (hitHurtBox)
								{
									if (hitHurtBox.Owner.CompareTag("Enemy"))
									{
										var enemy = hitHurtBox.Owner.GetComponent<IEnemy>();
										if (enemy == null) return;
										hasHit = true;
										DamageSystem.CalculateDamage(Global.SimpleAbilityDamage.Value * damageTimes,
											enemy);
									}
								}
                            }).UnRegisterWhenGameObjectDestroyed(selfCache);

							//劈砍动画
							ActionKit
							.Sequence()
                            .Parallel(p =>
                            {
                                p.Lerp(0, 10, 0.2f, (z) =>
                                {
                                    selfCache.LocalEulerAnglesZ(z);
                                });

								p.Append(ActionKit.Sequence()
								.Lerp(0,1.25f,0.1f,scale=>selfCache.LocalScale(scale))
								.Lerp(1.25f,1f,0.1f,scale=>selfCache.LocalScale(scale))
								);
                            })
                            .Parallel(p =>
                            {
                                p.Lerp(10,-180,0.2f,z=>selfCache.LocalEulerAnglesZ(z));
								p.Append(ActionKit.Sequence()
								.Lerp(1,1.25f,0.1f,scale=>selfCache.LocalScale(scale))
								.Lerp(1.25f,1f,0.1f,scale=>selfCache.LocalScale(scale))
								);						
                            })
                            .Lerp(-180, 0, 0.3f, z =>
                            {
                                selfCache.LocalEulerAnglesZ(z)
								.LocalScale(z.Abs() / 180 );						 
                            })
							.Start(this,()=>{
								if(selfCache != null)
								{
									selfCache.DestroyGameObjGracefully();
								}
							});
                        });						
					
				}
			}
        }
    }
}
