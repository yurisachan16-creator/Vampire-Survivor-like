using System;
using QFramework;

namespace VampireSurvivorLike
{
    public class ExpUpgradeItem
    {
        
        public bool UpgradeFinish{get;set;}=false;
        public string Key { get; private set; } //新增Key属性
        public string Description => _mDescriptionFactory(CurrentLevel.Value); 

        public int MaxLevel { get; private set; } = 1; //新增最大等级属性
        public BindableProperty<int> CurrentLevel = new BindableProperty<int>(1);
        public BindableProperty<bool> Visible { get; } = new BindableProperty<bool>();
        private Func<int,string> _mDescriptionFactory;
        private Action<ExpUpgradeItem, int> _mOnUpgrade; //升级时的回调
        private Func<ExpUpgradeItem,bool> _mCondition; //升级条件

        public void Upgrade()
        {
            CurrentLevel.Value++;
            if (_mCondition == null || _mCondition.Invoke(this))
            {
                _mOnUpgrade?.Invoke(this, CurrentLevel.Value);
            }
            if (CurrentLevel.Value > 10)
            {
                UpgradeFinish = true;
            }
            
        }

        

        public ExpUpgradeItem WithKey(string key)
        {
            Key = key;
            return this;
        }

        public ExpUpgradeItem WithDescription(Func<int,string> descriptionFactory)
        {
            _mDescriptionFactory = descriptionFactory;
            return this;
        }

        public ExpUpgradeItem OnUpgrade(Action<ExpUpgradeItem,int> onUpgrade)
        {
            _mOnUpgrade = onUpgrade;
            return this;
        }

        public ExpUpgradeItem Condition(Func<ExpUpgradeItem,bool> condition)
        {
            _mCondition = condition;
            return this;
        }

        public ExpUpgradeItem WithMaxLevel(int maxLevel)
        {
            MaxLevel = maxLevel;
            return this;
        }

        
    }
}
