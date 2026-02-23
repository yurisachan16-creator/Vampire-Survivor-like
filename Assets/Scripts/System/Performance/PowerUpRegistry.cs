using System.Collections.Generic;

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
        }
    }
}
