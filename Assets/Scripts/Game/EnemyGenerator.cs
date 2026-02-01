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

		[SerializeField]
		[Tooltip("敌人预制体映射表，用于将CSV中的敌人名称映射到实际预制体")]
		public EnemyPrefabMapping PrefabMapping;

		[SerializeField]
		[Tooltip("是否从CSV配置文件加载波次数据")]
		public bool UseCSVConfig = false;

		private float _mCurrentGenerateSeconds = 0;	//生成时间
		private float _mCurrentWaveSeconds = 0;	//当前波次持续时间计时器

		public static BindableProperty<int> EnemyCount = new BindableProperty<int>(0);
		[SerializeField]
		public List<EnemyWave> EnemyWaves = new List<EnemyWave>();	//敌人波次列表

		private Queue<EnemyWave> _mEnemyWaveQueue = new Queue<EnemyWave>();	

		public int WaveCount=0;
		private int _mToatalCount=0;
		public bool IsInitialized => _isInitialized;
		public bool IsLastWave=> _isInitialized && WaveCount==_mToatalCount && _mToatalCount > 0;
		public EnemyWave CurrentWave=>_mCurrentWave;

		private bool _isInitialized = false;

        IEnumerator Start()
        {
			if (UseCSVConfig && PrefabMapping != null)
			{
				// 从CSV加载配置
				yield return LoadFromCSVAsync();
			}
			else
			{
				// 使用ScriptableObject配置
				LoadFromScriptableObject();
			}

			_isInitialized = true;
        }

		/// <summary>
		/// 从ScriptableObject加载波次配置
		/// </summary>
		private void LoadFromScriptableObject()
		{
			foreach(var group in Config.EnemyWaveGroups)
            {
                foreach(var wave in group.EnemyWaves)
                {
					if (!wave.Active) continue;
                    _mEnemyWaveQueue.Enqueue(wave);
					_mToatalCount++;
                }
            }
		}

		/// <summary>
		/// 异步从CSV加载波次配置
		/// </summary>
		private IEnumerator LoadFromCSVAsync()
		{
			List<EnemyWaveConfigRow> configRows = null;
			
			yield return EnemyWaveConfigLoader.LoadAsync(rows => configRows = rows);

			if (configRows == null || configRows.Count == 0)
			{
				Debug.LogWarning("[EnemyGenerator] CSV配置为空，回退到ScriptableObject配置");
				LoadFromScriptableObject();
				yield break;
			}

			// 转换配置并解析预制体
			foreach (var row in configRows)
			{
				if (!row.Active) continue;

				var wave = EnemyWave.FromConfigRow(row);
				
				// 通过映射表获取预制体
				wave.EnemyPrefab = PrefabMapping.GetPrefab(row.EnemyPrefabName);
				
				if (wave.EnemyPrefab == null)
				{
					Debug.LogWarning($"[EnemyGenerator] 跳过波次 '{row.WaveName}'：未找到预制体 '{row.EnemyPrefabName}'");
					continue;
				}

				_mEnemyWaveQueue.Enqueue(wave);
				_mToatalCount++;
			}

			Debug.Log($"[EnemyGenerator] 从CSV成功加载 {_mToatalCount} 个波次");
		}

		private EnemyWave _mCurrentWave=null;

        void Update()
        {
			// 等待初始化完成
			if (!_isInitialized) return;

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
						
						var currentWave = _mCurrentWave;
						//生成敌人
						currentWave.EnemyPrefab.Instantiate()
												.Position(pos)
												.Self(self=>
												{
													var enemy=self.GetComponent<IEnemy>();
													enemy.SetSpeedScale(currentWave.SpeedScale);
													enemy.SetHPScale(currentWave.HPScale);
													enemy.SetDamageScale(currentWave.DamageScale);
												})
												.Show();
					}
                }
            }

			if(_mCurrentWave!=null && _mCurrentWaveSeconds>=_mCurrentWave.KeepSeconds)
			{
				_mCurrentWave = null;
				
			}
        }
    }
}
