using System;
using QFramework;

namespace VampireSurvivorLike
{
    public class CoinUpgradeItem
    {
        public EasyEvent OnChanged = new EasyEvent();
        public bool UpgradeFinish{get;set;}=false;
        public string Key { get; private set; } //新增Key属性
        public string Description { get; private set; } //新增描述属性
        public int Price { get; private set; }  //新增价格属性
        private Action<CoinUpgradeItem> _mOnUpgrade; //升级时的回调
        private Func<CoinUpgradeItem,bool> _mCondition; //升级条件

        public void Upgrade()
        {
            _mOnUpgrade?.Invoke(this);
            UpgradeFinish = true;
            OnChanged.Trigger();
            CoinUpgradeSystem.OnCoinUpgradeSystemChanged.Trigger();
        }

        public bool ConditionCheck()
        {
            if(_mCondition!=null)
            {
                return UpgradeFinish && _mCondition.Invoke(this);
            }
            return !UpgradeFinish;
        }

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
            _mOnUpgrade = onUpgrade;
            return this;
        }

        public CoinUpgradeItem Condition(Func<CoinUpgradeItem,bool> condition)
        {
            _mCondition = condition;
            return this;
        }

        public CoinUpgradeItem WithPrice(int price)
        {
            Price = price;
            return this;
        }

        
    }
}
