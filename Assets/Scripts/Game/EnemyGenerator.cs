using UnityEngine;
using QFramework;
using Unity.Mathematics;
using System.Collections;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Linq;

namespace VampireSurvivorLike
{
	public enum WaveSpawnPhase
	{
		Small = 0,
		Boss = 1
	}

	public enum MixedWaveStrategy
	{
		SeparateBossWave = 0,
		SeamlessBossAfterSmall = 1
	}

	[Serializable]
	public class WaveSegmentDefinition
	{
		public string DisplayName;
		public WaveSpawnPhase Phase;
		public GameObject Prefab;

		[Tooltip("刷新间隔（秒）：每隔多少秒生成 1 个该段落的敌人")]
		public float SpawnIntervalSeconds = 1f;

		[Tooltip("刷新持续时间（秒）：持续生成多久，超过后该段落不再生成新敌人")]
		public float SpawnDurationSeconds = 10f;

		[Tooltip("刷新数量上限（可选）：>0 时按数量刷新并忽略 SpawnDurationSeconds")]
		public int SpawnCount = 0;

		[Tooltip("该段落刷新结束后的最大等待时间（秒）：避免拖波/干等")]
		public float MaxWaitAfterSpawnSeconds = 3f;

		[Header("数值与掉落（来自配置）")]
		public float HPScale = 1f;
		public float SpeedScale = 1f;
		public float DamageScale = 1f;
		public float BaseSpeed = 2f;
		public bool IsTreasureChest = false;
		public float ExpDropRate = 0.3f;
		public float CoinDropRate = 0.3f;
		public float HpDropRate = 0.1f;
		public float BombDropRate = 0.05f;
	}

	[Serializable]
	public class WaveGroupDefinition
	{
		public string GroupName;
		public string GroupDescription;

		[Tooltip("是否允许将本组作为混合波处理（可被全局开关覆写）")]
		public bool AllowMixedWave = true;

		public List<WaveSegmentDefinition> Segments = new List<WaveSegmentDefinition>();
	}

	public readonly struct WaveSpawnRequest
	{
		public readonly WaveSpawnPhase Phase;
		public readonly GameObject Prefab;
		public readonly WaveSegmentDefinition Segment;

		public WaveSpawnRequest(WaveSpawnPhase phase, GameObject prefab, WaveSegmentDefinition segment)
		{
			Phase = phase;
			Prefab = prefab;
			Segment = segment;
		}
	}

	public interface IWaveWorld
	{
		int SmallAliveCount { get; }
		int BossAliveCount { get; }
		bool HasAnyAliveEnemies { get; }
		bool TrySpawn(in WaveSpawnRequest request);
	}

	public enum WaveControllerState
	{
		Waiting = 0,
		SpawningSmall = 1,
		SpawningBoss = 2,
		Completed = 3
	}

	public sealed class WaveController
	{
		private sealed class SegmentRuntime
		{
			public WaveSegmentDefinition Definition;
			public float SegmentTimer;
			public float SpawnTimer;
			public int SpawnedCount;
			public bool Finished;
		}

		private sealed class GroupRuntime
		{
			public WaveGroupDefinition Definition;
			public List<SegmentRuntime> SmallSegments = new List<SegmentRuntime>();
			public List<SegmentRuntime> BossSegments = new List<SegmentRuntime>();

			public float MaxWaitAfterSpawnSeconds;
			public bool BossSpawnSubmitted;
			public bool AllSmallSpawningFinished;
			public bool AllBossSpawningFinished;
			public float PostSpawnTimer;
			public float EmptySceneTimer;
		}

		public bool AllowMixedWave = true;
		public MixedWaveStrategy MixedStrategy = MixedWaveStrategy.SeamlessBossAfterSmall;
		public float EmptySceneBossTriggerSeconds = 0.5f;
		public float EmptySceneCheckIntervalSeconds = 0.1f;

		private readonly Queue<GroupRuntime> _queue = new Queue<GroupRuntime>();
		private GroupRuntime _current;
		private float _emptyCheckTimer;

		public WaveControllerState State { get; private set; } = WaveControllerState.Waiting;
		public int CurrentWaveIndex { get; private set; }
		public int TotalWaveCount { get; private set; }
		public float RemainingSeconds { get; private set; }
		public string CurrentWaveName => _current == null ? string.Empty : GetDisplayName(_current);

