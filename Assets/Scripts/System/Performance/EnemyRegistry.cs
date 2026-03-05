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

        public static void AddAllBossEnemiesTo(List<EnemyMiniBoss> results)
        {
            results.Clear();
            foreach (var b in BossEnemiesSet)
            {
                if (!b) continue;
                results.Add(b);
            }
        }

        public static bool TryGetNearestTarget(Vector2 from, float radius, out Transform target)
        {
            target = null;

            var maxSqr = radius <= 0f ? float.PositiveInfinity : radius * radius;
            var bestSqr = float.PositiveInfinity;

            foreach (var e in SmallEnemiesSet)
            {
                if (!e) continue;
                var t = e.transform;
                var sqr = ((Vector2)t.position - from).sqrMagnitude;
                if (sqr > maxSqr || sqr >= bestSqr) continue;
                bestSqr = sqr;
                target = t;
            }

            foreach (var b in BossEnemiesSet)
            {
                if (!b) continue;
                var t = b.transform;
                var sqr = ((Vector2)t.position - from).sqrMagnitude;
                if (sqr > maxSqr || sqr >= bestSqr) continue;
                bestSqr = sqr;
                target = t;
            }

            return target;
        }

        public static void GetNearestTargets(Vector2 from, int count, List<Transform> results)
        {
            GetNearestTargets(from, 0f, count, results);
        }

        public static void GetNearestTargets(Vector2 from, float radius, int count, List<Transform> results)
        {
            results.Clear();
            if (count <= 0) return;

            if (count == 1)
            {
                if (TryGetNearestTarget(from, radius, out var nearest) && nearest)
                {
                    results.Add(nearest);
                }
                return;
            }

            Candidates.Clear();
            var maxSqr = radius <= 0f ? float.PositiveInfinity : radius * radius;

            foreach (var e in SmallEnemiesSet)
            {
                if (!e) continue;
                var t = e.transform;
                var sqr = ((Vector2)t.position - from).sqrMagnitude;
                if (sqr > maxSqr) continue;
                InsertCandidateAscending(new TargetCandidate(t, sqr), count);
            }

            foreach (var b in BossEnemiesSet)
            {
                if (!b) continue;
                var t = b.transform;
                var sqr = ((Vector2)t.position - from).sqrMagnitude;
                if (sqr > maxSqr) continue;
                InsertCandidateAscending(new TargetCandidate(t, sqr), count);
            }

            var take = Mathf.Min(count, Candidates.Count);
            for (var i = 0; i < take; i++)
            {
                var t = Candidates[i].Transform;
                if (t) results.Add(t);
            }
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
