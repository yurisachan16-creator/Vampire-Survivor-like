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
        private float _slowMultiplier = 1f;
        private float _slowUntilTime;

        // 缓存 configKey，避免每次 string.Replace 分配
        internal string ConfigKey;

        // 受击计时器（替代 ActionKit.Delay，零 GC）
		private float _hurtTimer;
		private float _pendingDamage;
		private bool _hurtPending;
		private bool _knockbackActive;
		private Vector2 _knockbackVelocity;
		private float _knockbackRemainSeconds;
		private float _knockbackCooldownUntil;

		private const float ExternalKnockbackCooldownSeconds = 0.12f;
		private const float ExternalKnockbackMinDuration = 0.05f;
		private const float ExternalKnockbackMaxDuration = 0.35f;

		internal bool IsDeadOrIgnoringHurt => _isDead || _isIgnoreHurt;
		internal bool IsDead => _isDead;
		internal bool IsIgnoringHurt => _isIgnoreHurt;
		internal bool HasExternalKnockback => _knockbackActive;
		internal Vector2 ExternalKnockbackVelocity => _knockbackVelocity;

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

		// 生成器在 Start 前注入的缩放/覆盖值缓存
		private bool _hasPendingBaseSpeed;
		private float _pendingBaseSpeed;
		private float _pendingSpeedScale = 1f;
		private float _pendingHPScale = 1f;
		private float _pendingDamageScale = 1f;
		private bool _hasPendingDropRates;
		private float _pendingExpDropRate;
		private float _pendingCoinDropRate;
		private float _pendingHpDropRate;
		private float _pendingBombDropRate;
		private bool _hasPendingTreasureChest;
		private bool _pendingTreasureChest;

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
			CombatLayerSettings.ApplyEnemyBodyLayer(gameObject);
			ConfigureRuntimePhysics();
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
			if (_initialized) return;

			if (UseStatsConfig)
			{
				LoadStatsFromConfig();
			}

			ApplyPendingSpawnOverrides();

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

		private void ApplyPendingSpawnOverrides()
		{
			if (_hasPendingBaseSpeed)
			{
				MovementSpeed = _pendingBaseSpeed;
			}

			MovementSpeed *= _pendingSpeedScale;
			Health *= _pendingHPScale;
			DamageMultiplier *= _pendingDamageScale;

			if (_hasPendingDropRates)
			{
				ExpDropRate = _pendingExpDropRate;
				CoinDropRate = _pendingCoinDropRate;
				HpDropRate = _pendingHpDropRate;
				BombDropRate = _pendingBombDropRate;
			}

			if (_hasPendingTreasureChest)
			{
				TreasureChestEnemy = _pendingTreasureChest;
			}

			ResetPendingSpawnOverrides();
		}

		private void ResetPendingSpawnOverrides()
		{
			_hasPendingBaseSpeed = false;
			_pendingBaseSpeed = 0f;
			_pendingSpeedScale = 1f;
			_pendingHPScale = 1f;
			_pendingDamageScale = 1f;
			_hasPendingDropRates = false;
			_pendingExpDropRate = 0f;
			_pendingCoinDropRate = 0f;
			_pendingHpDropRate = 0f;
			_pendingBombDropRate = 0f;
			_hasPendingTreasureChest = false;
			_pendingTreasureChest = false;
		}

        void Update()
        {
			UpdateExternalKnockback();

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
			Global.RunKillCount++;
			_isIgnoreHurt = true;
			_hurtPending = false;
			ResetExternalKnockback(true);

			if (HitBox) HitBox.enabled = false;
			if (SelfRigidbody2D) SelfRigidbody2D.velocity = Vector2.zero;

			Global.GeneratePowerUpWithRates(gameObject, TreasureChestEnemy, ExpDropRate, CoinDropRate, HpDropRate, BombDropRate);
			if (SfxThrottle.CanPlay(Sfx.ENEMYDIE))
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
			ResetPendingSpawnOverrides();
			_slowMultiplier = 1f;
			_slowUntilTime = 0f;
			ResetExternalKnockback(true);
			CombatLayerSettings.ApplyEnemyBodyLayer(gameObject);
			ConfigureRuntimePhysics();

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
				EnemyGenerator.EnemyCount.Value = Mathf.Max(0, EnemyGenerator.EnemyCount.Value - 1);
				EnemyGenerator.SmallEnemyCount.Value = Mathf.Max(0, EnemyGenerator.SmallEnemyCount.Value - 1);
				EnemyRegistry.Unregister(this);
				_initialized = false;
			}
		}

		void OnDestroy()
        {
			// 真正销毁时也确保反注册（应对场景卸载等情况）
			if (_initialized)
			{
				EnemyGenerator.EnemyCount.Value = Mathf.Max(0, EnemyGenerator.EnemyCount.Value - 1);
				EnemyGenerator.SmallEnemyCount.Value = Mathf.Max(0, EnemyGenerator.SmallEnemyCount.Value - 1);
				EnemyRegistry.Unregister(this);
				_initialized = false;
			}
        }

        void FixedUpdate()
        {
			if (EnemySimulationManager.Enabled) return;

			if (_knockbackActive)
			{
				MoveByVelocity(_knockbackVelocity, Time.fixedDeltaTime);
				return;
			}

            if (!_isIgnoreHurt)
            {
                if(Player.Default)
                {
                    var direction=(Player.Default.transform.position-transform.position).normalized;
                    MoveByVelocity(direction * GetEffectiveMovementSpeed(), Time.fixedDeltaTime);
                }
                else
                {
                    MoveByVelocity(Vector2.zero, Time.fixedDeltaTime);
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
            if (SelfRigidbody2D) SelfRigidbody2D.velocity = Vector2.zero;

            //显示伤害数字
            FloatingTextController.PlayDamage(transform.position + Vector3.up * 0.5f, value, critical);

            Sprite.color = Color.red;
			if (SfxThrottle.CanPlay("Hit"))
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
			if (!_initialized)
			{
				_pendingSpeedScale *= SpeedScale;
				return;
			}

            MovementSpeed *= SpeedScale;
        }

        public void SetHPScale(float HPScale)
        {
			if (!_initialized)
			{
				_pendingHPScale *= HPScale;
				return;
			}

            Health *= HPScale;
        }

        public void SetDamageScale(float DamageScale)
        {
			if (!_initialized)
			{
				_pendingDamageScale *= DamageScale;
				return;
			}

            DamageMultiplier *= DamageScale;
        }

        public void SetBaseSpeed(float baseSpeed)
        {
			if (!_initialized)
			{
				_hasPendingBaseSpeed = true;
				_pendingBaseSpeed = baseSpeed;
				return;
			}

            MovementSpeed = baseSpeed;
        }

        public void SetDropRates(float expRate, float coinRate, float hpRate, float bombRate)
        {
			if (!_initialized)
			{
				_hasPendingDropRates = true;
				_pendingExpDropRate = expRate;
				_pendingCoinDropRate = coinRate;
				_pendingHpDropRate = hpRate;
				_pendingBombDropRate = bombRate;
				return;
			}

            ExpDropRate = expRate;
            CoinDropRate = coinRate;
            HpDropRate = hpRate;
            BombDropRate = bombRate;
        }

        public void SetTreasureChest(bool isTreasureChest)
        {
			if (!_initialized)
			{
				_hasPendingTreasureChest = true;
				_pendingTreasureChest = isTreasureChest;
				return;
			}

            TreasureChestEnemy = isTreasureChest;
        }

		public void ApplySlow(float multiplier, float durationSeconds)
		{
			multiplier = Mathf.Clamp(multiplier, 0.2f, 1f);
			durationSeconds = Mathf.Max(0f, durationSeconds);
			if (durationSeconds <= 0f) return;

			if (Time.time < _slowUntilTime && multiplier >= _slowMultiplier)
			{
				return;
			}

			_slowMultiplier = multiplier;
			_slowUntilTime = Time.time + durationSeconds;
		}

		public void ApplyExternalKnockback(Vector2 direction, float speed = 5.5f, float durationSeconds = 0.14f)
		{
			if (_isDead) return;
			if (Time.time < _knockbackCooldownUntil) return;
			if (direction.sqrMagnitude <= 0.0001f) return;

			direction.Normalize();
			_knockbackVelocity = direction * Mathf.Max(0f, speed);
			_knockbackRemainSeconds = Mathf.Clamp(durationSeconds, ExternalKnockbackMinDuration, ExternalKnockbackMaxDuration);
			_knockbackActive = true;
			_knockbackCooldownUntil = Time.time + ExternalKnockbackCooldownSeconds;
			_isIgnoreHurt = true;

			if (SelfRigidbody2D)
			{
				SelfRigidbody2D.velocity = Vector2.zero;
			}
		}

		internal float GetEffectiveMovementSpeed()
		{
			if (Time.time >= _slowUntilTime)
			{
				_slowMultiplier = 1f;
			}

			return MovementSpeed * _slowMultiplier;
		}

		private void ConfigureRuntimePhysics()
		{
			if (!SelfRigidbody2D) return;

			SelfRigidbody2D.bodyType = RigidbodyType2D.Kinematic;
			SelfRigidbody2D.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
			SelfRigidbody2D.interpolation = RigidbodyInterpolation2D.None;
		}

		private void UpdateExternalKnockback()
		{
			if (!_knockbackActive) return;

			_knockbackRemainSeconds -= Time.deltaTime;
			if (_knockbackRemainSeconds > 0f)
			{
				return;
			}

			ResetExternalKnockback(false);
			if (!_hurtPending)
			{
				_isIgnoreHurt = false;
			}
		}

		private void ResetExternalKnockback(bool clearCooldown)
		{
			_knockbackActive = false;
			_knockbackVelocity = Vector2.zero;
			_knockbackRemainSeconds = 0f;
			if (clearCooldown)
			{
				_knockbackCooldownUntil = 0f;
			}
		}

		internal void MoveByVelocity(Vector2 velocity, float stepSeconds)
		{
			if (!SelfRigidbody2D) return;

			if (stepSeconds <= 0f)
			{
				stepSeconds = Time.fixedDeltaTime;
			}

			var nextPosition = SelfRigidbody2D.position + velocity * stepSeconds;
			SelfRigidbody2D.MovePosition(nextPosition);
		}
    }
}
