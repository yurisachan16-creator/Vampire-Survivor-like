using UnityEngine;
using QFramework;

namespace VampireSurvivorLike
{
	public partial class SimpleAbility : ViewController
	{
		private float _mCurrentSecond=0;
		void Start()
		{
			// Code Here
		}

        void Update()
        {
            _mCurrentSecond += Time.deltaTime;
			if(_mCurrentSecond>=Global.SimpleAbilityDuration.Value)
			{
                _mCurrentSecond = 0;

				var enemies = FindObjectsByType<Enemy>(FindObjectsInactive.Exclude,FindObjectsSortMode.None);

				foreach(var enemy in enemies)
				{
					var distance =(Player.Default.transform.position-enemy.transform.position).magnitude;

					if(distance<=5f)
					{
						enemy.Hurt(Global.SimpleAbilityDamage.Value);
						
					}
				}
			}
        }
    }
}
