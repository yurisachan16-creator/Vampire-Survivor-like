using UnityEngine;
using QFramework;

namespace VampireSurvivorLike
{
	public partial class CollectableAera : ViewController
	{
		void Start()
		{
			Global.CollectableAreaRadius.Register(range =>
			{
				GetComponent<CircleCollider2D>().radius = range;

			}).UnRegisterWhenGameObjectDestroyed(gameObject);
		}
	}
}
