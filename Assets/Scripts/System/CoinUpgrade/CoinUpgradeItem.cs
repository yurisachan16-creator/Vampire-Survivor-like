using System;
using QFramework;

namespace VampireSurvivorLike
{
    public class CoinUpgradeItem
    {
        public string Key { get; private set; } //新增Key属性
        public string Description { get; private set; } //新增描述属性
        public int Price { get; private set; }  //新增价格属性
        private Action<CoinUpgradeItem> _onUpgrade;

        public CoinUpgradeItem WithKey(string key)
        {
            Key = key;
            return this;
        }

        public CoinUpgradeItem WithDescription(string description)
        {
            Description = description;
            return this;
        }

        public CoinUpgradeItem OnUpgrade(Action<CoinUpgradeItem> onUpgrade)
        {
            _onUpgrade = onUpgrade;
            return this;
        }

        public CoinUpgradeItem WithPrice(int price)
        {
            Price = price;
            return this;
        }

        public void Upgrade()
        {
            _onUpgrade?.Invoke(this);
        }
    }
}
