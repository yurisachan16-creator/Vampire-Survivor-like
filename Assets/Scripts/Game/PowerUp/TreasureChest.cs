using UnityEngine;
using QFramework;

namespace VampireSurvivorLike
{
	public partial class TreasureChest : GameplayObject
	{
		void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<CollectableAera>())
            {
                if (other.GetComponent<CollectableAera>())
                {
					UIGamePanel.OpenTreasureChestPanel.Trigger();
                    //TODO：播放音效
					AudioKit.PlaySound("TreasureChest");
					
					this.DestroyGameObjGracefully();
				}
			}
				
    	}

		protected override Collider2D Collider2D => SelfCircleCollider2D;
	}
}
