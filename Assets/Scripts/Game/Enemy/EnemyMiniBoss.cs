using UnityEngine;
using QFramework;

namespace VampireSurvivorLike
{
	public partial class EnemyMiniBoss : ViewController,IEnemy
	{
		public enum States
        {
            FlowingPlayer,
			Warning,
			Dash,
            Wait
        }

		public FSM<States> FSM = new FSM<States>();
		public float Health = 20f;
		public float MovementSpeed = 1.5f;
		public float DamageMultiplier = 1f;  //伤害倍率
        
        void Start()
		{
			EnemyGenerator.EnemyCount.Value++;

			FSM.State(States.FlowingPlayer)
            .OnFixedUpdate(() =>
            {
                if(Player.Default)
				{
					var direction=(Player.Default.transform.position-transform.position).normalized;

					SelfRigidbody2D.velocity = direction * MovementSpeed;
					//距离玩家15单位内则进入警戒状态
                    if ((Player.Default.transform.Position() - transform.Position()).magnitude <= 15)
                    {
                        FSM.ChangeState(States.Warning);
                    }
				}
				else
				{
					SelfRigidbody2D.velocity = Vector2.zero;
				}
            });

			FSM.State(States.Warning)
            .OnEnter(() =>
            {
                SelfRigidbody2D.velocity = Vector2.zero;
            })
            .OnUpdate(() =>
            {
                //冲刺预警
                var frames =  3 + (60*3-FSM.FrameCountOfCurrentState)/10;
                if(FSM.FrameCountOfCurrentState / frames % 2 ==0)
                {
                    Sprite.color = Color.red;
                }
                else
                {
                    Sprite.color = Color.white;
                }
				//警戒2秒后冲刺
                if (FSM.FrameCountOfCurrentState >= 60 * 3)
                {
                    FSM.ChangeState(States.Dash);
                }
            })
            .OnExit(() =>
            {
                Sprite.color = Color.white;
            });

			var dashStartPos = Vector3.zero;
			var dashStartDistanceToPlayer = 0f;
			FSM.State(States.Dash)
            .OnEnter(() =>
            {
                var direction=(Player.Default.transform.position-transform.position).normalized;
				SelfRigidbody2D.velocity = direction * MovementSpeed * 10;
				dashStartPos = transform.Position();
				dashStartDistanceToPlayer = (Player.Default.transform.Position() - dashStartPos).magnitude;
				
            })
            .OnUpdate(() =>
            {
				var distance = (transform.Position() - dashStartPos).magnitude;
				//冲刺距离超过一定值则返回追踪玩家状态
                if (distance >= dashStartDistanceToPlayer +5)
                {
					FSM.ChangeState(States.Wait);
                }
                
            });
			

            FSM.State(States.Wait)
            .OnEnter(() =>
            {
                SelfRigidbody2D.velocity = Vector2.zero;
            })
            .OnUpdate(() =>
            {
                if(FSM.FrameCountOfCurrentState >= 30)
                {
                    FSM.ChangeState(States.FlowingPlayer);
                }
            });

            FSM.StartState(States.FlowingPlayer);
		}

		void Update()
        {
			FSM.Update();

            if (Health <= 0)
            {
				//生成掉落物
				Global.GeneratePowerUp(gameObject, true);

				
                this.DestroyGameObjGracefully();
            }
			
        }

		void FixedUpdate()
        {
            FSM.FixedUpdate();
        }

		void OnDestroy()
        {
            EnemyGenerator.EnemyCount.Value--;
        }

		private bool _isIgnoreHurt = false;
		public void Hurt(float value, bool force = false,bool critical=false)
        {
            if (_isIgnoreHurt&&!force) return;

            //显示伤害数字
            FloatingTextController.Play(transform.position + Vector3.up * 0.5f, value.ToString("0"),critical);

            Sprite.color = Color.red;
			AudioKit.PlaySound("Hit");		
			//延时0.3秒后判断攻击，恢复颜色并扣血
			ActionKit.Delay(0.2f,() =>
			{
				this.Health -= value;
				this.Sprite.color = Color.white;
				_isIgnoreHurt = false;
			}).Start(this);
        }

        public void SetSpeedScale(float SpeedScale)
        {
            MovementSpeed *= SpeedScale;
        }

        public void SetHPScale(float HPScale)
        {
            Health *= HPScale;
        }

        public void SetDamageScale(float DamageScale)
        {
            DamageMultiplier *= DamageScale;
        }
    }
}
