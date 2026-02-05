using System;
using QFramework;

namespace VampireSurvivorLike
{
    public class ExpUpgradeItem
    {
        public ExpUpgradeItem(bool isWeapon)
        {
            IsWeapon = isWeapon;
        }
        public bool IsWeapon = false;   //是否是武器
        public bool UpgradeFinish => CurrentLevel.Value >= MaxLevel;
        public string Key { get; private set; } //新增Key属性
        public string Name {get; private set;}
        public string Description => NextDescription;
        public string CurrentDescription => GetDescriptionAtLevel(CurrentLevel.Value);
        public string NextDescription => GetDescriptionAtLevel(CurrentLevel.Value + 1);

        public int MaxLevel { get; private set; } //新增最大等级属性
        public string IconName { get;private set; } //图标名称
        public BindableProperty<int> CurrentLevel = new BindableProperty<int>(0);
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
            
            ExpUpgradeSystem.CheckAllUnlockedFinish();
            
        }

        

        public ExpUpgradeItem WithKey(string key)
        {
            Key = key;
            return this;
        }

        public ExpUpgradeItem WithName(string name)
        {
            Name = name;
            return this;
        }

        public ExpUpgradeItem WithIconName(string iconName)
        {
            IconName = iconName;
            return this;
        }

        public string PairedName{get; private set;}
        public string PairedDescription{get; private set;}
        public string PairedIconName{get; private set;}

        public ExpUpgradeItem WithPairedName(string pairedName)
        {
            PairedName = pairedName;
            return this;
        }

        public ExpUpgradeItem WithPairedDescription(string pairedDescription)
        {
            PairedDescription = pairedDescription;
            return this;
        }

        public ExpUpgradeItem WithPairedIconName(string pairedIconName)
        {
            PairedIconName = pairedIconName;
            return this;
        }

        public ExpUpgradeItem WithDescription(Func<int,string> descriptionFactory)
        {
            _mDescriptionFactory = descriptionFactory;
            return this;
        }

        public string GetDescriptionAtLevel(int level)
        {
            if (_mDescriptionFactory == null) return string.Empty;
            if (level < 1) level = 1;
            return _mDescriptionFactory(level);
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
