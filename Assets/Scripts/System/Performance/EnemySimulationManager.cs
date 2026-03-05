using System.Collections.Generic;
using UnityEngine;

namespace VampireSurvivorLike
{
    [DisallowMultipleComponent]
    public sealed class EnemySimulationManager : MonoBehaviour
    {
        private static EnemySimulationManager _instance;

        public static bool Enabled = true;

        public static bool EnableDistanceTiering = true;
        public static float NearDistance = 10f;
        public static float MidDistance = 20f;
        public static float MidIntervalSeconds = 0.1f;
        public static float FarIntervalSeconds = 0.25f;

        public static bool DisablePhysicsForFarEnemies = true;
        public static float PhysicsDisableDistance = 28f;

        private readonly HashSet<Enemy> _enemies = new HashSet<Enemy>();
        private readonly List<Enemy> _iteration = new List<Enemy>(8192);
        private readonly Dictionary<Enemy, float> _nextMoveTime = new Dictionary<Enemy, float>(8192);
        private readonly List<Enemy> _nextMoveTimeCleanup = new List<Enemy>(256);
        private Camera _cachedMainCamera;
        private float _nextCameraRefreshTime;
        private float _nextCompactionTime;
        private bool _iterationDirty = true;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (_instance) return;
            var go = new GameObject("EnemySimulationManager");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<EnemySimulationManager>();
        }

        private void Awake()
        {
            if (!Application.isMobilePlatform) return;

            NearDistance = 8f;
            MidDistance = 16f;
            MidIntervalSeconds = 0.16f;
            FarIntervalSeconds = 0.35f;
            PhysicsDisableDistance = 22f;
        }

        public static void Register(Enemy enemy)
        {
            if (!Enabled) return;
            if (!_instance || !enemy) return;
            if (_instance._enemies.Add(enemy))
            {
                _instance._iterationDirty = true;
            }
        }

        public static void Unregister(Enemy enemy)
        {
            if (!_instance || !enemy) return;
            if (_instance._enemies.Remove(enemy))
            {
                _instance._iterationDirty = true;
            }
            if (_instance._nextMoveTime.Remove(enemy))
            {
                _instance._iterationDirty = true;
            }
        }

        private void FixedUpdate()
        {
            if (!Enabled) return;
            if (!Player.Default) return;

            if (_iterationDirty || Time.unscaledTime >= _nextCompactionTime)
            {
                RebuildIterationCache();
            }

            var playerPos = (Vector2)Player.Default.transform.position;
            var now = Time.fixedTime;
            var cam = GetMainCamera();
            var camPos = cam ? (Vector2)cam.transform.position : Vector2.zero;
            var halfH = cam && cam.orthographic ? cam.orthographicSize : 0f;
            var halfW = cam && cam.orthographic ? cam.orthographicSize * cam.aspect : 0f;

            for (var i = 0; i < _iteration.Count; i++)
            {
                var e = _iteration[i];
                if (!e || e.IsDeadOrIgnoringHurt) continue;
                if (!e.SelfRigidbody2D) continue;

                var pos = (Vector2)e.transform.position;
                var delta = playerPos - pos;
                var distSqr = delta.sqrMagnitude;
                var dist = distSqr > 0.0001f ? Mathf.Sqrt(distSqr) : 0f;
                var dir = distSqr > 0.0001f ? (delta / dist) : Vector2.zero;

                if (DisablePhysicsForFarEnemies && dist > PhysicsDisableDistance)
                {
                    if (e.SelfRigidbody2D.simulated) e.SelfRigidbody2D.simulated = false;
                    if (e.HitBox && e.HitBox.enabled) e.HitBox.enabled = false;
                    e.transform.position = (Vector3)(pos + dir * (e.MovementSpeed * Time.fixedDeltaTime));
                    continue;
                }

                if (!e.SelfRigidbody2D.simulated) e.SelfRigidbody2D.simulated = true;
                if (e.HitBox && !e.HitBox.enabled) e.HitBox.enabled = true;

                if (EnableDistanceTiering)
                {
                    if (_nextMoveTime.TryGetValue(e, out var nextAt) && now < nextAt) continue;
                    var interval = 0f;
                    if (dist > MidDistance) interval = FarIntervalSeconds;
                    else if (dist > NearDistance) interval = MidIntervalSeconds;
                    if (interval > 0f) _nextMoveTime[e] = now + interval;
                }

                e.SelfRigidbody2D.velocity = dir * e.MovementSpeed;

                if (dist < 3f)
                {
                    TryApplyMeleeDamageFallback(e, pos, playerPos);
                }

                if (cam && cam.orthographic)
                {
                    var inView = Mathf.Abs(pos.x - camPos.x) <= halfW + 1f && Mathf.Abs(pos.y - camPos.y) <= halfH + 1f;
                    var enableSprite = inView && !PcInstancedEnemyRenderer.Enabled;
                    var enableAnimation = inView && dist <= MidDistance;
                    var enableShadow = inView && dist <= NearDistance;
                    e.ApplyLod(enableAnimation, enableShadow, enableSprite);
                }
            }
        }

        private void RebuildIterationCache()
        {
            _iteration.Clear();

            foreach (var e in _enemies)
            {
                if (!e) continue;
                _iteration.Add(e);
            }

            _nextMoveTimeCleanup.Clear();
            foreach (var kv in _nextMoveTime)
            {
                if (!kv.Key || !_enemies.Contains(kv.Key))
                {
                    _nextMoveTimeCleanup.Add(kv.Key);
                }
            }

            for (var i = 0; i < _nextMoveTimeCleanup.Count; i++)
            {
                _nextMoveTime.Remove(_nextMoveTimeCleanup[i]);
            }
            _nextMoveTimeCleanup.Clear();

            _iterationDirty = false;
            _nextCompactionTime = Time.unscaledTime + 0.5f;
        }

        private Camera GetMainCamera()
        {
            if (_cachedMainCamera && _cachedMainCamera.isActiveAndEnabled) return _cachedMainCamera;
            if (Time.unscaledTime < _nextCameraRefreshTime) return _cachedMainCamera;

            _nextCameraRefreshTime = Time.unscaledTime + 0.5f;
            _cachedMainCamera = Camera.main;
            return _cachedMainCamera;
        }

        private static void TryApplyMeleeDamageFallback(Enemy enemy, Vector2 enemyPos, Vector2 playerPos)
        {
            if (!Player.Default) return;
            if (Player.Default.IsGameOver) return;

            var playerCollider = Player.Default.HurtBox;
            var enemyCollider = enemy.HitBox;
            if (!playerCollider || !enemyCollider) return;

            var pr = playerCollider.radius * Mathf.Max(Player.Default.transform.lossyScale.x, Player.Default.transform.lossyScale.y);
            var er = enemyCollider.radius * Mathf.Max(enemy.transform.lossyScale.x, enemy.transform.lossyScale.y);
            var r = pr + er;
            if (((playerPos - enemyPos).sqrMagnitude) <= r * r)
            {
                Player.Default.ApplyDamage(1, string.Empty, "EnemyMelee");
            }
        }
    }
}
