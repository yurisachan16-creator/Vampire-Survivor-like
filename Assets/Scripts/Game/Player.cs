using UnityEngine;
using QFramework;
using QAssetBundle;

namespace VampireSurvivorLike
{
	public partial class Player : ViewController
	{
		public float MoveSpeed = 5f;
		[Tooltip("受击无敌时间（秒）：用于防止同帧/短时间内重复扣血")]
		public float InvincibleSeconds = 1f;
		private const float InvincibleBlinkSpeed = 14f;
		private const float InvincibleMinAlpha = 0.45f;
		private AudioPlayer _mWalkSfx;
		private float _invincibleUntilTime;
		private int _lastDamageFrame = -1;
		private string _lastDamageSource;
		private SpriteRenderer[] _playerRenderers = System.Array.Empty<SpriteRenderer>();
		private Color[] _rendererBaseColors = System.Array.Empty<Color>();
		private bool _invincibleVisualActive;

		public static Player Default { get; private set; }
		public bool IsGameOver => Global.IsGameOver.Value;

		#region 生命周期函数

		void Awake()
        {
            Default = this;
			CachePlayerRenderers();
        }

		void Start()
		{	
			if (!GetComponent<AttackRangeVisualizer>())
			{
				gameObject.AddComponent<AttackRangeVisualizer>();
			}

            HurtBox.OnTriggerEnter2DEvent(Collider2D =>
			{
				if (IsGameOver) return;

				if (!Collider2D.TryGetComponent<HitHurtBox>(out var hitHurtBox)) return;
				if (!hitHurtBox.IsEnemyOwner) return;

				var boss = hitHurtBox.CachedMiniBoss;
				var contactDamage = 1;
				if (boss)
				{
					contactDamage = Mathf.Max(1, Mathf.CeilToInt(boss.DamageMultiplier));
				}
				else if (hitHurtBox.TryGetEnemy(out var enemy) && enemy is Enemy normalEnemy)
				{
					contactDamage = Mathf.Max(1, Mathf.CeilToInt(normalEnemy.DamageMultiplier));
				}

				var bossId = boss ? boss.BossType.ToString() : string.Empty;
				ApplyDamage(contactDamage, bossId, boss ? "BossMelee" : "EnemyMelee");
			}).UnRegisterWhenGameObjectDestroyed(gameObject);

			void UPdateHP()
            {
                HPValue.fillAmount = Global.HP.Value / (float)Global.MaxHP.Value;
            }

			Global.RequestHPUIRefresh.Register(() =>
			{
				UPdateHP();
			}).UnRegisterWhenGameObjectDestroyed(gameObject);

			UPdateHP();
		}

		public bool ApplyDamage(int amount, string bossId, string damageSource, bool ignoreInvincible = false)
		{
			if (IsGameOver) return false;

			amount = Mathf.Max(1, amount - Mathf.Max(0, Global.ArmorValue.Value));
			damageSource ??= string.Empty;
			amount = ApplyBossDamageCap(amount, damageSource);

			if (_lastDamageFrame == Time.frameCount && _lastDamageSource == damageSource) return false;
			if (!ignoreInvincible && _lastDamageFrame != Time.frameCount && Time.time < _invincibleUntilTime)
			{
				return false;
			}

			_lastDamageFrame = Time.frameCount;
			_lastDamageSource = damageSource;
			_invincibleUntilTime = Time.time + Mathf.Max(0f, InvincibleSeconds);

			Global.HP.Value -= amount;

			if (Global.HP.Value <= 0)
			{
				GameOver(bossId, damageSource);
				return true;
			}

			AudioKit.PlaySound(Sfx.HURT);
			CameraController.ShakeCamera();
			_invincibleVisualActive = true;
			Global.RequestHPUIRefresh.Trigger();
			return true;
		}

		private static int ApplyBossDamageCap(int amount, string damageSource)
		{
			if (Global.MaxHP.Value <= 0) return amount;
			if (!IsBossDamageSource(damageSource)) return amount;

			var cap = Mathf.Max(1, Mathf.FloorToInt(Global.MaxHP.Value * Config.BossSingleHitDamageCapRatio));
			return Mathf.Min(amount, cap);
		}

		private static bool IsBossDamageSource(string damageSource)
		{
			if (string.IsNullOrEmpty(damageSource)) return false;
			return damageSource.StartsWith("Boss", System.StringComparison.Ordinal);
		}

