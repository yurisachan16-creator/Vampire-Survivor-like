using UnityEngine;
using QFramework;
using QAssetBundle;

namespace VampireSurvivorLike
{
	public partial class Ball : ViewController
	{
		
		void Start()
		{
			SelfRigidbody2D.velocity = 
				new Vector2(Random.Range(-1.0f,1.0f),Random.Range(-1.0f,1.0f))*
				Random.Range(Global.BasketBallSpeed.Value-2,Global.BasketBallSpeed.Value+2);

			//如果是超级武器，篮球就放大
			Global.SuperBasketBall.RegisterWithInitValue(unLocked=>
			{
				if(unLocked)
				{
					this.LocalScale(3);
				}
			}).UnRegisterWhenGameObjectDestroyed(gameObject);

			HurtBox.OnTriggerEnter2DEvent(collider=>
			{
				var hitHurtBox = collider.GetComponent<HitHurtBox>();
				if(hitHurtBox)
				{
					if(hitHurtBox.Owner.CompareTag("Enemy"))
					{
						var enemy=hitHurtBox.Owner.GetComponent<IEnemy>();
						var damageTimes=Global.SuperBasketBall.Value ? Random.Range(2,3+1) : 1;
						DamageSystem.CalculateDamage(Global.BasketBallDamage.Value * damageTimes,enemy);
						

						//有50%的概率对敌人进行击退
						if (Random.Range(0, 1.0f) < 0.5f&&collider&collider.attachedRigidbody&&Player.Default)
						{
							collider.attachedRigidbody.velocity =
								collider.NormalizedDirection2DFrom(this) * 5 +
								collider.NormalizedDirection2DFrom(Player.Default) * 10;
						}
					}
				}
			}).UnRegisterWhenGameObjectDestroyed(gameObject);
		}

		private void OnCollisionEnter2D(Collision2D other)
        {
            var normal = other.GetContact(0).normal;

            if (normal.x > normal.y)
            {
                SelfRigidbody2D.velocity = new Vector2(SelfRigidbody2D.velocity.x,
						Mathf.Sign(SelfRigidbody2D.velocity.y)*Random.Range(0.5f,1.5f)*
						Random.Range(Global.BasketBallSpeed.Value-2,Global.BasketBallSpeed.Value+2));
				SelfRigidbody2D.angularVelocity = Random.Range(-360,360);
			}
			else
			{
				var rb = SelfRigidbody2D;
				rb.velocity=
					new Vector2(Mathf.Sign(rb.velocity.x)*Random.Range(0.5f,1.5f)*Random.Range
							(Global.BasketBallSpeed.Value-2,Global.BasketBallSpeed.Value+2),
							rb.velocity.y);
				
				rb.angularVelocity = Random.Range(-360,360);
            }

			AudioKit.PlaySound(Sfx.BALL);
        }
	}
}
