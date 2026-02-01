using System.Collections.Generic;
using System.Linq;
using QFramework;
using UnityEngine;

namespace VampireSurvivorLike
{
    public class AchievementSystem : AbstractSystem
    {
        public AchievementSystem Add(AchievementItem item)
        {
            Items.Add(item);
            return this;
        }

        protected override void OnInit()
        {
            var saveSystem = this.GetSystem<SaveSystem>();

            Add(new AchievementItem()
                .WithKey("3_minutes")
                .WithName("坚持3分钟")
                .WithDescription("坚持3分钟\n获得成就奖励1000金币")
                .WithIconName("achievement_time_icon")
                .Condition(()=>Global.CurrentSeconds.Value >= 60 * 3)
                .OnUnlocked(_=>{Global.Coin.Value += 1000;})
            .Load(saveSystem));

            Add(new AchievementItem()
                .WithKey("5_minutes")
                .WithName("坚持5分钟")
                .WithDescription("坚持5分钟\n获得成就奖励1000金币")
                .WithIconName("achievement_time_icon")
                .Condition(()=>Global.CurrentSeconds.Value >= 60 * 5)
                .OnUnlocked(_=>{Global.Coin.Value += 1000;})
            .Load(saveSystem));

            Add(new AchievementItem()
                .WithKey("10_minutes")
                .WithName("坚持10分钟")
                .WithDescription("坚持10分钟\n获得成就奖励1000金币")
                .WithIconName("achievement_time_icon")
                .Condition(()=>Global.CurrentSeconds.Value >= 60 * 10)
                .OnUnlocked(_=>{Global.Coin.Value += 1000;})
            .Load(saveSystem));

            Add(new AchievementItem()
                .WithKey("20_minutes")
                .WithName("坚持20分钟")
                .WithDescription("坚持20分钟\n获得成就奖励1000金币")
                .WithIconName("achievement_time_icon")
                .Condition(()=>Global.CurrentSeconds.Value >= 60 * 20)
                .OnUnlocked(_=>{Global.Coin.Value += 1000;})
            .Load(saveSystem));

            Add(new AchievementItem()
                .WithKey("lv30")
                .WithName("30级")
                .WithDescription("第一次升级到30级\n获得成就奖励1000金币")
                .WithIconName("achievement_time_icon")
                .Condition(()=>Global.Level.Value >= 30)
                .OnUnlocked(_=>{Global.Coin.Value += 1000;})
            .Load(saveSystem));

            Add(new AchievementItem()
                .WithKey("lv50")
                .WithName("50级")
                .WithDescription("第一次升级到50级\n获得成就奖励1000金币")
                .WithIconName("achievement_time_icon")
                .Condition(()=>Global.Level.Value >= 50)
                .OnUnlocked(_=>{Global.Coin.Value += 1000;})
            .Load(saveSystem));

            Add(new AchievementItem()
                .WithKey("first_time_paired_ball")
                .WithName("合成后的篮球")
                .WithDescription("第一次解锁合成后的篮球\n获得成就奖励1000金币")
                .WithIconName("paired_ball_icon")
                .Condition(()=>Global.SuperBasketBall.Value)
                .OnUnlocked(_=>{Global.Coin.Value += 1000;})
            .Load(saveSystem));

            Add(new AchievementItem()
                .WithKey("first_time_paired_bomb")
                .WithName("合成后的炸弹")
                .WithDescription("第一次解锁合成后的炸弹\n获得成就奖励1000金币")
                .WithIconName("paired_bomb_icon")
                .Condition(()=>Global.SuperBomb.Value)
                .OnUnlocked(_=>{Global.Coin.Value += 1000;})
            .Load(saveSystem));

            Add(new AchievementItem()
                .WithKey("first_time_paired_sword")
                .WithName("合成后的剑")
                .WithDescription("第一次解锁合成后的剑\n获得成就奖励1000金币")
                .WithIconName("paired_simple_sword_icon")
                .Condition(()=>Global.SuperSword.Value)
                .OnUnlocked(_=>{Global.Coin.Value += 1000;})
            .Load(saveSystem));

            Add(new AchievementItem()
                .WithKey("first_time_paired_knife")
                .WithName("合成后的飞刀")
                .WithDescription("第一次解锁合成后的飞刀\n获得成就奖励1000金币")
                .WithIconName("paired_simple_knife_icon")
                .Condition(()=>Global.SuperKnife.Value)
                .OnUnlocked(_=>{Global.Coin.Value += 1000;})
            .Load(saveSystem));

            Add(new AchievementItem()
                .WithKey("first_time_paired_circle")
                .WithName("合成后的守卫剑")
                .WithDescription("第一次解锁合成后的守卫剑\n获得成就奖励1000金币")
                .WithIconName("paired_rotate_sword_icon")
                .Condition(()=>Global.SuperRotateSword.Value)
                .OnUnlocked(_=>{Global.Coin.Value += 1000;})
            .Load(saveSystem));

            Add(new AchievementItem()
                .WithKey("all_ability_upgrade")  // 改成不同的 key
                .WithName("全部能力升级")
                .WithDescription("全部能力升级完成\n获得成就奖励1000金币")
                .WithIconName("achievement_all_icon")
                .Condition(()=>ExpUpgradeSystem.AllUnlockedFinished)
                .OnUnlocked(_=>{Global.Coin.Value += 1000;})
            .Load(saveSystem));


            // 每10帧检查一次成就解锁条件
            ActionKit.OnUpdate.Register(() =>
            {
                if(Time.frameCount % 10 == 0)
                {
                    CheckAndUnlockAchievements(saveSystem);
                }
            });

            // 当超级武器解锁时立即检查成就
            Global.SuperSword.Register(_ => CheckAndUnlockAchievements(saveSystem));
            Global.SuperKnife.Register(_ => CheckAndUnlockAchievements(saveSystem));
            Global.SuperBomb.Register(_ => CheckAndUnlockAchievements(saveSystem));
            Global.SuperBasketBall.Register(_ => CheckAndUnlockAchievements(saveSystem));
            Global.SuperRotateSword.Register(_ => CheckAndUnlockAchievements(saveSystem));
        }

        /// <summary>
        /// 检查并解锁满足条件的成就
        /// </summary>
        private void CheckAndUnlockAchievements(SaveSystem saveSystem)
        {
            foreach (var achievementItem in Items.Where(item => 
                !item.Unlocked && item.ConditionCheck()))
            {
                achievementItem.UnLock(saveSystem);
            }
        }

        public List<AchievementItem> Items = new List<AchievementItem>();

        public static EasyEvent<AchievementItem> OnAchievementUnlocked = new EasyEvent<AchievementItem>();
        
    }
}

