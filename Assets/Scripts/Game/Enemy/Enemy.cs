using UnityEngine;
using QFramework;
using System;
using QAssetBundle;

namespace VampireSurvivorLike
{
	public partial class Enemy : ViewController, IEnemy, ObjectPoolSystem.IPoolable
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
        private bool _initialized = false;

        // 缓存 configKey，避免每次 string.Replace 分配
        internal string ConfigKey;

        // 受击计时器（替代 ActionKit.Delay，零 GC）
        private float _hurtTimer;
        private float _pendingDamage;
        private bool _hurtPending;

		internal bool IsDeadOrIgnoringHurt => _isDead || _isIgnoreHurt;

		private Animator _animator;
		private Transform _shadowTransform;

		// 缓存默认值用于池化重置
		private float _defaultHealth;
		private float _defaultMovementSpeed;
		private float _defaultDamageMultiplier;
		private float _defaultExpDropRate;
		private float _defaultCoinDropRate;
		private float _defaultHpDropRate;
		private float _defaultBombDropRate;
		private Color _defaultDissolveColor;

		private void Awake()
		{
			_animator = GetComponent<Animator>();
			_shadowTransform = transform.Find("Shadow");

			// 缓存预制体默认值
			_defaultHealth = Health;
			_defaultMovementSpeed = MovementSpeed;
			_defaultDamageMultiplier = DamageMultiplier;
			_defaultExpDropRate = ExpDropRate;
			_defaultCoinDropRate = CoinDropRate;
			_defaultHpDropRate = HpDropRate;
			_defaultBombDropRate = BombDropRate;
			_defaultDissolveColor = DissolveColor;
		}

		internal void ApplyLod(bool enableAnimation, bool enableShadow, bool enableSprite)
		{
			if (_animator) _animator.enabled = enableAnimation;
			if (_shadowTransform) _shadowTransform.gameObject.SetActive(enableShadow);
			if (Sprite) Sprite.enabled = enableSprite;
		}

		private void OnEnable()
		{
			EnemySimulationManager.Register(this);
		}

		private void OnDisable()
		{
			EnemySimulationManager.Unregister(this);
		}
        
		void Start()
		{
			if (!_initialized)
			{
				InitializeEnemy();
			}
		}

		/// <summary>
		/// 初始化敌人（首次 Start 或从池中取出时调用）
		/// </summary>
		internal void InitializeEnemy()
		{
			if (UseStatsConfig)
			{
				LoadStatsFromConfig();
			}

			EnemyGenerator.EnemyCount.Value++;
			EnemyGenerator.SmallEnemyCount.Value++;
			EnemyRegistry.Register(this);
			_initialized = true;
		}

		/// <summary>
		/// 从EnemyStatsConfig加载属性
		/// </summary>
		private void LoadStatsFromConfig()
		{
			var config = EnemyStatsConfig.Instance;
			if (config == null) return;

			// 优先使用外部传入的 ConfigKey，避免 string.Replace 分配
			var key = !string.IsNullOrEmpty(ConfigKey) ? ConfigKey : gameObject.name.Replace("(Clone)", "").Trim();
			var stats = config.GetStats(key);
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

			// 受击计时器：替代 ActionKit.Delay，零 GC
			if (_hurtPending)
			{
				_hurtTimer -= Time.deltaTime;
				if (_hurtTimer <= 0f)
				{
					_hurtPending = false;
					if (!_isDead)
					{
						Health -= _pendingDamage;
						if (Health <= 0)
						{
							Die();
							return;
						}
						if (Sprite) Sprite.color = Color.white;
						_isIgnoreHurt = false;
					}
				}
			}
        }

		private void Die()
		{
			if (_isDead) return;

			_isDead = true;
			_isIgnoreHurt = true;
			_hurtPending = false;

			if (HitBox) HitBox.enabled = false;
			if (SelfRigidbody2D) SelfRigidbody2D.velocity = Vector2.zero;

			Global.GeneratePowerUpWithRates(gameObject, TreasureChestEnemy, ExpDropRate, CoinDropRate, HpDropRate, BombDropRate);
			AudioKit.PlaySound(Sfx.ENEMYDIE);
			FxController.Play(Sprite, DissolveColor);
			ObjectPoolSystem.Despawn(gameObject);
		}

		/// <summary>
		/// 从池中取出时的回调
		/// </summary>
		public void OnSpawned()
		{
			// 重置状态
			_isDead = false;
			_isIgnoreHurt = false;
			_hurtPending = false;
			_hurtTimer = 0f;
			_initialized = false;
			ConfigKey = null;

			// 恢复默认属性值
			Health = _defaultHealth;
			MovementSpeed = _defaultMovementSpeed;
			DamageMultiplier = _defaultDamageMultiplier;
			ExpDropRate = _defaultExpDropRate;
			CoinDropRate = _defaultCoinDropRate;
			HpDropRate = _defaultHpDropRate;
			BombDropRate = _defaultBombDropRate;
			DissolveColor = _defaultDissolveColor;
			TreasureChestEnemy = false;

			// 恢复视觉状态
			if (Sprite) Sprite.color = Color.white;
			if (HitBox) HitBox.enabled = true;
			if (SelfRigidbody2D)
			{
				SelfRigidbody2D.velocity = Vector2.zero;
				SelfRigidbody2D.simulated = true;
			}
			if (_animator) _animator.enabled = true;
			if (_shadowTransform) _shadowTransform.gameObject.SetActive(true);
		}

		/// <summary>
		/// 回收进池时的回调
		/// </summary>
		public void OnDespawned()
		{
			// 反注册
			if (_initialized)
			{
				EnemyGenerator.EnemyCount.Value--;
				EnemyGenerator.SmallEnemyCount.Value--;
				EnemyRegistry.Unregister(this);
				_initialized = false;
			}
		}

		void OnDestroy()
        {
			// 真正销毁时也确保反注册（应对场景卸载等情况）
			if (_initialized)
			{
				EnemyGenerator.EnemyCount.Value--;
				EnemyGenerator.SmallEnemyCount.Value--;
				EnemyRegistry.Unregister(this);
				_initialized = false;
			}
        }

        void FixedUpdate()
        {
			if (EnemySimulationManager.Enabled) return;

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
			//使用计时器替代 ActionKit.Delay，零 GC 分配
			_pendingDamage = value;
			_hurtTimer = 0.2f;
			_hurtPending = true;
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
