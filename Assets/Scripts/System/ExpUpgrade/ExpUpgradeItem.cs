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
        public string Name => Resolve(_nameKey, _nameLiteral);
        public string Description => NextDescription;
        public string CurrentDescription => GetDescriptionAtLevel(CurrentLevel.Value);
        public string NextDescription => GetDescriptionAtLevel(CurrentLevel.Value + 1);

        public int MaxLevel { get; private set; } //新增最大等级属性
        public string IconName { get;private set; } //图标名称
        public BindableProperty<int> CurrentLevel = new BindableProperty<int>(0);
        public BindableProperty<bool> Visible { get; } = new BindableProperty<bool>();
        private string _nameLiteral;
        private string _nameKey;
        private string _pairedNameLiteral;
        private string _pairedNameKey;
        private string _pairedDescriptionLiteral;
        private string _pairedDescriptionKey;
        private Func<int,string> _mDescriptionFactory;
        private Func<int, string> _mDescriptionKeyFactory;
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
            _nameLiteral = name;
            _nameKey = null;
            return this;
        }

        public ExpUpgradeItem WithNameKey(string nameKey)
        {
            _nameKey = nameKey;
            if (string.IsNullOrWhiteSpace(_nameLiteral)) _nameLiteral = null;
            return this;
        }

        public ExpUpgradeItem WithIconName(string iconName)
        {
            IconName = iconName;
            return this;
        }

        public string PairedName => Resolve(_pairedNameKey, _pairedNameLiteral);
        public string PairedDescription => Resolve(_pairedDescriptionKey, _pairedDescriptionLiteral);
        public string PairedIconName{get; private set;}

        public ExpUpgradeItem WithPairedName(string pairedName)
        {
            _pairedNameLiteral = pairedName;
            _pairedNameKey = null;
            return this;
        }

        public ExpUpgradeItem WithPairedNameKey(string pairedNameKey)
        {
            _pairedNameKey = pairedNameKey;
            if (string.IsNullOrWhiteSpace(_pairedNameLiteral)) _pairedNameLiteral = null;
            return this;
        }

        public ExpUpgradeItem WithPairedDescription(string pairedDescription)
        {
            _pairedDescriptionLiteral = pairedDescription;
            _pairedDescriptionKey = null;
            return this;
        }

        public ExpUpgradeItem WithPairedDescriptionKey(string pairedDescriptionKey)
        {
            _pairedDescriptionKey = pairedDescriptionKey;
            if (string.IsNullOrWhiteSpace(_pairedDescriptionLiteral)) _pairedDescriptionLiteral = null;
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
            _mDescriptionKeyFactory = null;
            return this;
        }

        public ExpUpgradeItem WithDescriptionKey(Func<int, string> descriptionKeyFactory)
        {
            _mDescriptionKeyFactory = descriptionKeyFactory;
            if (_mDescriptionFactory == null) _mDescriptionFactory = null;
            return this;
        }

        public string GetDescriptionAtLevel(int level)
        {
            if (level < 1) level = 1;

            if (_mDescriptionKeyFactory != null)
            {
                var key = _mDescriptionKeyFactory(level);
                if (string.IsNullOrWhiteSpace(key)) return string.Empty;
                if (LocalizationManager.TryGet(key, out var value)) return value;
                if (_mDescriptionFactory != null) return _mDescriptionFactory(level);
                return key;
            }

            if (_mDescriptionFactory == null) return string.Empty;
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

        private static string Resolve(string key, string fallbackLiteral)
        {
            if (!string.IsNullOrWhiteSpace(key))
            {
                if (LocalizationManager.TryGet(key, out var value)) return value;
                if (!string.IsNullOrWhiteSpace(fallbackLiteral)) return fallbackLiteral;
                return key;
            }

            return fallbackLiteral ?? string.Empty;
        }
        
    }
}
