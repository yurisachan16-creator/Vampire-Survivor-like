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
            if (_grid == null || CellSize <= 0f || Mathf.Abs(CellSize - GetGridCellSize()) > 0.001f)
            {
                _grid = new SpatialHashGrid(CellSize);
            }
            else
            {
                _grid.Clear();
            }

            EnemyRegistry.AddAllEnemyTransformsTo(QueryBuffer);
            for (var i = 0; i < QueryBuffer.Count; i++) _grid.Add(QueryBuffer[i]);
            QueryBuffer.Clear();
        }

        private float GetGridCellSize()
        {
            return CellSize;
        }

        public static void GetNearestTargets(Vector2 from, float radius, int count, List<Transform> results)
        {
            results.Clear();
            if (count <= 0) return;
            if (!_instance || _instance._grid == null)
            {
                EnemyRegistry.GetNearestTargets(from, count, results);
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
                Candidates.Add(new TargetCandidate(t, sqr));
            }

            Candidates.Sort(TargetCandidateComparer.Instance);

            var take = Mathf.Min(count, Candidates.Count);
            for (var i = 0; i < take; i++)
            {
                var t = Candidates[i].Transform;
                if (t) results.Add(t);
            }

            QueryBuffer.Clear();
            Candidates.Clear();
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

        private sealed class TargetCandidateComparer : IComparer<TargetCandidate>
        {
            public static readonly TargetCandidateComparer Instance = new TargetCandidateComparer();

            public int Compare(TargetCandidate x, TargetCandidate y)
            {
                return x.SqrDistance.CompareTo(y.SqrDistance);
            }
        }
    }
}

