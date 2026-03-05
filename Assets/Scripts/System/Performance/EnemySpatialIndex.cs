using System.Collections.Generic;
using UnityEngine;

namespace VampireSurvivorLike
{
    [DisallowMultipleComponent]
    public sealed class EnemySpatialIndex : MonoBehaviour
    {
        private static EnemySpatialIndex _instance;
        private static readonly List<Transform> QueryBuffer = new List<Transform>(4096);
        private static readonly List<TargetCandidate> Candidates = new List<TargetCandidate>(4096);

        private SpatialHashGrid _grid;
        private float _gridCellSize;
        private float _nextRebuildTime;

        public float CellSize = 2.5f;
        public float RebuildIntervalSeconds = 0.1f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (_instance) return;
            var go = new GameObject("EnemySpatialIndex");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<EnemySpatialIndex>();
        }

        private void Awake()
        {
            if (_instance && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _grid = new SpatialHashGrid(CellSize);
            _gridCellSize = Mathf.Max(0.01f, CellSize);
            _nextRebuildTime = Time.unscaledTime;
        }

        private void Update()
        {
            if (Time.unscaledTime < _nextRebuildTime) return;
            _nextRebuildTime = Time.unscaledTime + Mathf.Max(0.02f, RebuildIntervalSeconds);
            Rebuild();
        }

        private void Rebuild()
        {
            var targetCellSize = Mathf.Max(0.01f, CellSize);
            if (_grid == null || Mathf.Abs(targetCellSize - _gridCellSize) > 0.001f)
            {
                _grid = new SpatialHashGrid(targetCellSize);
                _gridCellSize = targetCellSize;
            }
            else
            {
                _grid.Clear();
            }

            EnemyRegistry.AddAllEnemyTransformsTo(QueryBuffer);
            for (var i = 0; i < QueryBuffer.Count; i++) _grid.Add(QueryBuffer[i]);
            QueryBuffer.Clear();
        }

        public static void GetNearestTargets(Vector2 from, float radius, int count, List<Transform> results)
        {
            results.Clear();
            if (count <= 0) return;

            if (radius <= 0f)
            {
                EnemyRegistry.GetNearestTargets(from, radius, count, results);
                return;
            }

            if (count == 1)
            {
                if (TryGetNearestTarget(from, radius, out var nearest) && nearest)
                {
                    results.Add(nearest);
                }
                return;
            }

            if (!_instance || _instance._grid == null)
            {
                EnemyRegistry.GetNearestTargets(from, radius, count, results);
                return;
            }

            QueryBuffer.Clear();
            _instance._grid.Query(from, radius, QueryBuffer);

            Candidates.Clear();
            var rSqr = radius <= 0f ? float.PositiveInfinity : radius * radius;

            for (var i = 0; i < QueryBuffer.Count; i++)
            {
                var t = QueryBuffer[i];
                if (!t) continue;
                var sqr = ((Vector2)t.position - from).sqrMagnitude;
                if (sqr > rSqr) continue;
                InsertCandidateAscending(new TargetCandidate(t, sqr), count);
            }

            var take = Mathf.Min(count, Candidates.Count);
            for (var i = 0; i < take; i++)
            {
                var t = Candidates[i].Transform;
                if (t) results.Add(t);
            }

            QueryBuffer.Clear();
            Candidates.Clear();
        }

        public static bool TryGetNearestTarget(Vector2 from, float radius, out Transform target)
        {
            target = null;

            if (radius <= 0f)
            {
                return EnemyRegistry.TryGetNearestTarget(from, radius, out target);
            }

            if (!_instance || _instance._grid == null)
            {
                return EnemyRegistry.TryGetNearestTarget(from, radius, out target);
            }

            QueryBuffer.Clear();
            _instance._grid.Query(from, radius, QueryBuffer);
            if (QueryBuffer.Count == 0)
            {
                QueryBuffer.Clear();
                return false;
            }

            var rSqr = radius <= 0f ? float.PositiveInfinity : radius * radius;
            var bestSqr = float.PositiveInfinity;

            for (var i = 0; i < QueryBuffer.Count; i++)
            {
                var t = QueryBuffer[i];
                if (!t) continue;
                var sqr = ((Vector2)t.position - from).sqrMagnitude;
                if (sqr > rSqr || sqr >= bestSqr) continue;
                bestSqr = sqr;
                target = t;
            }

            QueryBuffer.Clear();
            return target;
        }

        private static void InsertCandidateAscending(TargetCandidate candidate, int maxCount)
        {
            if (maxCount <= 0) return;
            if (Candidates.Count >= maxCount && candidate.SqrDistance >= Candidates[Candidates.Count - 1].SqrDistance) return;

            var insertIndex = Candidates.Count;
            while (insertIndex > 0 && candidate.SqrDistance < Candidates[insertIndex - 1].SqrDistance)
            {
                insertIndex--;
            }

            if (Candidates.Count < maxCount)
            {
                Candidates.Add(default);
            }
            else
            {
                Candidates[Candidates.Count - 1] = default;
            }

            for (var i = Candidates.Count - 1; i > insertIndex; i--)
            {
                Candidates[i] = Candidates[i - 1];
            }

            Candidates[insertIndex] = candidate;
        }

        private readonly struct TargetCandidate
        {
            public readonly Transform Transform;
            public readonly float SqrDistance;

            public TargetCandidate(Transform transform, float sqrDistance)
            {
                Transform = transform;
                SqrDistance = sqrDistance;
            }
        }

    }
}