		public void Load(IReadOnlyList<WaveGroupDefinition> groups)
		{
			_queue.Clear();
			_current = null;
			State = WaveControllerState.Waiting;
			CurrentWaveIndex = 0;
			TotalWaveCount = 0;
			RemainingSeconds = 0;
			_emptyCheckTimer = 0;

			if (groups == null) return;

			var normalized = NormalizeGroups(groups);
			TotalWaveCount = normalized.Count;
			foreach (var g in normalized)
			{
				_queue.Enqueue(BuildRuntime(g));
			}
		}

		public void Tick(float deltaTime, IWaveWorld world)
		{
			if (world == null) return;

			RemainingSeconds = 0;

			if (_current == null)
			{
				TryStartNextWave();
			}

			if (_current == null)
			{
				State = WaveControllerState.Waiting;
				return;
			}

			switch (State)
			{
				case WaveControllerState.SpawningSmall:
					TickSpawningSmall(deltaTime, world);
					break;
				case WaveControllerState.SpawningBoss:
					TickSpawningBoss(deltaTime, world);
					break;
				case WaveControllerState.Completed:
					TickCompleted(deltaTime, world);
					break;
				default:
					State = WaveControllerState.Waiting;
					break;
			}
		}

		private void TickSpawningSmall(float dt, IWaveWorld world)
		{
			TickSegments(dt, world, _current.SmallSegments);

			var hasBoss = _current.BossSegments.Count > 0;
			var isMixed = _current.SmallSegments.Count > 0 && _current.BossSegments.Count > 0;
			var mixedAllowed = AllowMixedWave && _current.Definition.AllowMixedWave && isMixed;

			if (mixedAllowed && MixedStrategy == MixedWaveStrategy.SeamlessBossAfterSmall && hasBoss && !_current.BossSpawnSubmitted)
			{
				TickEmptySceneBossTrigger(dt, world);
				if (_current.EmptySceneTimer >= EmptySceneBossTriggerSeconds)
				{
					_current.AllSmallSpawningFinished = true;
				}
			}

			_current.AllSmallSpawningFinished = _current.AllSmallSpawningFinished || _current.SmallSegments.All(s => s.Finished);

			if (mixedAllowed && MixedStrategy == MixedWaveStrategy.SeamlessBossAfterSmall && hasBoss && _current.AllSmallSpawningFinished)
			{
				State = WaveControllerState.SpawningBoss;
				return;
			}

			if (!hasBoss && _current.AllSmallSpawningFinished)
			{
				State = WaveControllerState.Completed;
				_current.PostSpawnTimer = 0;
				return;
			}

			RemainingSeconds = GetSpawningRemainingSeconds(_current.SmallSegments);
		}

		private void TickSpawningBoss(float dt, IWaveWorld world)
		{
			_current.BossSpawnSubmitted = true;

			foreach (var boss in _current.BossSegments)
			{
				SubmitBossSpawn(world, boss);
			}

			_current.AllBossSpawningFinished = _current.BossSegments.All(s => s.Finished);

			if (_current.AllBossSpawningFinished)
			{
				State = WaveControllerState.Completed;
				_current.PostSpawnTimer = 0;
			}
		}

		private void TickCompleted(float dt, IWaveWorld world)
		{
			if (!world.HasAnyAliveEnemies)
			{
				EndWaveAndAdvance();
				return;
			}

			if (_current.MaxWaitAfterSpawnSeconds > 0f)
			{
				_current.PostSpawnTimer += dt;
				RemainingSeconds = Mathf.Max(0f, _current.MaxWaitAfterSpawnSeconds - _current.PostSpawnTimer);
				if (_current.PostSpawnTimer >= _current.MaxWaitAfterSpawnSeconds)
				{
					EndWaveAndAdvance();
				}
			}
		}

