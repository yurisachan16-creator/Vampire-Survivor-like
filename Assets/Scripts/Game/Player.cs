using UnityEngine;
using QFramework;
using QAssetBundle;

namespace VampireSurvivorLike
{
	public partial class Player : ViewController
	{
		public float MoveSpeed = 5f;
		private AudioPlayer _mWalkSfx;

		public static Player Default { get; private set; }

		#region 生命周期函数

		void Awake()
        {
            Default = this;
        }

		void Start()
		{	
            HurtBox.OnTriggerEnter2DEvent(Collider2D =>
			{
				var hitBox = Collider2D.GetComponent<HitBox>();
				if(hitBox)
				{
					if(hitBox.Owner.CompareTag("Enemy"))
					{
						//玩家受伤
						Global.HP.Value -= 1;

						if(Global.HP.Value<=0)
						{
							//播放死亡音效
							AudioKit.PlaySound("Death");
							//玩家死亡,销毁自己
							this.DestroyGameObjGracefully();
							//暂停游戏
							Time.timeScale = 0f;
							//关闭游戏面板
							UIKit.ClosePanel<UIGamePanel>();
							//显示游戏结束面板
							UIKit.OpenPanel<UIGameOverPanel>();
						}
						else
						{
							//播放受伤音效
							AudioKit.PlaySound("Hurt");
						}
						
					}
				}
			}).UnRegisterWhenGameObjectDestroyed(gameObject);

			void UPdateHP()
            {
                HPValue.fillAmount = Global.HP.Value / (float)Global.MaxHP.Value;
            }

			Global.HP.RegisterWithInitValue(hp =>
			{
				UPdateHP();
			}).UnRegisterWhenGameObjectDestroyed(gameObject);

			Global.MaxHP.RegisterWithInitValue(maxHp =>
			{
				UPdateHP();
			}).UnRegisterWhenGameObjectDestroyed(gameObject);
		}

        private bool _mFaceRight = true;

        void Update()
        {
            var horizontal = Input.GetAxisRaw("Horizontal");
			var vertical = Input.GetAxisRaw("Vertical");
			var targetVelocity = new Vector2(horizontal, vertical).normalized * MoveSpeed
												* Global.MovementSpeedRate.Value;

			//简单的动作与面向处理
			if(horizontal == 0 &&vertical == 0)
            {
                if (_mFaceRight)
                {
                    Sprite.Play("PLayerIdleRight");
                }
                else
                {
                    Sprite.Play("PLayerIdleLeft");
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
