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

				var enemies = FindObjectsByType<Enemy>(FindObjectsInactive.Exclude,FindObjectsSortMode.None)
								.OrderBy(enemy=>Player.Default.Distance2D(enemy))
									.Take(Global.SimpleKnifeCount.Value);

				
				var i = 0;
				foreach(var enemy in enemies)
                {
					//计时器，游戏中最多同时有4个小刀的声音
                    if (i < 4)
                    {
						ActionKit.DelayFrame(11*i,()=>AudioKit.PlaySound(Sfx.KNIFE))
									.StartGlobal();
						i++;
                        
                    }		

                    if (enemy)
					{
						Knife.Instantiate()
						.Position(this.Position())
						.Show()
						.Self(self =>
						{
							
							var selfCache = self;
							var direction = enemy.NormalizedDirection2DFrom(Player.Default);
							self.transform.up = direction;

							var rigidbody2D = self.GetComponent<Rigidbody2D>();

							rigidbody2D.velocity = enemy.NormalizedDirection2DFrom(Player.Default) * 10;
							var attackCount = 0;

							self.OnTriggerEnter2DEvent(collider=>
							{
								
								var hurtBox=collider.GetComponent<HurtBox>();		

								if (hurtBox)
								{
									if(hurtBox.Owner.CompareTag("Enemy"))
									{
										//hurtBox.Owner.GetComponent<Enemy>().Hurt(Global.SimpleKnifeDamage.Value);
										DamageSystem.CalculateDamage(Global.SimpleKnifeDamage.Value,
												hurtBox.Owner.GetComponent<Enemy>());
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