		private void GameOver(string bossId, string damageSource)
		{
			if (IsGameOver) return;

			Global.IsGameOver.Value = true;
			Global.ReportPlayerDeath(bossId, damageSource);
			var deathReason = string.IsNullOrEmpty(bossId) ? damageSource : $"{bossId}:{damageSource}";
			LeaderboardSystem.RecordCurrentRun(false, LeaderboardSystem.BuildDeathReason(false, deathReason));

			AudioKit.PlaySound(Sfx.DEATH);
			Time.timeScale = 0;

			if(_mWalkSfx != null)
			{
				_mWalkSfx.Stop();
				_mWalkSfx = null;
			}

			SetPlayerAlpha(1f);
			_invincibleVisualActive = false;
							
			UIKit.ClosePanel<UIGamePanel>();
			UIKit.OpenPanel<UIGameOverPanel>();

			Global.RequestHPUIRefresh.Trigger();
		}

        private bool _mFaceRight = true;

        void Update()
        {
			UpdateInvincibleVisual();

            var move = PlatformInput.GetMoveAxisRaw();
            var horizontal = move.x;
			var vertical = move.y;
			var targetVelocity = move.normalized * MoveSpeed
												* Global.MovementSpeedRate.Value;

			//简单的动作与面向处理
			if(horizontal == 0 &&vertical == 0)
            {
                if (_mFaceRight)
                {
                    Sprite.Play("PlayerIdleRight");
                }
                else
                {
                    Sprite.Play("PlayerIdleLeft");
                }

				//闲置音效
				if(_mWalkSfx != null)
                {
                    _mWalkSfx.Stop();
					_mWalkSfx = null;
                }
            }
            else
            {
				//行走音效
				if(_mWalkSfx == null)
				{
					_mWalkSfx = AudioKit.PlaySound(Sfx.WALK,true);
				}

				if(horizontal > 0)
				{
					_mFaceRight = true;
				}
				else if(horizontal < 0)
                {
					_mFaceRight = false;
				}


                if (_mFaceRight)
                {
                    Sprite.Play("PlayerWalkRight");
                }
                else
                {
                    Sprite.Play("PlayerWalkLeft");
                }
            }

			SelfRigidbody2D.velocity = Vector2.Lerp(SelfRigidbody2D.velocity, targetVelocity, 1-Mathf.Exp(-10f * Time.deltaTime));
        }

        void OnDestroy()
        {
			SetPlayerAlpha(1f);
            if (Default == this)
				Default = null;
        }

		private void CachePlayerRenderers()
		{
			if (Sprite)
			{
				_playerRenderers = Sprite.GetComponentsInChildren<SpriteRenderer>(true);
			}

			if (_playerRenderers == null || _playerRenderers.Length == 0)
			{
				_playerRenderers = GetComponentsInChildren<SpriteRenderer>(true);
			}

			if (_playerRenderers == null)
			{
				_playerRenderers = System.Array.Empty<SpriteRenderer>();
				_rendererBaseColors = System.Array.Empty<Color>();
				return;
			}

			_rendererBaseColors = new Color[_playerRenderers.Length];
			for (var i = 0; i < _playerRenderers.Length; i++)
			{
				_rendererBaseColors[i] = _playerRenderers[i] ? _playerRenderers[i].color : Color.white;
			}
		}

		private void UpdateInvincibleVisual()
		{
			if (!_invincibleVisualActive) return;

			if (Time.time >= _invincibleUntilTime || IsGameOver)
			{
				SetPlayerAlpha(1f);
				_invincibleVisualActive = false;
				return;
			}

			var pulse = Mathf.PingPong(Time.time * InvincibleBlinkSpeed, 1f);
			var alpha = Mathf.Lerp(InvincibleMinAlpha, 1f, pulse);
			SetPlayerAlpha(alpha);
		}

		private void SetPlayerAlpha(float alpha)
		{
			if (_playerRenderers == null || _playerRenderers.Length == 0) return;

			alpha = Mathf.Clamp01(alpha);
			for (var i = 0; i < _playerRenderers.Length; i++)
			{
				var renderer = _playerRenderers[i];
				if (!renderer) continue;

				var baseColor = i < _rendererBaseColors.Length ? _rendererBaseColors[i] : renderer.color;
				renderer.color = new Color(baseColor.r, baseColor.g, baseColor.b, baseColor.a * alpha);
			}
		}
		#endregion
    }
}