		private void TickSegments(float dt, IWaveWorld world, List<SegmentRuntime> segments)
		{
			foreach (var seg in segments)
			{
				if (seg.Finished) continue;

				if (seg.Definition.SpawnCount > 0 && seg.SpawnedCount >= seg.Definition.SpawnCount)
				{
					seg.Finished = true;
					continue;
				}

				if (seg.Definition.SpawnCount <= 0)
				{
					seg.SegmentTimer += dt;
					if (seg.SegmentTimer >= seg.Definition.SpawnDurationSeconds)
					{
						seg.Finished = true;
						continue;
					}
				}

				seg.SpawnTimer += dt;
				if (seg.SpawnTimer >= Mathf.Max(0.01f, seg.Definition.SpawnIntervalSeconds))
				{
					seg.SpawnTimer = 0;
					if (world.TrySpawn(new WaveSpawnRequest(seg.Definition.Phase, seg.Definition.Prefab, seg.Definition)))
					{
						seg.SpawnedCount++;
					}
				}
			}
		}

		private void TickEmptySceneBossTrigger(float dt, IWaveWorld world)
		{
			_emptyCheckTimer += dt;
			var checkInterval = Mathf.Max(0.02f, EmptySceneCheckIntervalSeconds);
			if (_emptyCheckTimer < checkInterval) return;
			var elapsed = _emptyCheckTimer;
			_emptyCheckTimer = 0;

			if (world.SmallAliveCount == 0 && world.BossAliveCount == 0)
			{
				_current.EmptySceneTimer += elapsed;
			}
			else
			{
				_current.EmptySceneTimer = 0;
			}
		}

		private void SubmitBossSpawn(IWaveWorld world, SegmentRuntime boss)
		{
			if (boss.Finished) return;

			var targetCount = boss.Definition.SpawnCount > 0 ? boss.Definition.SpawnCount : 1;
			while (boss.SpawnedCount < targetCount)
			{
				if (world.TrySpawn(new WaveSpawnRequest(WaveSpawnPhase.Boss, boss.Definition.Prefab, boss.Definition)))
				{
					boss.SpawnedCount++;
				}
				else
				{
					break;
				}
			}

			boss.Finished = boss.SpawnedCount >= targetCount;
		}

		private void TryStartNextWave()
		{
			if (_queue.Count == 0) return;
			_current = _queue.Dequeue();
			CurrentWaveIndex++;
			State = _current.SmallSegments.Count > 0 ? WaveControllerState.SpawningSmall : WaveControllerState.SpawningBoss;
		}

		private void EndWaveAndAdvance()
		{
			_current = null;
			State = WaveControllerState.Waiting;
			RemainingSeconds = 0;
			_emptyCheckTimer = 0;
		}

		private static float GetSpawningRemainingSeconds(List<SegmentRuntime> segments)
		{
			var seg = segments.FirstOrDefault(s => !s.Finished);
			if (seg == null) return 0;
			if (seg.Definition.SpawnCount > 0) return 0;
			return Mathf.Max(0f, seg.Definition.SpawnDurationSeconds - seg.SegmentTimer);
		}

		private static string GetDisplayName(GroupRuntime g)
		{
			var seg = g.SmallSegments.FirstOrDefault(s => !s.Finished) ?? g.BossSegments.FirstOrDefault(s => !s.Finished);
			return seg?.Definition.DisplayName ?? g.Definition.GroupName ?? string.Empty;
		}

