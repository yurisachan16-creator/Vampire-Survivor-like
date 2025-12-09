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

				var enemies = FindObjectsByType<Enemy>(FindObjectsInactive.Exclude,FindObjectsSortMode.None);

				foreach(var enemy in enemies
									.OrderBy(e=>e.Direction2DFrom(Player.Default).magnitude)
									.Where(e=>e.Direction2DFrom(Player.Default).magnitude<=Global.SimpleSwordRange.Value)
									.Take(Global.SimpleSwordCount.Value))
								
				{
					
						Sword.Instantiate()
						.Position(enemy.Position()+Vector3.left*0.25f)
						.Show()
                        .Self(self =>
                        {
                            var selfCache=self;
                            selfCache.OnTriggerEnter2DEvent(collider2D =>
                            {
                                var hurtBox = collider2D.GetComponent<HurtBox>();

								if (hurtBox)
								{
									if (hurtBox.Owner.CompareTag("Enemy"))
									{
										hurtBox.Owner.GetComponent<Enemy>().Hurt(Global.SimpleAbilityDamage.Value);
									}
								}
                            }).UnRegisterWhenGameObjectDestroyed(gameObject);

							//劈砍动画
							ActionKit
							.Sequence()
							.Callback(()=>{selfCache.enabled=false;})
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
							.Callback(()=>{selfCache.enabled=true;})
                            .Parallel(p =>
                            {
                                p.Lerp(10,-180,0.2f,z=>selfCache.LocalEulerAnglesZ(z));
								p.Append(ActionKit.Sequence()
								.Lerp(1,1.25f,0.1f,scale=>selfCache.LocalScale(scale))
								.Lerp(1.25f,1f,0.1f,scale=>selfCache.LocalScale(scale))
								);						
                            })
							.Callback(()=>{selfCache.enabled=false;})
                            .Lerp(-180, 0, 0.3f, z =>
                            {
                                selfCache.LocalEulerAnglesZ(z)
								.LocalScale(z.Abs() / 180 );						 
                            })
							.Start(this,()=>{selfCache.DestroyGameObjGracefully();});
                        });						
					
				}
			}
        }
    }
}
