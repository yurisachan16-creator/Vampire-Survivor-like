using System;
using UnityEngine;

namespace VampireSurvivorLike
{
    public class AchievementItem
    {
        public string Key { get; private set; } 
        public string Name {get; private set;}
        public string Description { get; private set; }
        public bool Unlocked { get; private set; }
        public string IconName { get; private set; }

        private Func<bool> _mCondition;
        private Action<AchievementItem> _mOnUnlocked;

        public AchievementItem WithKey(string key)
        {
            Key = key;
            return this;
        }

        public AchievementItem WithName(string name)
        {
            Name = name;
            return this;
        }

        public AchievementItem WithDescription(string description)
        {
            Description = description;
            return this;
        }

        public AchievementItem WithIconName(string iconName)
        {
            IconName = iconName;
            return this;
        }

        public AchievementItem OnUnlocked(Action<AchievementItem> onUnlocked)
        {
            _mOnUnlocked = onUnlocked;
            return this;
        }

        public AchievementItem Condition(Func<bool> condition)
        {
            _mCondition = condition;
            return this;
        }

        public bool ConditionCheck()
        {
            return _mCondition();
        }

        public AchievementItem Load(SaveSystem saveSystem)
        {
            Unlocked = saveSystem.LoadBool($"achievement_first_{Key}",false);
            return this;
        }

        public void UnLock(SaveSystem saveSystem)
        {
            Unlocked = true;
            Global.Interface.GetSystem<SaveSystem>().SaveBool($"achievement_first_{Key}",true);
            _mOnUnlocked?.Invoke(this);
            AchievementSystem.OnAchivementUnlocked.Trigger(this);
        }
    }
}

