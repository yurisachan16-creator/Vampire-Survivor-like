using UnityEngine;
using QFramework;
using System;
using UnityEditor.UI;

namespace VampireSurvivorLike
{
	public partial class Enemy : ViewController
	{
		public float MovementSpeed = 2f;

		public float Health = 3f;
		void Start()
		{
			EnemyGenerator.EnemyCount.Value++;
		}

        void Update()
        {
			

            if (Health <= 0)
            {
				//掉落经验值
				Global.GeneratePowerUp(gameObject);

				
                this.DestroyGameObjGracefully();
            }
			
        }

		void OnDestroy()
        {
            EnemyGenerator.EnemyCount.Value--;
        }

        void FixedUpdate()
        {
            if(Player.Default)
            {
                var direction=(Player.Default.transform.position-transform.position).normalized;

				SelfRigidbody2D.velocity = direction * MovementSpeed;
            }
            else
            {
                SelfRigidbody2D.velocity = Vector2.zero;
            }
        }

        private bool _isIgnoreHurt = false;

        internal void Hurt(float value,bool force=false)
        {
			if (_isIgnoreHurt&&!force) return;

            //显示伤害数字
            FloatingTextController.Play(transform.position + Vector3.up * 0.5f, value.ToString());

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
    }
}
