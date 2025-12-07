using UnityEngine;
using QFramework;
using Unity.Mathematics;
using System.Collections;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace VampireSurvivorLike
{
	public partial class EnemyGenerator : ViewController
	{
		[SerializeField]
		public LevelConfig Config;

		private float _mCurrentGenerateSeconds = 0;	//生成时间
		private float _mCurrentWaveSeconds = 0;	//当前波次持续时间计时器

		public static BindableProperty<int> EnemyCount = new BindableProperty<int>(0);
		[SerializeField]
		public List<EnemyWave> EnemyWaves = new List<EnemyWave>();	//敌人波次列表

		private Queue<EnemyWave> _mEnemyWaveQueue = new Queue<EnemyWave>();	

		public int WaveCount=0;
		private int _mToatalCount=0;
		public bool IsLastWave=>WaveCount==_mToatalCount;
		public EnemyWave CurrentWave=>_mCurrentWave;

        void Start()
        {
			foreach(var group in Config.EnemyWaveGroups)
            {
                foreach(var wave in group.EnemyWaves)
                {
                    _mEnemyWaveQueue.Enqueue(wave);
					_mToatalCount++;
                }
            }
            
        }

		private EnemyWave _mCurrentWave=null;

        void Update()
        {
			if(_mCurrentWave==null && _mEnemyWaveQueue.Count>0)
			{
				WaveCount++;
				_mCurrentWave = _mEnemyWaveQueue.Dequeue();
				_mCurrentGenerateSeconds = 0;
				_mCurrentWaveSeconds = 0;
			}

			if(_mCurrentWave!=null)
            {
                _mCurrentGenerateSeconds += Time.deltaTime;
				_mCurrentWaveSeconds += Time.deltaTime;

				

				if(_mCurrentGenerateSeconds>=_mCurrentWave.GenerateDuration)
                {
                    _mCurrentGenerateSeconds = 0;

					/// <summary>
					/// 初始化玩家
					/// 在以RT，LB为对角点的矩形区域外生成敌人
					/// </summary>
					var player=Player.Default;
					if(player)
					{
						var xOry = RandomUtility.Choose( -1, 1);
						var pos = Vector2.zero;
						if(xOry == -1)
                        {
                            pos.x = RandomUtility.Choose(CameraController.LBTransform.position.x,
														CameraController.RTTransform.position.x);
							pos.y = UnityEngine.Random.Range(CameraController.LBTransform.position.y,
													CameraController.RTTransform.position.y);							
                        }
                        else
                        {
                            pos.x = UnityEngine.Random.Range(CameraController.LBTransform.position.x,
													CameraController.RTTransform.position.x);
							pos.y = RandomUtility.Choose(CameraController.LBTransform.position.y,
														CameraController.RTTransform.position.y);
                        }
						

						//生成敌人
						_mCurrentWave.EnemyPrefab.Instantiate()
												.Position(pos)
												.Self(self=>
												{
													var enemy=self.GetComponent<IEnemy>();
													enemy.SetSpeedScale(_mCurrentWave.SpeedScale);
													enemy.SetHPScale(_mCurrentWave.HPScale);
												})
												.Show();
					}
                }
            }

			if(_mCurrentWaveSeconds>=_mCurrentWave.KeepSeconds)
			{
				_mCurrentWave = null;
				
			}
        }
    }
}
