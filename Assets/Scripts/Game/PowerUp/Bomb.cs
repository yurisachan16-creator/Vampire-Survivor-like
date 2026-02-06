using UnityEngine;
using QFramework;
using System.Data;

namespace VampireSurvivorLike
{
	public partial class Bomb : GameplayObject
	{
		private void OnEnable()
		{
			var sr = GetComponent<SpriteRenderer>();
			LootGuideSystem.Current?.Register(this, LootGuideKind.Bomb, sr ? sr.sprite : null);
			LootGuideSystem.Current?.TryPlayDropFeedback(transform.position, LootGuideKind.Bomb);
		}

		private void OnDisable()
		{
			LootGuideSystem.Current?.Unregister(this);
		}

		public static void Execute()
        {
            //炸弹效果: 清除屏幕上所有敌人
				foreach(var enemyObj in GameObject.FindGameObjectsWithTag("Enemy"))
				{
					var enemy=enemyObj.GetComponent<IEnemy>();
					if(enemy!=null&&enemyObj.gameObject.activeSelf)
					{
						DamageSystem.CalculateDamage(Global.BombDamage.Value,enemy);
					}
				}
				//TODO：播放炸弹音效
				AudioKit.PlaySound("BombExplosion");
				//屏幕闪烁
				UIGamePanel.FlashScreen.Trigger();
				//屏幕震动
				CameraController.ShakeCamera();
        }
		void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<CollectableAera>())
            {
				Execute();
				
				this.DestroyGameObjGracefully();
            }
        }

		protected override Collider2D Collider2D => SelfCircleCollider2D;
	}
}
