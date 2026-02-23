using UnityEngine;
using QFramework;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VampireSurvivorLike
{
	// ═══════════════════════════════════════════════════════════════
	// 时间轴驱动波次系统 —— 模仿吸血鬼幸存者
	// 所有频道由游戏时钟驱动，多种敌人可同时活跃，无需清波
	// ═══════════════════════════════════════════════════════════════

	public enum WaveSpawnPhase
	{
		Small = 0,
		Boss = 1
	}

	/// <summary>
	/// 刷怪频道定义：描述一种敌人在某个时间窗口内的刷新规则
	/// </summary>
	[Serializable]
	public class SpawnChannel
	{
		public string ChannelName;
		public string EnemyPrefabName;

		[NonSerialized]
		public GameObject Prefab;

		public WaveSpawnPhase Phase = WaveSpawnPhase.Small;

		[Tooltip("频道激活时间（游戏秒）")]
		public float StartTimeSec = 0f;

		[Tooltip("频道结束时间（游戏秒），-1 表示持续到游戏结束")]
		public float EndTimeSec = 120f;

		[Tooltip("刷新间隔（秒）：每隔多少秒生成 1 个敌人（受难度公式缩放）")]
		public float SpawnIntervalSec = 1f;

		[Tooltip("刷新数量上限（可选）：>0 时总共只刷这么多个，达到后频道关闭")]
		public int SpawnCount = 0;

		[Header("基础数值（受时间难度公式缩放）")]
		public float HPScale = 1f;
		public float SpeedScale = 1f;
		public float DamageScale = 1f;
		public float BaseSpeed = 2f;

		[Header("掉落配置")]
		public bool IsTreasureChest = false;
		public float ExpDropRate = 0.3f;
		public float CoinDropRate = 0.3f;
		public float HpDropRate = 0.1f;
		public float BombDropRate = 0.05f;

		[Header("事件扩展")]
		[Tooltip("edge/swarm/ring，默认 edge")]
		public string SpawnPattern = "edge";
		[Tooltip("每次触发时生成数量")]
		public int BurstCount = 1;
		[Tooltip("swarm/ring 模式半径（世界单位）")]
		public float SpawnRadius = 12f;
	}

	/// <summary>
	/// 频道运行时状态
	/// </summary>
	public sealed class SpawnChannelRuntime
	{
		public SpawnChannel Definition;
		public float SpawnTimer;
		public int SpawnedCount;

		public bool IsActive(float gameTime)
		{
			if (gameTime < Definition.StartTimeSec) return false;
			if (Definition.EndTimeSec >= 0 && gameTime >= Definition.EndTimeSec) return false;
			if (Definition.SpawnCount > 0 && SpawnedCount >= Definition.SpawnCount) return false;
			return true;
		}

		public bool IsExhausted(float gameTime)
		{
			if (Definition.SpawnCount > 0 && SpawnedCount >= Definition.SpawnCount) return true;
			if (Definition.EndTimeSec >= 0 && gameTime >= Definition.EndTimeSec) return true;
			return false;
		}
	}

	/// <summary>
	/// 刷怪请求结构体（传递给 IWaveWorld.TrySpawn）
	/// </summary>
	public readonly struct WaveSpawnRequest
	{
		public readonly WaveSpawnPhase Phase;
		public readonly GameObject Prefab;
		public readonly SpawnChannel Channel;
		/// <summary>经过难度公式缩放后的实际属性</summary>
		public readonly float EffectiveHPScale;
		public readonly float EffectiveSpeedScale;
		public readonly float EffectiveDamageScale;

		public WaveSpawnRequest(WaveSpawnPhase phase, GameObject prefab, SpawnChannel channel,
			float effectiveHP, float effectiveSpeed, float effectiveDamage)
		{
			Phase = phase;
			Prefab = prefab;
			Channel = channel;
			EffectiveHPScale = effectiveHP;
			EffectiveSpeedScale = effectiveSpeed;
			EffectiveDamageScale = effectiveDamage;
		}
	}

	/// <summary>
	/// 世界接口：EnemyGenerator 实现此接口，TimelineController 通过它执行刷怪
	/// </summary>
	public interface IWaveWorld
	{
		int SmallAliveCount { get; }
		int BossAliveCount { get; }
		bool HasAnyAliveEnemies { get; }
		bool TrySpawn(in WaveSpawnRequest request);
	}

	/// <summary>
	/// 时间轴控制器：纯 C# 类，无 MonoBehaviour 依赖。
	/// 根据游戏时钟同时驱动多个刷怪频道，应用难度递增公式。
	/// </summary>
	public sealed class TimelineController
	{
		private List<SpawnChannelRuntime> _channels = new List<SpawnChannelRuntime>();

		/// <summary>当前活跃的频道名称（供 UI 显示）</summary>
		public string ActiveChannelNames { get; private set; } = string.Empty;

		/// <summary>活跃频道数量</summary>
		public int ActiveChannelCount { get; private set; }

		/// <summary>总频道数</summary>
		public int TotalChannelCount => _channels.Count;

		/// <summary>到游戏结束的剩余秒数</summary>
		public float RemainingSeconds { get; private set; }

		/// <summary>当前游戏分钟数（整数）</summary>
		public int CurrentMinute { get; private set; }

		/// <summary>所有频道是否都已耗尽且游戏时间到</summary>
		public bool IsTimelineComplete(float gameTime)
		{
			if (gameTime < Config.MaxGameSeconds) return false;
			return _channels.All(ch => ch.IsExhausted(gameTime));
		}

		public void Load(IReadOnlyList<SpawnChannel> channels)
		{
			_channels.Clear();
			ActiveChannelNames = string.Empty;
			ActiveChannelCount = 0;

			if (channels == null) return;

			foreach (var ch in channels)
			{
				if (ch == null || ch.Prefab == null) continue;
				_channels.Add(new SpawnChannelRuntime { Definition = ch });
			}

			Debug.Log($"[TimelineController] 加载 {_channels.Count} 个刷怪频道");
		}

		public void Tick(float gameTime, float deltaTime, IWaveWorld world)
		{
			if (world == null || _channels.Count == 0) return;

			CurrentMinute = Mathf.FloorToInt(gameTime / 60f);
			RemainingSeconds = Mathf.Max(0f, Config.MaxGameSeconds - gameTime);

			// 难度倍率（基于游戏分钟数）
			var minutes = gameTime / 60f;

			var activeNames = new List<string>();

			foreach (var ch in _channels)
			{
				if (!ch.IsActive(gameTime)) continue;

				activeNames.Add(ch.Definition.ChannelName);

				var isBossPhase = ch.Definition.Phase == WaveSpawnPhase.Boss;
				var hpGrowth = isBossPhase ? Config.BossHPGrowthPerMinute : Config.HPGrowthPerMinute;
				var speedGrowth = isBossPhase ? Config.BossSpeedGrowthPerMinute : Config.SpeedGrowthPerMinute;
				var damageGrowth = isBossPhase ? Config.BossDamageGrowthPerMinute : Config.DamageGrowthPerMinute;
				var spawnGrowth = isBossPhase ? Config.BossSpawnRateGrowthPerMinute : Config.SpawnRateGrowthPerMinute;
				var hpMul = 1f + hpGrowth * minutes;
				var speedMul = 1f + speedGrowth * minutes;
				var damageMul = 1f + damageGrowth * minutes;
				var spawnRateMul = 1f + spawnGrowth * minutes;
				if (!isBossPhase && gameTime < Config.EarlyGameSpawnBoostDurationSeconds)
				{
					spawnRateMul *= Config.EarlyGameSpawnRateMultiplier;
				}

				// 计算经过难度缩放后的刷新间隔
				var effectiveInterval = ch.Definition.SpawnIntervalSec / spawnRateMul;
				effectiveInterval = Mathf.Max(0.05f, effectiveInterval); // 最快 20/秒

				ch.SpawnTimer += deltaTime;

				if (ch.SpawnTimer >= effectiveInterval)
				{
					ch.SpawnTimer -= effectiveInterval;

					// 计算缩放后属性
					var effHP = ch.Definition.HPScale * hpMul;
					var effSpeed = ch.Definition.SpeedScale * speedMul;
					var effDamage = ch.Definition.DamageScale * damageMul;

					var request = new WaveSpawnRequest(
						ch.Definition.Phase,
						ch.Definition.Prefab,
						ch.Definition,
						effHP, effSpeed, effDamage
					);

					if (world.TrySpawn(request))
					{
						ch.SpawnedCount++;
					}
				}
			}

			ActiveChannelCount = activeNames.Count;
			ActiveChannelNames = activeNames.Count > 0 ? string.Join(" + ", activeNames) : string.Empty;
		}
	}

	// ═══════════════════════════════════════════════════════════════
	// EnemyGenerator MonoBehaviour
	// ═══════════════════════════════════════════════════════════════

	public partial class EnemyGenerator : ViewController, IWaveWorld
	{
		[SerializeField]
		[Tooltip("敌人预制体映射表，用于将CSV中的敌人名称映射到实际预制体")]
		public EnemyPrefabMapping PrefabMapping;

		public static BindableProperty<int> EnemyCount = new BindableProperty<int>(0);
		public static BindableProperty<int> SmallEnemyCount = new BindableProperty<int>(0);
		public static BindableProperty<int> BossEnemyCount = new BindableProperty<int>(0);

		/// <summary>当前游戏时间对应的分钟数</summary>
		public static BindableProperty<int> CurrentMinute = new BindableProperty<int>(0);
		/// <summary>当前活跃的刷怪频道名称</summary>
		public static BindableProperty<string> ActiveChannelNames = new BindableProperty<string>("");
		/// <summary>到游戏结束的剩余秒数</summary>
		public static BindableProperty<float> GameRemainingTime = new BindableProperty<float>(0);
		/// <summary>活跃频道数量</summary>
		public static BindableProperty<int> ActiveChannelCount = new BindableProperty<int>(0);

		// ── 向后兼容旧 UI 引用（映射到新语义）──
		/// <summary>[兼容] 当前分钟数（旧 CurrentWaveIndex）</summary>
		public static BindableProperty<int> CurrentWaveIndex => CurrentMinute;
		/// <summary>[兼容] 总分钟数 30（旧 TotalWaveCount）</summary>
		public static BindableProperty<int> TotalWaveCount = new BindableProperty<int>(30);
		/// <summary>[兼容] 当前活跃频道名（旧 CurrentWaveName）</summary>
		public static BindableProperty<string> CurrentWaveName => ActiveChannelNames;
		/// <summary>[兼容] 剩余时间（旧 WaveRemainingTime）</summary>
		public static BindableProperty<float> WaveRemainingTime => GameRemainingTime;

		public bool IsInitialized => _isInitialized;
		/// <summary>游戏时间是否已到达上限（30分钟）</summary>
		public bool IsGameTimeUp => _isInitialized && Global.CurrentSeconds.Value >= Config.MaxGameSeconds;
		/// <summary>[兼容] 所有波次是否完成 → 游戏时间到且所有频道耗尽</summary>
		public bool IsAllWavesFinished => _isInitialized && _timeline != null && _timeline.IsTimelineComplete(Global.CurrentSeconds.Value);

		private bool _isInitialized = false;
		private TimelineController _timeline;
		private bool _reaperSpawned = false;

		IEnumerator Start()
		{
			_timeline = new TimelineController();

			if (PrefabMapping != null)
			{
				yield return LoadFromCSVAsync();
			}
			else
			{
				Debug.LogError("[EnemyGenerator] PrefabMapping 未设置！无法加载频道配置。");
			}

			_isInitialized = true;
			TotalWaveCount.Value = 30; // 30 分钟
		}

		/// <summary>
		/// 异步从CSV加载时间轴频道配置
		/// </summary>
		private IEnumerator LoadFromCSVAsync()
		{
			List<SpawnChannelConfigRow> configRows = null;

			yield return SpawnChannelConfigLoader.LoadAsync(rows => configRows = rows);

			if (configRows == null || configRows.Count == 0)
			{
				Debug.LogWarning("[EnemyGenerator] CSV频道配置为空！");
				yield break;
			}

			var channels = new List<SpawnChannel>();

			foreach (var row in configRows)
			{
				if (!row.Active) continue;

				var prefab = PrefabMapping.GetPrefab(row.EnemyPrefabName);
				if (prefab == null)
				{
					Debug.LogWarning($"[EnemyGenerator] 跳过频道 '{row.ChannelName}'：未找到预制体 '{row.EnemyPrefabName}'");
					continue;
				}

				var phase = GuessPhase(prefab);
				if (!string.IsNullOrEmpty(row.Phase))
				{
					var p = row.Phase.Trim().ToLowerInvariant();
					if (p == "boss") phase = WaveSpawnPhase.Boss;
					else if (p == "small") phase = WaveSpawnPhase.Small;
				}

				channels.Add(new SpawnChannel
				{
					ChannelName = row.ChannelName,
					EnemyPrefabName = row.EnemyPrefabName,
					Prefab = prefab,
					Phase = phase,
					StartTimeSec = row.StartTimeSec,
					EndTimeSec = row.EndTimeSec,
					SpawnIntervalSec = row.SpawnIntervalSec,
					SpawnCount = row.SpawnCount,
					HPScale = row.HPScale,
					SpeedScale = row.SpeedScale,
					DamageScale = row.DamageScale,
					BaseSpeed = row.BaseSpeed,
					IsTreasureChest = row.IsTreasureChest,
					ExpDropRate = row.ExpDropRate,
					CoinDropRate = row.CoinDropRate,
					HpDropRate = row.HpDropRate,
					BombDropRate = row.BombDropRate,
					SpawnPattern = row.SpawnPattern,
					BurstCount = row.BurstCount,
					SpawnRadius = row.SpawnRadius
				});
			}

			_timeline.Load(channels);
			Debug.Log($"[EnemyGenerator] 从CSV成功加载 {channels.Count} 个刷怪频道");
		}

		private static WaveSpawnPhase GuessPhase(GameObject prefab)
		{
			if (!prefab) return WaveSpawnPhase.Small;
			return prefab.GetComponent<EnemyMiniBoss>() ? WaveSpawnPhase.Boss : WaveSpawnPhase.Small;
		}

		void Update()
		{
			if (!_isInitialized || _timeline == null) return;

			var gameTime = Global.CurrentSeconds.Value;

			_timeline.Tick(gameTime, Time.deltaTime, this);

			CurrentMinute.Value = _timeline.CurrentMinute;
			ActiveChannelNames.Value = _timeline.ActiveChannelNames;
			GameRemainingTime.Value = _timeline.RemainingSeconds;
			ActiveChannelCount.Value = _timeline.ActiveChannelCount;

			// 死神生成检测
			if (!_reaperSpawned && gameTime >= Config.ReaperSpawnTimeSeconds)
			{
				SpawnReaper();
			}
		}

		private void SpawnReaper()
		{
			_reaperSpawned = true;

			if (PrefabMapping == null) return;
			var reaperPrefab = PrefabMapping.GetPrefab(Config.ReaperPrefabName);
			if (reaperPrefab == null)
			{
				Debug.LogWarning($"[EnemyGenerator] 死神预制体 '{Config.ReaperPrefabName}' 未在 PrefabMapping 中注册，跳过生成");
				return;
			}

			var pos = GetSpawnPositionOutsideCamera();
			reaperPrefab.Instantiate()
				.Position(pos)
				.Self(self =>
				{
					var enemy = self.GetComponent<IEnemy>();
					if (enemy == null) return;
					enemy.SetBaseSpeed(5f);
					enemy.SetSpeedScale(2f);
					enemy.SetHPScale(99999f);
					enemy.SetDamageScale(999f);
					enemy.SetTreasureChest(false);
					enemy.SetDropRates(0, 0, 0, 0);
				})
				.Show();

			Debug.Log("[EnemyGenerator] 死神已降临！");
		}

		// ── IWaveWorld 实现 ──

		public int SmallAliveCount => SmallEnemyCount.Value;
		public int BossAliveCount => BossEnemyCount.Value;
		public bool HasAnyAliveEnemies => SmallEnemyCount.Value > 0 || BossEnemyCount.Value > 0;

		public bool TrySpawn(in WaveSpawnRequest request)
		{
			if (!request.Prefab) return false;
			if (!Player.Default) return false;
			var ch = request.Channel;
			var burstCount = ch != null ? Mathf.Max(1, ch.BurstCount) : 1;
			var spawnedAny = false;

			for (var i = 0; i < burstCount; i++)
			{
				if (request.Phase != WaveSpawnPhase.Boss)
				{
					var limit = GameSettings.GetMaxSmallEnemyCountForCurrentPlatform();
					if (limit > 0 && SmallEnemyCount.Value >= limit) break;
				}

				var pos = ResolveSpawnPosition(request, i, burstCount);
				if (TrySpawnSingle(request, pos))
				{
					spawnedAny = true;
				}
			}

			return spawnedAny;
		}

		private bool TrySpawnSingle(in WaveSpawnRequest request, Vector2 pos)
		{
			var ch = request.Channel;
			var effectiveSpeed = request.EffectiveSpeedScale;
			var effectiveHP = request.EffectiveHPScale;
			var effectiveDamage = request.EffectiveDamageScale;

			request.Prefab.Instantiate()
				.Position(pos)
				.Self(self =>
				{
					var enemy = self.GetComponent<IEnemy>();
					if (enemy == null) return;

					if (ch != null)
					{
						if (ch.BaseSpeed > 0) enemy.SetBaseSpeed(ch.BaseSpeed);
						enemy.SetSpeedScale(effectiveSpeed);
						enemy.SetHPScale(effectiveHP);
						enemy.SetDamageScale(effectiveDamage);
						enemy.SetTreasureChest(ch.IsTreasureChest);
						enemy.SetDropRates(ch.ExpDropRate, ch.CoinDropRate, ch.HpDropRate, ch.BombDropRate);
					}
				})
				.Show();

			return true;
		}

		private Vector2 ResolveSpawnPosition(in WaveSpawnRequest request, int burstIndex, int burstCount)
		{
			var channel = request.Channel;
			var pattern = channel != null && !string.IsNullOrWhiteSpace(channel.SpawnPattern)
				? channel.SpawnPattern.Trim().ToLowerInvariant()
				: "edge";
			var radius = channel != null ? Mathf.Max(1f, channel.SpawnRadius) : 12f;

			switch (pattern)
			{
				case "swarm":
				{
					var center = (Vector2)Player.Default.transform.position;
					return center + UnityEngine.Random.insideUnitCircle * radius;
				}
				case "ring":
				{
					var center = (Vector2)Player.Default.transform.position;
					var count = Mathf.Max(1, burstCount);
					var angle = (Mathf.PI * 2f) * (burstIndex / (float)count);
					return center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
				}
				case "edge":
				default:
					return GetSpawnPositionOutsideCamera();
			}
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
