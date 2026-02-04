using UnityEngine;
using QFramework;
using System.Linq;
using QAssetBundle;

namespace VampireSurvivorLike
{
	public partial class SimpleKnife : ViewController
	{
		

		private float _mCurrentSeconds = 0;

		

        void Update()
        {
            _mCurrentSeconds += Time.deltaTime;

			//每隔一段时间发射一把飞刀
            if (_mCurrentSeconds >= Global.SimpleKnifeDuration.Value)
            {
                _mCurrentSeconds = 0;

				if (!Player.Default) return;

				var targetCount = Global.SimpleKnifeCount.Value + Global.AdditionalFlyThingCount.Value;
				var targets = FindObjectsByType<Enemy>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
					.Cast<Component>()
					.Concat(FindObjectsByType<EnemyMiniBoss>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
					.OrderBy(target => Vector2.Distance(target.transform.position, Player.Default.transform.position))
					.Take(targetCount);

				
				var i = 0;
				foreach(var target in targets)
                {
					//计时器，游戏中最多同时有4个小刀的声音
                    if (i < 4)
                    {
						ActionKit.DelayFrame(11*i,()=>AudioKit.PlaySound(Sfx.KNIFE))
									.StartGlobal();
						i++;
                        
                    }		

                    if (target)
					{
						Knife.Instantiate()
						.Position(this.Position())
						.Show()
						.Self(self =>
						{
							
							var selfCache = self;
							var direction = ((Vector2)target.transform.position - (Vector2)Player.Default.transform.position).normalized;
							self.transform.up = direction;

							var rigidbody2D = self.GetComponent<Rigidbody2D>();

							rigidbody2D.velocity = direction * 10;
							var attackCount = 0;

							self.OnTriggerEnter2DEvent(collider=>
							{
								
								var hitHurtBox = collider.GetComponent<HitHurtBox>();		

								if (hitHurtBox)
								{
									if(hitHurtBox.Owner.CompareTag("Enemy"))
									{
										var enemy = hitHurtBox.Owner.GetComponent<IEnemy>();
										if (enemy == null) return;
										var damageTimes=Global.SuperKnife.Value ? Random.Range(2,3+1) : 1;
										DamageSystem.CalculateDamage(Global.SimpleKnifeDamage.Value * damageTimes,
											enemy);
										attackCount++;

										if(attackCount>=Global.SimpleKnifeAttackCount.Value)
                                        {
                                            selfCache.DestroyGameObjGracefully();
                                        }
										
									}
								}

							}).UnRegisterWhenGameObjectDestroyed(self);

							ActionKit.OnUpdate.Register(() =>
							{
								if (Player.Default)
								{
									if(Player.Default.Distance2D(selfCache) > 20)
									{
										self.DestroyGameObjGracefully();
									}
								}
							}).UnRegisterWhenGameObjectDestroyed(self);

							});
					}
                } 
            }
			
        }
    }
}
