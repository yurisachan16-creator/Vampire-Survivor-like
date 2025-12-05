using UnityEngine;
using QFramework;
using System.Linq;

namespace VampireSurvivorLike
{
	public partial class SimpleKnife : ViewController
	{
		void Start()
		{
			// Code Here
		}

		private float _mCurrentSeconds = 0;

        void Update()
        {
            _mCurrentSeconds += Time.deltaTime;

			//每秒发射一把刀
            if (_mCurrentSeconds >= 1.0f)
            {
                _mCurrentSeconds = 0;

				var enemies = FindObjectsByType<Enemy>(FindObjectsInactive.Exclude,FindObjectsSortMode.None);

				var enemy = enemies.OrderBy(enemy=>(Player.Default.transform.position-enemy.transform.position).magnitude).FirstOrDefault();
				if (enemy)
				{
					Knife.Instantiate()
					.Position(this.Position())
					.Show()
					.Self(self =>
					{
						var rigidbody2D = self.GetComponent<Rigidbody2D>();

						var direction = (enemy.Position() - Player.Default.Position()).normalized;
						rigidbody2D.velocity = direction * 10;

						self.OnTriggerEnter2DEvent(collider=>
						{
							
							var hurtBox=collider.GetComponent<HurtBox>();		

							if (hurtBox)
							{
								if(hurtBox.Owner.CompareTag("Enemy"))
								{
									hurtBox.Owner.GetComponent<Enemy>().Hurt(5);
									self.DestroyGameObjGracefully();
								}
							}

						}).UnRegisterWhenGameObjectDestroyed(self);

						ActionKit.OnUpdate.Register(() =>
						{
							if (Player.Default)
							{
								if(Player.Default.Position().y - self.Position().magnitude > 20)
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
