using UnityEngine;
using QFramework;
using System;
using UnityEditor.UI;
using QAssetBundle;

namespace VampireSurvivorLike
{
	public partial class Enemy : ViewController,IEnemy
	{
		public float MovementSpeed = 2f;

		public float Health = 3f;
        public Color DissolveColor = Color.yellow;
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
                AudioKit.PlaySound(Sfx.ENEMYDIE);
				FxController.Play(Sprite, DissolveColor);
                this.DestroyGameObjGracefully();
            }
			
        }

		void OnDestroy()
        {
            EnemyGenerator.EnemyCount.Value--;
        }

        void FixedUpdate()
        {
            if (!_isIgnoreHurt)
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
            
        }

        private bool _isIgnoreHurt = false;

        internal void Hurt(float value,bool force=false)
        {
			if (_isIgnoreHurt&&!force) return;

            //受伤时停止移动
            _isIgnoreHurt = true;
            SelfRigidbody2D.velocity = Vector2.zero;

            //显示伤害数字
            FloatingTextController.Play(transform.position + Vector3.up * 0.5f, value.ToString("0"));

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

        void IEnemy.Hurt(float value, bool force)
        {
            Hurt(value, force);
        }

        public void SetSpeedScale(float SpeedScale)
        {
            MovementSpeed *= SpeedScale;
        }

        public void SetHPScale(float HPScale)
        {
            Health *= HPScale;
        }
    }
}
