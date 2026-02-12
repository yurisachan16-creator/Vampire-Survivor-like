using UnityEngine;
using QFramework;
using QAssetBundle;

namespace VampireSurvivorLike
{
	public partial class SimpleKnife : ViewController
	{
		private static readonly System.Collections.Generic.List<Transform> TargetsBuffer = new System.Collections.Generic.List<Transform>(512);
		private const float TargetSearchRadius = 25f;
		private const float KnifeSpeed = 10f;
		private const float KnifeMaxDistanceFromPlayer = 20f;
		

		private float _mCurrentSeconds = 0;

		

        void Update()
        {
            _mCurrentSeconds += Time.deltaTime;

			//每隔一段时间发射一把飞刀
            if (_mCurrentSeconds >= Global.SimpleKnifeDuration.Value)
            {
                _mCurrentSeconds = 0;

				if (!Player.Default) return;

				var targetCount = Global.SimpleKnifeCount.Value + Global.AdditionalFlyThingCount.Value;
				EnemySpatialIndex.GetNearestTargets(Player.Default.transform.position, TargetSearchRadius, targetCount, TargetsBuffer);

				
				var i = 0;
				foreach(var targetTransform in TargetsBuffer)
                {
					//计时器，游戏中最多同时有4个小刀的声音
                    if (i < 4)
                    {
						ActionKit.DelayFrame(11*i,()=>AudioKit.PlaySound(Sfx.KNIFE))
									.StartGlobal();
						i++;
                        
                    }		

                    if (targetTransform)
					{
						var go = ObjectPoolSystem.Spawn(Knife.gameObject, null, true);
						if (!go) continue;
						go.transform.position = this.Position();

						var direction = ((Vector2)targetTransform.position - (Vector2)Player.Default.transform.position).normalized;
						var projectile = go.GetComponent<PooledKnifeProjectile>();
						if (!projectile) projectile = go.AddComponent<PooledKnifeProjectile>();
						projectile.Configure(direction, KnifeSpeed, Global.SimpleKnifeDamage.Value, Global.SuperKnife.Value, Global.SimpleKnifeAttackCount.Value, KnifeMaxDistanceFromPlayer);
					}
                } 
            }
			
        }
    }
}
