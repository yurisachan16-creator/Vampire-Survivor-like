using UnityEngine;
using QFramework;

namespace VampireSurvivorLike
{
	public partial class TreasureChest : GameplayObject
	{
		private void OnEnable()
		{
			var sr = GetComponent<SpriteRenderer>();
			LootGuideSystem.Current?.Register(this, LootGuideKind.TreasureChest, sr ? sr.sprite : null);
		}

		private void OnDisable()
		{
			LootGuideSystem.Current?.Unregister(this);
		}

		void OnTriggerEnter2D(Collider2D other)
        {
			if (!other.TryGetComponent<CollectableAera>(out _)) return;

			UIGamePanel.OpenTreasureChestPanel.Trigger();
            //TODO：播放音效
			AudioKit.PlaySound("Retro Event Acute 08");
			this.DestroyGameObjGracefully();
				
    	}

		protected override Collider2D Collider2D => SelfCircleCollider2D;
	}
}
