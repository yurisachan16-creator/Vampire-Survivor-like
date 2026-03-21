using UnityEngine;
using QFramework;
using QAssetBundle;
using System.Collections.Generic;

namespace VampireSurvivorLike
{
	public partial class Ball : ViewController
	{
		private const int SameTargetHitCooldownFrames = 6;
		private readonly Dictionary<int, int> _lastHitFrameByEnemy = new Dictionary<int, int>(64);
		
		void Start()
		{
			CombatLayerSettings.ApplyPlayerAttackLayer(gameObject);
			SelfRigidbody2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

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
				if (!collider.TryGetComponent<HitHurtBox>(out var hitHurtBox)) return;
				if (!hitHurtBox.IsEnemyOwner) return;
				if (!hitHurtBox.TryGetEnemy(out var enemy)) return;
				if (!hitHurtBox.Owner) return;

				var enemyId = hitHurtBox.Owner.GetInstanceID();
				if (_lastHitFrameByEnemy.TryGetValue(enemyId, out var lastFrame) &&
				    Time.frameCount - lastFrame < SameTargetHitCooldownFrames)
				{
					return;
				}

				_lastHitFrameByEnemy[enemyId] = Time.frameCount;

				var damageTimes = Global.SuperBasketBall.Value ? Random.Range(2, 4) : 1;
				DamageSystem.CalculateDamage(Global.BasketBallDamage.Value * damageTimes, enemy);

				//有50%的概率对敌人进行击退
				if (Random.Range(0, 1.0f) < 0.5f && Player.Default)
				{
					var knockbackDirection = collider.NormalizedDirection2DFrom(this);
					var playerDirection = collider.NormalizedDirection2DFrom(Player.Default);
					var combinedDirection = (knockbackDirection + playerDirection).normalized;
					if (combinedDirection.sqrMagnitude <= 0.0001f)
					{
						combinedDirection = knockbackDirection.sqrMagnitude > 0.0001f
							? knockbackDirection
							: playerDirection;
					}

					enemy.ApplyExternalKnockback(combinedDirection, 6f, 0.14f);
				}
			}).UnRegisterWhenGameObjectDestroyed(gameObject);
		}

		private void FixedUpdate()
		{
			if (!SelfRigidbody2D) return;
			if (!CameraController.LBTransform || !CameraController.RTTransform) return;

			var radius = HurtBox
				? HurtBox.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y)
				: 0.5f;

			var lb = CameraController.LBTransform.position;
			var rt = CameraController.RTTransform.position;
			var minX = lb.x + radius;
			var maxX = rt.x - radius;
			var minY = lb.y + radius;
			var maxY = rt.y - radius;

			var pos = SelfRigidbody2D.position;
			var vel = SelfRigidbody2D.velocity;
			var corrected = false;

			if (pos.x < minX)
			{
				pos.x = minX;
				vel.x = Mathf.Abs(vel.x);
				corrected = true;
			}
			else if (pos.x > maxX)
			{
				pos.x = maxX;
				vel.x = -Mathf.Abs(vel.x);
				corrected = true;
			}

			if (pos.y < minY)
			{
				pos.y = minY;
				vel.y = Mathf.Abs(vel.y);
				corrected = true;
			}
			else if (pos.y > maxY)
			{
				pos.y = maxY;
				vel.y = -Mathf.Abs(vel.y);
				corrected = true;
			}

			if (corrected)
			{
				SelfRigidbody2D.position = pos;
				SelfRigidbody2D.velocity = vel;
			}
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

			if (SfxThrottle.CanPlay(Sfx.BALL))
				AudioKit.PlaySound(Sfx.BALL);
        }
	}
}
