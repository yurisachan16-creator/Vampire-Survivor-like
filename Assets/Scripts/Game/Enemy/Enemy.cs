using UnityEngine;
using QFramework;
using System;
using QAssetBundle;

namespace VampireSurvivorLike
{
	public partial class Enemy : ViewController,IEnemy
	{
		public float MovementSpeed = 2f;

		public float Health = 3f;
		public float DamageMultiplier = 1f;  //伤害倍率
        public Color DissolveColor = Color.yellow;
        public bool TreasureChestEnemy = false;
        
        // 掉落概率配置
        public float ExpDropRate = 0.3f;
        public float CoinDropRate = 0.3f;
        public float HpDropRate = 0.1f;
        public float BombDropRate = 0.05f;
        
		void Start()
		{
			EnemyGenerator.EnemyCount.Value++;
		}

        void Update()
        {
			

            if (Health <= 0)
            {
				//掉落道具（使用配置的掉落率）
				Global.GeneratePowerUpWithRates(gameObject, TreasureChestEnemy, ExpDropRate, CoinDropRate, HpDropRate, BombDropRate);
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

        internal void Hurt(float value,bool force=false, bool critical=false)
        {
			if (_isIgnoreHurt&&!force) return;

            //受伤时停止移动
            _isIgnoreHurt = true;
            SelfRigidbody2D.velocity = Vector2.zero;

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

        void IEnemy.Hurt(float value, bool force, bool critical)
        {
            Hurt(value, force, critical);
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

        public void SetBaseSpeed(float baseSpeed)
        {
            MovementSpeed = baseSpeed;
        }

        public void SetDropRates(float expRate, float coinRate, float hpRate, float bombRate)
        {
            ExpDropRate = expRate;
            CoinDropRate = coinRate;
            HpDropRate = hpRate;
            BombDropRate = bombRate;
        }

        public void SetTreasureChest(bool isTreasureChest)
        {
            TreasureChestEnemy = isTreasureChest;
        }
    }
}
