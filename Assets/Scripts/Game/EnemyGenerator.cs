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
		[Serializable]
		public class EnemyWave
        {
			public float GenerateDuration=1;	//每隔多少秒生成一波次敌人
            public GameObject EnemyPrefab;	//敌人预制体
			public int KeepSeconds = 10;	//持续生成多少秒
			
        }


		private float _mCurrentGenerateSeconds = 0;	//生成时间
		private float _mCurrentWaveSeconds = 0;	//当前波次持续时间计时器

		public static BindableProperty<int> EnemyCount = new BindableProperty<int>(0);
		[SerializeField]
		public List<EnemyWave> EnemyWaves = new List<EnemyWave>();	//敌人波次列表

		private Queue<EnemyWave> _mEnemyWaveQueue = new Queue<EnemyWave>();	

		public int WaveCount=0;
		public bool IsLastWave=>WaveCount==EnemyWaves.Count;
		public EnemyWave CurrentWave=>_mCurrentWave;

        void Start()
        {
            foreach(var wave in EnemyWaves)
			{
				_mEnemyWaveQueue.Enqueue(wave);
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
					/// 在玩家周围10个单位的随机位置
					/// </summary>
					var player=Player.Default;
					if(player)
					{
						var randomAngle= UnityEngine.Random.Range(0f,360f);
						var randomRadius=randomAngle*Mathf.Deg2Rad;
						var direction= new Vector3(Mathf.Cos(randomRadius),Mathf.Sin(randomRadius));
						var generate = player.transform.position +direction*10;

						//生成敌人
						_mCurrentWave.EnemyPrefab.Instantiate()
												.Position(generate)
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
