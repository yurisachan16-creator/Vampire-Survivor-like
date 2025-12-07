using UnityEngine;
using QFramework;

namespace VampireSurvivorLike
{
	public partial class RecoverHP : ViewController
	{
		void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<CollectableAera>())
            {
				if(Global.HP.Value==Global.MaxHP.Value)
                {
                    
                }
                else
                {
                    AudioKit.PlaySound("Health");
					Global.HP.Value += 1;
					this.DestroyGameObjGracefully();
                }
            }
        }
	}
}
