using UnityEngine;
using QFramework;
using System.Linq;
using System;

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
				var damageTimes=Global.SuperSword.Value ? UnityEngine.Random.Range(2,3+1) : 1;
				var distanceTimes=Global.SuperSword.Value ? 2 : 1;

				// 查找所有带 "Enemy" 标签的对象（包括 Boss）
				var enemyObjects = GameObject.FindGameObjectsWithTag("Enemy");
				var enemies = enemyObjects.Select(obj => new { Obj = obj, Enemy = obj.GetComponent<IEnemy>() })
					.Where(x => x.Enemy != null && Player.Default != null)
					.OrderBy(x => Vector3.Distance(x.Obj.transform.position, Player.Default.transform.position))
					.Where(x => Vector3.Distance(x.Obj.transform.position, Player.Default.transform.position) <= Global.SimpleSwordRange.Value * distanceTimes)
					.Take((Global.SimpleSwordCount.Value + Global.AdditionalFlyThingCount.Value) * countTimes);

				foreach(var enemyData in enemies)
								
				{
					
						Sword.Instantiate()
						.Position(enemyData.Obj.transform.position+Vector3.left*0.25f)
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
