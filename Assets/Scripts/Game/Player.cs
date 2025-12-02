using UnityEngine;
using QFramework;

namespace VampireSurvivorLike
{
	public partial class Player : ViewController
	{
		public float MoveSpeed = 5f;

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
							AudioKit.PlaySound("");
							//玩家死亡,销毁自己
							this.DestroyGameObjGracefully();
							//重置游戏数据
							Global.ResetData();

							//显示游戏结束面板
							UIKit.OpenPanel<UIGameOverPanel>();
						}
						else
						{
							//播放受伤音效
							AudioKit.PlaySound("");
						}
						
					}
				}
			}).UnRegisterWhenGameObjectDestroyed(gameObject);
		}

        

        void Update()
        {
            var horizontal = Input.GetAxis("Horizontal");
			var vertical = Input.GetAxis("Vertical");

			//方向归一化
			var direction = new Vector2(horizontal, vertical).normalized;

			SelfRigidbody2D.velocity = direction * MoveSpeed;
        }

        void OnDestroy()
        {
            if (Default == this)
				Default = null;
        }
		#endregion
    }
}
