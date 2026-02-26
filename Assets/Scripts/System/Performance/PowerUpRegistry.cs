using System.Collections.Generic;
using UnityEngine;

namespace VampireSurvivorLike
{
    /// <summary>
    /// 掉落物静态注册表，替代 FindObjectOfType / FindObjectsByType 全场扫描。
    /// 各掉落物在 OnEnable 中注册，OnDisable 中反注册。
    /// </summary>
    public static class PowerUpRegistry
    {
        // 用于 Global.GeneratePowerUpWithRates 中快速判断是否已存在
        public static int ActiveRecoverHPCount;
        public static int ActiveWineCount;
        public static int ActiveLemonBuffCount;
        public static int ActiveCherryCount;
        public static int ActiveBombCount;
        public static int ActiveGetAllExpCount;

        // 用于 GetAllExp 收集全场经验球/金币（替代 FindObjectsByType）
        private static readonly HashSet<Exp> ExpSet = new HashSet<Exp>();
        private static readonly HashSet<Coin> CoinSet = new HashSet<Coin>();
        private static readonly List<ExpDistanceCandidate> ExpCandidates = new List<ExpDistanceCandidate>(512);
        private static readonly List<CoinDistanceCandidate> CoinCandidates = new List<CoinDistanceCandidate>(512);

        public static int ExpCount => ExpSet.Count;
        public static int CoinCount => CoinSet.Count;

        public static void RegisterExp(Exp exp) { if (exp) ExpSet.Add(exp); }
        public static void UnregisterExp(Exp exp) { ExpSet.Remove(exp); }

        public static void RegisterCoin(Coin coin) { if (coin) CoinSet.Add(coin); }
        public static void UnregisterCoin(Coin coin) { CoinSet.Remove(coin); }

        /// <summary>
        /// 收集所有活跃的 Exp 和 Coin 到目标列表（零分配）
        /// </summary>
        public static void CollectAllExpAndCoins(List<PowerUp> results)
        {
            results.Clear();
            foreach (var e in ExpSet)
            {
                if (e && e.gameObject.activeInHierarchy) results.Add(e);
            }
            foreach (var c in CoinSet)
            {
                if (c && c.gameObject.activeInHierarchy) results.Add(c);
            }
        }

        public static void GetFarthestExps(Vector3 playerPos, int count, List<Exp> results)
        {
            results.Clear();
            if (count <= 0 || ExpSet.Count == 0) return;

            ExpCandidates.Clear();
            foreach (var exp in ExpSet)
            {
                if (!exp || !exp.gameObject.activeInHierarchy || exp.FlyingToPalyer) continue;
                var sqrDistance = ((Vector2)exp.transform.position - (Vector2)playerPos).sqrMagnitude;
                ExpCandidates.Add(new ExpDistanceCandidate(exp, sqrDistance));
            }

            if (ExpCandidates.Count == 0) return;

            ExpCandidates.Sort(ExpDistanceCandidateComparer.Instance);

            var take = Mathf.Min(count, ExpCandidates.Count);
            for (var i = 0; i < take; i++)
            {
                var exp = ExpCandidates[i].Exp;
                if (exp) results.Add(exp);
            }
        }

        public static void GetFarthestCoins(Vector3 playerPos, int count, List<Coin> results)
        {
            results.Clear();
            if (count <= 0 || CoinSet.Count == 0) return;

            CoinCandidates.Clear();
            foreach (var coin in CoinSet)
            {
                if (!coin || !coin.gameObject.activeInHierarchy || coin.FlyingToPalyer) continue;
                var sqrDistance = ((Vector2)coin.transform.position - (Vector2)playerPos).sqrMagnitude;
                CoinCandidates.Add(new CoinDistanceCandidate(coin, sqrDistance));
            }

            if (CoinCandidates.Count == 0) return;

            CoinCandidates.Sort(CoinDistanceCandidateComparer.Instance);

            var take = Mathf.Min(count, CoinCandidates.Count);
            for (var i = 0; i < take; i++)
            {
                var coin = CoinCandidates[i].Coin;
                if (coin) results.Add(coin);
            }
        }

        public static void Clear()
        {
            ActiveRecoverHPCount = 0;
            ActiveWineCount = 0;
            ActiveLemonBuffCount = 0;
            ActiveCherryCount = 0;
            ActiveBombCount = 0;
            ActiveGetAllExpCount = 0;
            ExpSet.Clear();
            CoinSet.Clear();
            ExpCandidates.Clear();
            CoinCandidates.Clear();
        }

        private readonly struct ExpDistanceCandidate
        {
            public readonly Exp Exp;
            public readonly float SqrDistance;

            public ExpDistanceCandidate(Exp exp, float sqrDistance)
            {
                Exp = exp;
                SqrDistance = sqrDistance;
            }
        }

        private sealed class ExpDistanceCandidateComparer : IComparer<ExpDistanceCandidate>
        {
            public static readonly ExpDistanceCandidateComparer Instance = new ExpDistanceCandidateComparer();

            public int Compare(ExpDistanceCandidate x, ExpDistanceCandidate y)
            {
                return y.SqrDistance.CompareTo(x.SqrDistance);
            }
        }

        private readonly struct CoinDistanceCandidate
        {
            public readonly Coin Coin;
            public readonly float SqrDistance;

            public CoinDistanceCandidate(Coin coin, float sqrDistance)
            {
                Coin = coin;
                SqrDistance = sqrDistance;
            }
        }

        private sealed class CoinDistanceCandidateComparer : IComparer<CoinDistanceCandidate>
        {
            public static readonly CoinDistanceCandidateComparer Instance = new CoinDistanceCandidateComparer();

            public int Compare(CoinDistanceCandidate x, CoinDistanceCandidate y)
            {
                return y.SqrDistance.CompareTo(x.SqrDistance);
            }
        }
    }
}
