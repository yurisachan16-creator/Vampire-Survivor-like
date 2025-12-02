using UnityEngine;
using QFramework;
using System.Data;

namespace VampireSurvivorLike
{
	public partial class Bomb : ViewController
	{
		void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<CollectableAera>())
            {
				//炸弹效果: 清除屏幕上所有敌人
				foreach(var enemyObj in GameObject.FindGameObjectsWithTag("Enemy"))
				{
					var enemy=enemyObj.GetComponent<Enemy>();
					if(enemy&&enemy.gameObject.activeSelf)
					{
						enemy.Hurt(enemy.Health);
					}
				}
				//TODO：播放炸弹音效
				AudioKit.PlaySound("");
				//屏幕震动
				CameraController.ShakeCamera();
				
				this.DestroyGameObjGracefully();
            }
        }
	}
}
