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
		private void OnEnable()
		{
			PowerUpRegistry.ActiveGetAllExpCount++;
			var sr = GetComponent<SpriteRenderer>();
			LootGuideSystem.Current?.Register(this, LootGuideKind.GetAllExp, sr ? sr.sprite : null);
			LootGuideSystem.Current?.TryPlayDropFeedback(transform.position, LootGuideKind.GetAllExp);
		}

		private void OnDisable()
		{
			PowerUpRegistry.ActiveGetAllExpCount--;
			LootGuideSystem.Current?.Unregister(this);
		}

		// 预分配列表，避免每次调用 FlyToPlayerStart 时分配
		private static readonly List<PowerUp> _tempPowerUps = new List<PowerUp>(2048);

		private static IEnumerator FlyToPlayerStart()
		{
			// 使用 PowerUpRegistry 替代 FindObjectsByType，零分配
			PowerUpRegistry.CollectAllExpAndCoins(_tempPowerUps);

			// 按屏幕内优先、距离排序
			_tempPowerUps.Sort((a, b) =>
			{
				var aIn = a.InScreen ? 0 : 1;
				var bIn = b.InScreen ? 0 : 1;
				if (aIn != bIn) return aIn.CompareTo(bIn);
				var aDist = Player.Default ? a.Distance2D(Player.Default) : 0f;
				var bDist = Player.Default ? b.Distance2D(Player.Default) : 0f;
				return aDist.CompareTo(bDist);
			});

			int count = 0;
			for (int i = 0; i < _tempPowerUps.Count; i++)
			{
				var powerUp = _tempPowerUps[i];
				if (!powerUp || !powerUp.gameObject.activeInHierarchy) continue;

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
				powerUp.FlyingToPalyer = true;
			}

			_tempPowerUps.Clear();
		}

		void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<CollectableAera>())
            {
				PowerUpManager.Default.StartCoroutine(FlyToPlayerStart());

				if (SfxThrottle.CanPlay("Exp", 0.1f))
					AudioKit.PlaySound("Exp");
				
				this.DestroyGameObjGracefully();
            }
        }

		protected override Collider2D Collider2D => SelfCircleCollider2D;
	}
}
