using UnityEngine;
using QFramework;

namespace VampireSurvivorLike
{
	public partial class PowerUpManager : ViewController
	{
		public static PowerUpManager Default { get; private set; }

        void Awake()
        {
            Default = this;
        }

        void OnDestroy()
        {
            if (Default == this)
				Default = null;
        }
    }
}
