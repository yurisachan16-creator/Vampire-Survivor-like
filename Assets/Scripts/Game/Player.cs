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
		private const float InvincibleHintCooldownSeconds = 0.2f;
		private const float DamageHintOffsetY = 0.9f;
		private AudioPlayer _mWalkSfx;
		private float _invincibleUntilTime;
		private float _nextInvincibleHintTime;
		private int _lastDamageFrame = -1;
		private string _lastDamageSource;

		public static Player Default { get; private set; }
		public bool IsGameOver => Global.IsGameOver.Value;

		#region 生命周期函数

		void Awake()
        {
            Default = this;
        }

		void Start()
		{	
            HurtBox.OnTriggerEnter2DEvent(Collider2D =>
			{
				if (IsGameOver) return;

				var hitHurtBox = Collider2D.GetComponent<HitHurtBox>();
				if(hitHurtBox)
				{
					if(hitHurtBox.Owner.CompareTag("Enemy"))
					{
						var boss = hitHurtBox.Owner.GetComponent<EnemyMiniBoss>();
						var bossId = boss ? boss.BossType.ToString() : string.Empty;
						ApplyDamage(1, bossId, boss ? "BossMelee" : "EnemyMelee");
						
					}
				}
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

			if (_lastDamageFrame == Time.frameCount && _lastDamageSource == damageSource) return false;
			if (!ignoreInvincible && _lastDamageFrame != Time.frameCount && Time.time < _invincibleUntilTime)
			{
				ShowInvincibleHint("无敌中");
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
			ShowInvincibleHint($"无敌 {InvincibleSeconds:0.#} 秒", true);
			Global.RequestHPUIRefresh.Trigger();
			return true;
		}

		private void ShowInvincibleHint(string text, bool ignoreCooldown = false)
		{
			if (!ignoreCooldown && Time.time < _nextInvincibleHintTime) return;
			_nextInvincibleHintTime = Time.time + InvincibleHintCooldownSeconds;

			FloatingTextController.Play(transform.position + Vector3.up * DamageHintOffsetY, text, false);
		}

		private void GameOver(string bossId, string damageSource)
		{
			if (IsGameOver) return;

			Global.IsGameOver.Value = true;
			Global.ReportPlayerDeath(bossId, damageSource);

			AudioKit.PlaySound(Sfx.DEATH);
			Time.timeScale = 0;

			if(_mWalkSfx != null)
			{
				_mWalkSfx.Stop();
				_mWalkSfx = null;
			}
							
			UIKit.ClosePanel<UIGamePanel>();
			UIKit.OpenPanel<UIGameOverPanel>();

			Global.RequestHPUIRefresh.Trigger();
		}

        private bool _mFaceRight = true;

        void Update()
        {
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
            if (Default == this)
				Default = null;
        }
		#endregion
    }
}