		private List<WaveGroupDefinition> NormalizeGroups(IReadOnlyList<WaveGroupDefinition> groups)
		{
			var result = new List<WaveGroupDefinition>();

			foreach (var g in groups)
			{
				if (g == null) continue;
				var hasSmall = g.Segments.Any(s => s != null && s.Phase == WaveSpawnPhase.Small);
				var hasBoss = g.Segments.Any(s => s != null && s.Phase == WaveSpawnPhase.Boss);
				var isMixed = hasSmall && hasBoss;
				var mixedAllowed = AllowMixedWave && g.AllowMixedWave && isMixed;

				if (mixedAllowed && MixedStrategy == MixedWaveStrategy.SeparateBossWave)
				{
					var smallGroup = new WaveGroupDefinition
					{
						GroupName = g.GroupName,
						GroupDescription = g.GroupDescription,
						AllowMixedWave = g.AllowMixedWave,
						Segments = g.Segments.Where(s => s != null && s.Phase == WaveSpawnPhase.Small).ToList()
					};
					if (smallGroup.Segments.Count > 0) result.Add(smallGroup);

					var bossGroup = new WaveGroupDefinition
					{
						GroupName = g.GroupName,
						GroupDescription = g.GroupDescription,
						AllowMixedWave = g.AllowMixedWave,
						Segments = g.Segments.Where(s => s != null && s.Phase == WaveSpawnPhase.Boss).ToList()
					};
					if (bossGroup.Segments.Count > 0) result.Add(bossGroup);
				}
				else if (isMixed && !mixedAllowed)
				{
					var legacyGroup = new WaveGroupDefinition
					{
						GroupName = g.GroupName,
						GroupDescription = g.GroupDescription,
						AllowMixedWave = false,
						Segments = g.Segments
							.Where(s => s != null)
							.Select(s => new WaveSegmentDefinition
							{
								DisplayName = s.DisplayName,
								Phase = WaveSpawnPhase.Small,
								Prefab = s.Prefab,
								SpawnIntervalSeconds = s.SpawnIntervalSeconds,
								SpawnDurationSeconds = s.SpawnDurationSeconds,
								SpawnCount = s.SpawnCount,
								MaxWaitAfterSpawnSeconds = s.MaxWaitAfterSpawnSeconds,
								HPScale = s.HPScale,
								SpeedScale = s.SpeedScale,
								DamageScale = s.DamageScale,
								BaseSpeed = s.BaseSpeed,
								IsTreasureChest = s.IsTreasureChest,
								ExpDropRate = s.ExpDropRate,
								CoinDropRate = s.CoinDropRate,
								HpDropRate = s.HpDropRate,
								BombDropRate = s.BombDropRate
							})
							.ToList()
					};

					result.Add(legacyGroup);
				}
				else
				{
					result.Add(g);
				}
			}

			return result;
		}

		private static GroupRuntime BuildRuntime(WaveGroupDefinition def)
		{
			var rt = new GroupRuntime { Definition = def };

			foreach (var seg in def.Segments)
			{
				if (seg == null) continue;
				var srt = new SegmentRuntime { Definition = seg };
				if (seg.Phase == WaveSpawnPhase.Boss) rt.BossSegments.Add(srt);
				else rt.SmallSegments.Add(srt);
			}

			var maxWait = 0f;
			foreach (var s in def.Segments)
			{
				if (s == null) continue;
				maxWait = Mathf.Max(maxWait, s.MaxWaitAfterSpawnSeconds);
			}
			rt.MaxWaitAfterSpawnSeconds = maxWait;

			return rt;
		}
	}

	public partial class EnemyGenerator : ViewController, IWaveWorld
	{
		[SerializeField]
		public LevelConfig Config;

		[SerializeField]
		[Tooltip("敌人预制体映射表，用于将CSV中的敌人名称映射到实际预制体")]
		public EnemyPrefabMapping PrefabMapping;

		[SerializeField]
		[Tooltip("是否从CSV配置文件加载波次数据")]
		public bool UseCSVConfig = false;

		[Header("混合波策略")]
		[Tooltip("是否允许混合波处理（向后兼容开关，可用于线上热更回退）")]
		public bool AllowMixedWave = true;

		[Tooltip("混合波策略：A 拆分 Boss 为独立波；B 小怪清空后立即刷 Boss")]
		public MixedWaveStrategy MixedStrategy = MixedWaveStrategy.SeamlessBossAfterSmall;

		[Tooltip("空场检测阈值（秒）：小怪归零且 Boss 未生成时，连续空场达到该时间则强制刷 Boss")]
		public float EmptySceneBossTriggerSeconds = 0.5f;

		[Tooltip("空场检测节流（秒）：避免每帧做重检查")]
		public float EmptySceneCheckIntervalSeconds = 0.1f;

