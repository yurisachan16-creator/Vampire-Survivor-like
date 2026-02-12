using UnityEngine;
using QFramework;

namespace VampireSurvivorLike
{
	public partial class SimpleSword : ViewController
	{
		private float _mCurrentSecond=0;
		private static readonly System.Collections.Generic.List<Transform> TargetsBuffer = new System.Collections.Generic.List<Transform>(512);

		

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

				//计算数量和伤害倍数
				var countTimes=Global.SuperSword.Value ? 2 : 1;
				var damageTimes=Global.SuperSword.Value ? UnityEngine.Random.Range(2,3+1) : 1;
				var distanceTimes=Global.SuperSword.Value ? 2 : 1;

				if (!Player.Default) return;

				var range = Global.SimpleSwordRange.Value * distanceTimes;
				var targetCount = (Global.SimpleSwordCount.Value + Global.AdditionalFlyThingCount.Value) * countTimes;
				EnemySpatialIndex.GetNearestTargets(Player.Default.transform.position, range, targetCount, TargetsBuffer);

				foreach (var targetTransform in TargetsBuffer)
								
				{
					if (!targetTransform) continue;
					var go = ObjectPoolSystem.Spawn(Sword.gameObject, null, true);
					if (!go) continue;

					go.transform.position = targetTransform.position + Vector3.left * 0.25f;
					var slash = go.GetComponent<PooledSwordSlash>();
					if (!slash) slash = go.AddComponent<PooledSwordSlash>();
					slash.Configure(Global.SimpleAbilityDamage.Value * damageTimes);
				}
			}
        }
    }
}
