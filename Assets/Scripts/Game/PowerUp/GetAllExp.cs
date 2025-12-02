using UnityEngine;
using QFramework;

namespace VampireSurvivorLike
{
	public partial class GetAllExp : ViewController
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

					});
				}
				//TODO：播放音效
				AudioKit.PlaySound("");
				
				this.DestroyGameObjGracefully();
            }
        }
	}
}
