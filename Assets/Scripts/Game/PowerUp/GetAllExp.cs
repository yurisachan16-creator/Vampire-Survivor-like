using UnityEngine;
using QFramework;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

namespace VampireSurvivorLike
{
	/// <summary>
	/// 可以获取地图上掉落的所有经验值与金币的道具
	/// </summary>
	public partial class GetAllExp : GameplayObject
	{
		private static IEnumerator FlyToPlayerStart()
		{
			IEnumerable<PowerUp> exps = FindObjectsByType<Exp>(FindObjectsInactive.Exclude,FindObjectsSortMode.None);
			IEnumerable<PowerUp> coins = FindObjectsByType<Coin>(FindObjectsInactive.Exclude,FindObjectsSortMode.None);
			int count = 0;

			foreach(var powerUp in exps.Concat(coins)
						.OrderByDescending(e=>e.InScreen)
						.ThenBy(e=>e.Distance2D(Player.Default)))
			{
				//确保经验值和金币在屏幕内才开始飞向玩家
				//否则等待下一帧执行
				if (powerUp.InScreen)
				{
					if(count > 5)
					{
						count = 0;
						yield return new WaitForEndOfFrame();
						
					}
				}
				else
				{
					if(count > 2)
					{
						count = 0;
						yield return new WaitForEndOfFrame();
						
					}
				}

				count++;
				// 直接触发道具的飞行模式，使用 PowerUp 自带的飞行逻辑
				powerUp.FlyingToPalyer = true;
			}
		}

		void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<CollectableAera>())
            {
				PowerUpManager.Default.StartCoroutine(FlyToPlayerStart());

				//TODO：播放音效
				AudioKit.PlaySound("Exp");
				
				this.DestroyGameObjGracefully();
            }
        }

		protected override Collider2D Collider2D => SelfCircleCollider2D;
	}
}
