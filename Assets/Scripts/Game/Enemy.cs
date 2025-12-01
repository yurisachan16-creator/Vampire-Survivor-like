using UnityEngine;
using QFramework;
using System;

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
			if(Player.Default)
            {
                var direction=(Player.Default.transform.position-transform.position).normalized;


            	transform.Translate(direction*Time.deltaTime*MovementSpeed);
            }

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

		private bool _isIgnoreHurt = false;

        internal void Hurt(float value)
        {
			if (_isIgnoreHurt) return;

            Sprite.color = Color.red;		
			//延时0.3秒后判断攻击，恢复颜色并扣血
			ActionKit.Delay(0.2f,() =>
			{
				this.Health -= Global.SimpleAbilityDamage.Value;
				this.Sprite.color = Color.white;
				_isIgnoreHurt = false;
			}).Start(this);
        }
    }
}
