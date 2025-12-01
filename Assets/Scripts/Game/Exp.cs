using UnityEngine;
using QFramework;

namespace VampireSurvivorLike
{
	public partial class Exp : ViewController
	{
        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<CollectableAera>())
            {
                Global.Exp.Value += 1;
				this.DestroyGameObjGracefully();
            }
        }
    }
}