		public static BindableProperty<int> EnemyCount = new BindableProperty<int>(0);
		public static BindableProperty<int> SmallEnemyCount = new BindableProperty<int>(0);
		public static BindableProperty<int> BossEnemyCount = new BindableProperty<int>(0);
		/// <summary>
		/// 当前波次编号（可绑定显示）
		/// </summary>
		public static BindableProperty<int> CurrentWaveIndex = new BindableProperty<int>(0);
		/// <summary>
		/// 总波次数（可绑定显示）
		/// </summary>
		public static BindableProperty<int> TotalWaveCount = new BindableProperty<int>(0);
		/// <summary>
		/// 当前波次名称
		/// </summary>
		public static BindableProperty<string> CurrentWaveName = new BindableProperty<string>("");
		/// <summary>
		/// 当前波次剩余时间（秒）
		/// </summary>
		public static BindableProperty<float> WaveRemainingTime = new BindableProperty<float>(0);
		[SerializeField]
		public List<EnemyWave> EnemyWaves = new List<EnemyWave>();	//敌人波次列表

		public int WaveCount=0;
		private int _mToatalCount=0;
		public bool IsInitialized => _isInitialized;
		public bool IsLastWave=> _isInitialized && WaveCount==_mToatalCount && _mToatalCount > 0;
		public bool IsAllWavesFinished => _isInitialized && _waveController != null && _waveController.State == WaveControllerState.Waiting && _waveController.CurrentWaveIndex >= _mToatalCount;

		private bool _isInitialized = false;
		private WaveController _waveController;

        IEnumerator Start()
        {
			_waveController = new WaveController
			{
				AllowMixedWave = AllowMixedWave,
				MixedStrategy = MixedStrategy,
				EmptySceneBossTriggerSeconds = EmptySceneBossTriggerSeconds,
				EmptySceneCheckIntervalSeconds = EmptySceneCheckIntervalSeconds
			};

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
			TotalWaveCount.Value = _mToatalCount;
        }

		/// <summary>
		/// 从ScriptableObject加载波次配置
		/// </summary>
		private void LoadFromScriptableObject()
		{
			var groups = new List<WaveGroupDefinition>();

			foreach (var group in Config.EnemyWaveGroups)
			{
				var g = new WaveGroupDefinition
				{
					GroupName = group.Name,
					GroupDescription = group.Description,
					AllowMixedWave = true
				};

				foreach (var wave in group.EnemyWaves)
				{
					if (!wave.Active) continue;
					var prefab = wave.EnemyPrefab;
					if (!prefab) continue;

					g.Segments.Add(BuildSegmentFromEnemyWave(wave, prefab));
				}

				if (g.Segments.Count > 0) groups.Add(g);
			}

			_waveController.Load(groups);
			_mToatalCount = _waveController.TotalWaveCount;
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

			var groups = new List<WaveGroupDefinition>();
			var groupMap = new Dictionary<string, WaveGroupDefinition>(StringComparer.Ordinal);

			foreach (var row in configRows)
			{
				if (!row.Active) continue;

				var prefab = PrefabMapping.GetPrefab(row.EnemyPrefabName);
				if (prefab == null)
				{
					Debug.LogWarning($"[EnemyGenerator] 跳过配置行 '{row.WaveName}'：未找到预制体 '{row.EnemyPrefabName}'");
					continue;
				}

				var groupKey = string.IsNullOrEmpty(row.MixedGroupId) ? row.GroupName : row.MixedGroupId;

				if (!groupMap.TryGetValue(groupKey, out var g))
				{
					g = new WaveGroupDefinition
					{
						GroupName = groupKey,
						GroupDescription = row.GroupDescription,
						AllowMixedWave = row.AllowMixedWave
					};
					groupMap[groupKey] = g;
					groups.Add(g);
				}
				else
				{
					g.AllowMixedWave = g.AllowMixedWave && row.AllowMixedWave;
				}

				var wave = EnemyWave.FromConfigRow(row);
				wave.EnemyPrefab = prefab;
				g.Segments.Add(BuildSegmentFromEnemyWave(wave, prefab));
			}

			_waveController.Load(groups);
			_mToatalCount = _waveController.TotalWaveCount;
			Debug.Log($"[EnemyGenerator] 从CSV成功加载 {_mToatalCount} 个波次组");
		}

		private static WaveSpawnPhase GuessPhase(GameObject prefab)
		{
			if (!prefab) return WaveSpawnPhase.Small;
			return prefab.GetComponent<EnemyMiniBoss>() ? WaveSpawnPhase.Boss : WaveSpawnPhase.Small;
		}

