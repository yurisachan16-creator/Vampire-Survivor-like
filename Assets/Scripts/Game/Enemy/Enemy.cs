using UnityEngine;
using QFramework;
using System;
using QAssetBundle;

namespace VampireSurvivorLike
{
	public partial class Enemy : ViewController,IEnemy
	{
		[Header("基础属性（可被EnemyStatsConfig覆盖）")]
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
        
        [Header("配置来源")]
        [Tooltip("是否从EnemyStatsConfig读取属性，如果为false则使用预制体上的值")]
        public bool UseStatsConfig = true;

        private bool _isDead = false;
        
		void Start()
		{
			// 从配置中读取属性
			if (UseStatsConfig)
			{
				LoadStatsFromConfig();
			}
			
			EnemyGenerator.EnemyCount.Value++;
		}
		
		/// <summary>
		/// 从EnemyStatsConfig加载属性
		/// </summary>
		private void LoadStatsFromConfig()
		{
			var config = EnemyStatsConfig.Instance;
			if (config == null) return;
			
			var stats = config.GetStats(gameObject.name.Replace("(Clone)", "").Trim());
			if (stats == null) return;
			
			Health = stats.BaseHP;
			MovementSpeed = stats.BaseSpeed;
			DamageMultiplier = stats.BaseDamageMultiplier;
			ExpDropRate = stats.ExpDropRate;
			CoinDropRate = stats.CoinDropRate;
			HpDropRate = stats.HpDropRate;
			BombDropRate = stats.BombDropRate;
			DissolveColor = stats.DissolveColor;
		}

        void Update()
        {
			if (!_isDead && Health <= 0)
			{
				Die();
			}
			
        }

		private void Die()
		{
			if (_isDead) return;

			_isDead = true;
			_isIgnoreHurt = true;

			if (HitBox) HitBox.enabled = false;
			if (SelfRigidbody2D) SelfRigidbody2D.velocity = Vector2.zero;

			Global.GeneratePowerUpWithRates(gameObject, TreasureChestEnemy, ExpDropRate, CoinDropRate, HpDropRate, BombDropRate);
			AudioKit.PlaySound(Sfx.ENEMYDIE);
			FxController.Play(Sprite, DissolveColor);
			this.DestroyGameObjGracefully();
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
			if (_isDead) return;
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
				if (!this || _isDead) return;
				this.Health -= value;
				if (this.Health <= 0)
				{
					Die();
					return;
				}
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
