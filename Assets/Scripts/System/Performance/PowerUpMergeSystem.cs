using System.Collections.Generic;
using UnityEngine;

namespace VampireSurvivorLike
{
    [DisallowMultipleComponent]
    public sealed class PowerUpMergeSystem : MonoBehaviour
    {
        private const int ExpMinMergeBatchCount = 50;

        private static PowerUpMergeSystem _instance;
        public static int CoinMergeTriggerCount { get; private set; }

        private static readonly List<Exp> ExpCandidates = new List<Exp>(1024);
        private static readonly List<Exp> ExpMergeBatch = new List<Exp>(1024);
        private static readonly List<Coin> CoinCandidates = new List<Coin>(1024);
        private static readonly List<Coin> CoinMergeBatch = new List<Coin>(1024);

        private float _nextExpCheckTime;
        private float _nextCoinCheckTime;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (_instance) return;
            var go = new GameObject("PowerUpMergeSystem");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<PowerUpMergeSystem>();
            CoinMergeTriggerCount = 0;
        }

        public static void ResetStats()
        {
            CoinMergeTriggerCount = 0;
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        private void Update()
        {
            if (Global.IsGameOver.Value) return;
            if (!Player.Default) return;

            var now = Time.unscaledTime;
            var playerPos = Player.Default.transform.position;
            if (now >= _nextExpCheckTime)
            {
                _nextExpCheckTime = now + Config.ExpMergeCheckInterval;
                TryMergeExpNow(playerPos);
            }

            if (now >= _nextCoinCheckTime)
            {
                _nextCoinCheckTime = now + Config.CoinMergeCheckInterval;
                TryMergeCoinNow(playerPos);
            }
        }

        public static bool TryMergeExpNow(Vector3 playerPos)
        {
            var activeCount = PowerUpRegistry.ExpCount;
            if (activeCount <= Config.MaxActiveExpCount) return false;

            var manager = PowerUpManager.Default;
            if (!manager || !manager.Exp) return false;

            var exceedCount = activeCount - Config.MaxActiveExpCount;
            var desiredCount = Mathf.Min(activeCount, Mathf.Max(exceedCount, ExpMinMergeBatchCount));
            var mergedCount = BuildExpMergeBatch(playerPos, desiredCount);
            if (mergedCount < 2) return false;

            var totalValue = 0;
            var mergedValidCount = 0;
            var center = Vector3.zero;
            for (var i = 0; i < ExpMergeBatch.Count; i++)
            {
                var exp = ExpMergeBatch[i];
                if (!exp || !exp.gameObject.activeInHierarchy) continue;
                totalValue += Mathf.Max(1, exp.ExpValue);
                center += exp.transform.position;
                mergedValidCount++;
            }

            if (mergedValidCount < 2 || totalValue <= 0) return false;
            center /= mergedValidCount;

            for (var i = 0; i < ExpMergeBatch.Count; i++)
            {
                var exp = ExpMergeBatch[i];
                if (!exp || !exp.gameObject.activeInHierarchy) continue;
                ObjectPoolSystem.Despawn(exp.gameObject);
            }

            var mergedGo = ObjectPoolSystem.Spawn(manager.Exp.gameObject, null, true);
            if (!mergedGo) return false;

            mergedGo.transform.position = center;
            var mergedExp = mergedGo.GetComponent<Exp>();
            if (mergedExp) mergedExp.SetExpValue(totalValue);

            return true;
        }

        public static bool TryMergeCoinNow(Vector3 playerPos)
        {
            var activeCount = PowerUpRegistry.CoinCount;
            if (activeCount <= Config.MaxActiveCoinCountSoft) return false;

            var manager = PowerUpManager.Default;
            if (!manager || !manager.Coin) return false;

            var exceedCount = activeCount - Config.MaxActiveCoinCountSoft;
            var desiredCountRaw = Mathf.Max(exceedCount * Config.CoinMergePressureFactor, Config.CoinMergeMinBatchCount);
            var desiredCount = Mathf.Clamp(desiredCountRaw, 0, activeCount);
            var mergedCount = BuildCoinMergeBatch(playerPos, desiredCount);
            if (mergedCount < 2) return false;

            var totalValue = 0;
            var mergedValidCount = 0;
            var center = Vector3.zero;
            for (var i = 0; i < CoinMergeBatch.Count; i++)
            {
                var coin = CoinMergeBatch[i];
                if (!coin || !coin.gameObject.activeInHierarchy) continue;
                totalValue += Mathf.Max(1, coin.CoinValue);
                center += coin.transform.position;
                mergedValidCount++;
            }

            if (mergedValidCount < 2 || totalValue <= 0) return false;
            center /= mergedValidCount;

            for (var i = 0; i < CoinMergeBatch.Count; i++)
            {
                var coin = CoinMergeBatch[i];
                if (!coin || !coin.gameObject.activeInHierarchy) continue;
                ObjectPoolSystem.Despawn(coin.gameObject);
            }

            var mergedGo = ObjectPoolSystem.Spawn(manager.Coin.gameObject, null, true);
            if (!mergedGo) return false;

            mergedGo.transform.position = center;
            var mergedCoin = mergedGo.GetComponent<Coin>();
            if (mergedCoin) mergedCoin.SetCoinValue(totalValue);

            CoinMergeTriggerCount++;
            return true;
        }

        private static int BuildExpMergeBatch(Vector3 playerPos, int desiredCount)
        {
            ExpMergeBatch.Clear();
            PowerUpRegistry.GetFarthestExps(playerPos, desiredCount, ExpCandidates);
            if (ExpCandidates.Count == 0) return 0;

            var radiusSqr = Config.ExpMergeRadius * Config.ExpMergeRadius;
            var anchor = ExpCandidates[0];
            if (!anchor) return 0;

            var anchorPos = anchor.transform.position;
            ExpMergeBatch.Add(anchor);

            for (var i = 1; i < ExpCandidates.Count; i++)
            {
                if (ExpMergeBatch.Count >= desiredCount) break;
                var exp = ExpCandidates[i];
                if (!exp) continue;
                if ((exp.transform.position - anchorPos).sqrMagnitude <= radiusSqr)
                {
                    ExpMergeBatch.Add(exp);
                }
            }

            for (var i = 1; i < ExpCandidates.Count; i++)
            {
                if (ExpMergeBatch.Count >= desiredCount) break;
                var exp = ExpCandidates[i];
                if (!exp || ExpMergeBatch.Contains(exp)) continue;
                ExpMergeBatch.Add(exp);
            }

            return ExpMergeBatch.Count;
        }

        private static int BuildCoinMergeBatch(Vector3 playerPos, int desiredCount)
        {
            CoinMergeBatch.Clear();
            PowerUpRegistry.GetFarthestCoins(playerPos, desiredCount, CoinCandidates);
            if (CoinCandidates.Count == 0) return 0;

            var radiusSqr = Config.CoinMergeRadius * Config.CoinMergeRadius;
            var anchor = CoinCandidates[0];
            if (!anchor) return 0;

            var anchorPos = anchor.transform.position;
            CoinMergeBatch.Add(anchor);

            for (var i = 1; i < CoinCandidates.Count; i++)
            {
                if (CoinMergeBatch.Count >= desiredCount) break;
                var coin = CoinCandidates[i];
                if (!coin) continue;
                if ((coin.transform.position - anchorPos).sqrMagnitude <= radiusSqr)
                {
                    CoinMergeBatch.Add(coin);
                }
            }

            for (var i = 1; i < CoinCandidates.Count; i++)
            {
                if (CoinMergeBatch.Count >= desiredCount) break;
                var coin = CoinCandidates[i];
                if (!coin || CoinMergeBatch.Contains(coin)) continue;
                CoinMergeBatch.Add(coin);
            }

            return CoinMergeBatch.Count;
        }
    }
}