		private static WaveSegmentDefinition BuildSegmentFromEnemyWave(EnemyWave wave, GameObject prefab)
		{
			var phase = GuessPhase(prefab);
			if (!string.IsNullOrEmpty(wave.Phase))
			{
				var p = wave.Phase.Trim().ToLowerInvariant();
				if (p == "boss") phase = WaveSpawnPhase.Boss;
				else if (p == "small") phase = WaveSpawnPhase.Small;
			}

			return new WaveSegmentDefinition
			{
				DisplayName = wave.WaveName,
				Phase = phase,
				Prefab = prefab,
				SpawnIntervalSeconds = wave.GenerateDuration,
				SpawnDurationSeconds = wave.KeepSeconds,
				SpawnCount = wave.SpawnCount,
				MaxWaitAfterSpawnSeconds = wave.MaxWaitAfterSpawnSeconds,
				HPScale = wave.HPScale,
				SpeedScale = wave.SpeedScale,
				DamageScale = wave.DamageScale,
				BaseSpeed = wave.BaseSpeed,
				IsTreasureChest = wave.IsTreasureChest,
				ExpDropRate = wave.ExpDropRate,
				CoinDropRate = wave.CoinDropRate,
				HpDropRate = wave.HpDropRate,
				BombDropRate = wave.BombDropRate
			};
		}

        void Update()
        {
			if (!_isInitialized || _waveController == null) return;

			_waveController.AllowMixedWave = AllowMixedWave;
			_waveController.MixedStrategy = MixedStrategy;
			_waveController.EmptySceneBossTriggerSeconds = EmptySceneBossTriggerSeconds;
			_waveController.EmptySceneCheckIntervalSeconds = EmptySceneCheckIntervalSeconds;

			_waveController.Tick(Time.deltaTime, this);

			WaveCount = _waveController.CurrentWaveIndex;
			CurrentWaveIndex.Value = WaveCount;
			CurrentWaveName.Value = _waveController.CurrentWaveName;
			WaveRemainingTime.Value = _waveController.RemainingSeconds;
        }

		public int SmallAliveCount => SmallEnemyCount.Value;
		public int BossAliveCount => BossEnemyCount.Value;
		public bool HasAnyAliveEnemies => SmallEnemyCount.Value > 0 || BossEnemyCount.Value > 0;

		public bool TrySpawn(in WaveSpawnRequest request)
		{
			if (!request.Prefab) return false;
			if (!Player.Default) return false;

			if (request.Phase != WaveSpawnPhase.Boss)
			{
				var limit = GameSettings.GetMaxSmallEnemyCountForCurrentPlatform();
				if (limit > 0 && SmallEnemyCount.Value >= limit) return false;
			}

			var pos = GetSpawnPositionOutsideCamera();
			var seg = request.Segment;

			request.Prefab.Instantiate()
				.Position(pos)
				.Self(self =>
				{
					var enemy = self.GetComponent<IEnemy>();
					if (enemy == null) return;

					if (seg != null)
					{
						if (seg.BaseSpeed > 0) enemy.SetBaseSpeed(seg.BaseSpeed);
						enemy.SetSpeedScale(seg.SpeedScale);
						enemy.SetHPScale(seg.HPScale);
						enemy.SetDamageScale(seg.DamageScale);
						enemy.SetTreasureChest(seg.IsTreasureChest);
						enemy.SetDropRates(seg.ExpDropRate, seg.CoinDropRate, seg.HpDropRate, seg.BombDropRate);
					}
				})
				.Show();

			return true;
		}

		private Vector2 GetSpawnPositionOutsideCamera()
		{
			var xOry = RandomUtility.Choose(-1, 1);
			var pos = Vector2.zero;
			if (xOry == -1)
			{
				pos.x = RandomUtility.Choose(CameraController.LBTransform.position.x, CameraController.RTTransform.position.x);
				pos.y = UnityEngine.Random.Range(CameraController.LBTransform.position.y, CameraController.RTTransform.position.y);
			}
			else
			{
				pos.x = UnityEngine.Random.Range(CameraController.LBTransform.position.x, CameraController.RTTransform.position.x);
				pos.y = RandomUtility.Choose(CameraController.LBTransform.position.y, CameraController.RTTransform.position.y);
			}
			return pos;
		}
    }
}
