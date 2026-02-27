using System.Collections.Generic;
using UnityEngine;

namespace VampireSurvivorLike
{
    public static class EnemyRegistry
    {
        private static readonly HashSet<Enemy> SmallEnemiesSet = new HashSet<Enemy>();
        private static readonly HashSet<EnemyMiniBoss> BossEnemiesSet = new HashSet<EnemyMiniBoss>();

        private static readonly List<TargetCandidate> Candidates = new List<TargetCandidate>(4096);

        public static int SmallCount => SmallEnemiesSet.Count;
        public static int BossCount => BossEnemiesSet.Count;

        public static void Register(Enemy enemy)
        {
            if (!enemy) return;
            SmallEnemiesSet.Add(enemy);
        }

        public static void Unregister(Enemy enemy)
        {
            if (!enemy) return;
            SmallEnemiesSet.Remove(enemy);
        }

        public static void Register(EnemyMiniBoss boss)
        {
            if (!boss) return;
            BossEnemiesSet.Add(boss);
        }

        public static void Unregister(EnemyMiniBoss boss)
        {
            if (!boss) return;
            BossEnemiesSet.Remove(boss);
        }

        public static void Clear()
        {
            SmallEnemiesSet.Clear();
            BossEnemiesSet.Clear();
            Candidates.Clear();
        }

        public static void AddAllEnemyTransformsTo(List<Transform> results)
        {
            results.Clear();

            foreach (var e in SmallEnemiesSet)
            {
                if (!e) continue;
                results.Add(e.transform);
            }

            foreach (var b in BossEnemiesSet)
            {
                if (!b) continue;
                results.Add(b.transform);
            }
        }

        public static void AddAllSmallEnemiesTo(List<Enemy> results)
        {
            results.Clear();
            foreach (var e in SmallEnemiesSet)
            {
                if (!e) continue;
                results.Add(e);
            }
        }

        public static void GetNearestTargets(Vector2 from, int count, List<Transform> results)
        {
            results.Clear();
            if (count <= 0) return;

            Candidates.Clear();

            foreach (var e in SmallEnemiesSet)
            {
                if (!e) continue;
                var t = e.transform;
                var sqr = ((Vector2)t.position - from).sqrMagnitude;
                Candidates.Add(new TargetCandidate(t, sqr));
            }

            foreach (var b in BossEnemiesSet)
            {
                if (!b) continue;
                var t = b.transform;
                var sqr = ((Vector2)t.position - from).sqrMagnitude;
                Candidates.Add(new TargetCandidate(t, sqr));
            }

            Candidates.Sort(TargetCandidateComparer.Instance);

            var take = Mathf.Min(count, Candidates.Count);
            for (var i = 0; i < take; i++)
            {
                var t = Candidates[i].Transform;
                if (t) results.Add(t);
            }
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
