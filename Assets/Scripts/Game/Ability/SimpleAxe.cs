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
                Axe.Instantiate()
				.Show()
				.Position(this.Position())
				.Self(self =>
                {
                    var rigidbody2D = self.GetComponent<Rigidbody2D>();

					var randomX = RandomUtility.Choose(-8,-5,-3,3,5,8);
					var randomY = RandomUtility.Choose(3,5,8);
					rigidbody2D.velocity = new Vector2(randomX, randomY);

                    self.OnTriggerEnter2DEvent(collider=>
                    {
						var hurtBox=collider.GetComponent<HurtBox>();		

                        if (hurtBox)
                		{
							if(hurtBox.Owner.CompareTag("Enemy"))
							{
								hurtBox.Owner.GetComponent<Enemy>().Hurt(2);
							}
                		}

                    }).UnRegisterWhenGameObjectDestroyed(gameObject);

					//定时检测是否超出屏幕上方，超出则销毁
                    ActionKit.OnUpdate.Register(() =>
                    {
                        if (Player.Default)
                        {
                            if(Player.Default.Position().y - self.Position().y > 15)
							{
								self.DestroyGameObjGracefully();
							}
                        }
                        
                    }).UnRegisterWhenGameObjectDestroyed(self);
                });

				_mCurrentSecond=0;
            }
        }
    }
}
