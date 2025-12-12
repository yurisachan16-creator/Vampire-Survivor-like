using UnityEngine;
using QFramework;

namespace VampireSurvivorLike
{
	/// <summary>
	/// 可以获取地图上掉落的所有经验值与金币的道具
	/// </summary>
	public partial class GetAllExp : GameplayObject
	{
		void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<CollectableAera>())
            {
				foreach (var exp in FindObjectsByType<Exp>(FindObjectsInactive.Exclude,FindObjectsSortMode.None))
				{
					ActionKit.OnUpdate.Register(() =>
					{
						var player=Player.Default;
                        if (player)
                        {
                            var direction=player.Position()-exp.Position();
							exp.transform.Translate(direction.normalized*Time.deltaTime*5f);
                        }

					}).UnRegisterWhenGameObjectDestroyed(exp);
				}

				foreach (var coin in FindObjectsByType<Coin>(FindObjectsInactive.Exclude,FindObjectsSortMode.None))
				{
					ActionKit.OnUpdate.Register(() =>
					{
						var player=Player.Default;
                        if (player)
                        {
                            var direction=player.Position()-coin.Position();
							coin.transform.Translate(direction.normalized*Time.deltaTime*5f);
                        }

					}).UnRegisterWhenGameObjectDestroyed(coin);
				}

				//TODO：播放音效
				AudioKit.PlaySound("Exp");
				
				this.DestroyGameObjGracefully();
            }
        }

		protected override Collider2D Collider2D => SelfCircleCollider2D;
	}
}
