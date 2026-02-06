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
                .WithNameKey("achievement.3_minutes.name")
                .WithName("坚持3分钟")
                .WithDescriptionKey("achievement.3_minutes.desc")
                .WithDescription("坚持3分钟\n获得成就奖励1000金币")
                .WithIconName("achievement_time_icon")
                .Condition(()=>Global.CurrentSeconds.Value >= 60 * 3)
                .OnUnlocked(_=>{Global.Coin.Value += 1000;})
            .Load(saveSystem));

            Add(new AchievementItem()
                .WithKey("5_minutes")
                .WithNameKey("achievement.5_minutes.name")
                .WithName("坚持5分钟")
                .WithDescriptionKey("achievement.5_minutes.desc")
                .WithDescription("坚持5分钟\n获得成就奖励1000金币")
                .WithIconName("achievement_time_icon")
                .Condition(()=>Global.CurrentSeconds.Value >= 60 * 5)
                .OnUnlocked(_=>{Global.Coin.Value += 1000;})
            .Load(saveSystem));

            Add(new AchievementItem()
                .WithKey("10_minutes")
                .WithNameKey("achievement.10_minutes.name")
                .WithName("坚持10分钟")
                .WithDescriptionKey("achievement.10_minutes.desc")
                .WithDescription("坚持10分钟\n获得成就奖励1000金币")
                .WithIconName("achievement_time_icon")
                .Condition(()=>Global.CurrentSeconds.Value >= 60 * 10)
                .OnUnlocked(_=>{Global.Coin.Value += 1000;})
            .Load(saveSystem));

            Add(new AchievementItem()
                .WithKey("20_minutes")
                .WithNameKey("achievement.20_minutes.name")
                .WithName("坚持20分钟")
                .WithDescriptionKey("achievement.20_minutes.desc")
                .WithDescription("坚持20分钟\n获得成就奖励1000金币")
                .WithIconName("achievement_time_icon")
                .Condition(()=>Global.CurrentSeconds.Value >= 60 * 20)
                .OnUnlocked(_=>{Global.Coin.Value += 1000;})
            .Load(saveSystem));

            Add(new AchievementItem()
                .WithKey("lv30")
                .WithNameKey("achievement.lv30.name")
                .WithName("30级")
                .WithDescriptionKey("achievement.lv30.desc")
                .WithDescription("第一次升级到30级\n获得成就奖励1000金币")
                .WithIconName("achievement_time_icon")
                .Condition(()=>Global.Level.Value >= 30)
                .OnUnlocked(_=>{Global.Coin.Value += 1000;})
            .Load(saveSystem));

            Add(new AchievementItem()
                .WithKey("lv50")
                .WithNameKey("achievement.lv50.name")
                .WithName("50级")
                .WithDescriptionKey("achievement.lv50.desc")
                .WithDescription("第一次升级到50级\n获得成就奖励1000金币")
                .WithIconName("achievement_time_icon")
                .Condition(()=>Global.Level.Value >= 50)
                .OnUnlocked(_=>{Global.Coin.Value += 1000;})
            .Load(saveSystem));

            Add(new AchievementItem()
                .WithKey("first_time_paired_ball")
                .WithNameKey("achievement.first_time_paired_ball.name")
                .WithName("合成后的篮球")
                .WithDescriptionKey("achievement.first_time_paired_ball.desc")
                .WithDescription("第一次解锁合成后的篮球\n获得成就奖励1000金币")
                .WithIconName("paired_ball_icon")
                .Condition(()=>Global.SuperBasketBall.Value)
                .OnUnlocked(_=>{Global.Coin.Value += 1000;})
            .Load(saveSystem));

            Add(new AchievementItem()
                .WithKey("first_time_paired_bomb")
                .WithNameKey("achievement.first_time_paired_bomb.name")
                .WithName("合成后的炸弹")
                .WithDescriptionKey("achievement.first_time_paired_bomb.desc")
                .WithDescription("第一次解锁合成后的炸弹\n获得成就奖励1000金币")
                .WithIconName("paired_bomb_icon")
                .Condition(()=>Global.SuperBomb.Value)
                .OnUnlocked(_=>{Global.Coin.Value += 1000;})
            .Load(saveSystem));

            Add(new AchievementItem()
                .WithKey("first_time_paired_sword")
                .WithNameKey("achievement.first_time_paired_sword.name")
                .WithName("合成后的剑")
                .WithDescriptionKey("achievement.first_time_paired_sword.desc")
                .WithDescription("第一次解锁合成后的剑\n获得成就奖励1000金币")
                .WithIconName("paired_simple_sword_icon")
                .Condition(()=>Global.SuperSword.Value)
                .OnUnlocked(_=>{Global.Coin.Value += 1000;})
            .Load(saveSystem));

            Add(new AchievementItem()
                .WithKey("first_time_paired_knife")
                .WithNameKey("achievement.first_time_paired_knife.name")
                .WithName("合成后的飞刀")
                .WithDescriptionKey("achievement.first_time_paired_knife.desc")
                .WithDescription("第一次解锁合成后的飞刀\n获得成就奖励1000金币")
                .WithIconName("paired_simple_knife_icon")
                .Condition(()=>Global.SuperKnife.Value)
                .OnUnlocked(_=>{Global.Coin.Value += 1000;})
            .Load(saveSystem));

            Add(new AchievementItem()
                .WithKey("first_time_paired_circle")
                .WithNameKey("achievement.first_time_paired_circle.name")
                .WithName("合成后的守卫剑")
                .WithDescriptionKey("achievement.first_time_paired_circle.desc")
                .WithDescription("第一次解锁合成后的守卫剑\n获得成就奖励1000金币")
                .WithIconName("paired_rotate_sword_icon")
                .Condition(()=>Global.SuperRotateSword.Value)
                .OnUnlocked(_=>{Global.Coin.Value += 1000;})
            .Load(saveSystem));

            Add(new AchievementItem()
                .WithKey("all_ability_upgrade")  // 改成不同的 key
                .WithNameKey("achievement.all_ability_upgrade.name")
                .WithName("全部能力升级")
                .WithDescriptionKey("achievement.all_ability_upgrade.desc")
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

