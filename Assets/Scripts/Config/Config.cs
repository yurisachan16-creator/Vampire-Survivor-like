namespace VampireSurvivorLike
{
    public class Config
    {
        public const float InitSimpleSwordDamage=3f;
        public const float InitSimpleSwordDuration=1.5f;
        public const int InitSimpleSwordCount=3;
        public const float InitSimpleSwordRange=3f;

        public const float InitSimpleKnifeDamage=5f;
        public const float InitSimpleKnifeDuration=1f;
        public const int InitSimpleKnifeCount=3;
        public const int InitSimpleKnifeAttackCount=1; //穿透数量

        public const float InitRotateSwordDamage=5;
        public const int InitRotateSwordCount=1;
        public const float InitRotateSwordSpeed=2f;
        public const float InitRotateSwordRange=2f;

        public const float InitBasketBallDamage=5f;
        public const float InitBasketBallSpeed=10f;
        public const int InitBasketBallCount=1;

        public const float InitBombDamage=10f;
        public const float InitBombPercent=0.05f;

        public const float InitSimpleAxeDamage = 8f;
        public const float InitSimpleAxeDuration = 1.2f;
        public const int InitSimpleAxeCount = 1;
        public const int InitSimpleAxePierce = 1;

        public const float InitCriticalRate=0.05f;  //暴击率

        public const float InitDamageRate=1.0f; //伤害倍率

        public const float InitMovementSpeedRate=1.0f;  //移动速度倍率

        public const float InitCollectableAreaRadius=1f;    //可收集物品范围半径
        public const float InitAdditionalExpPercent=0f; //额外经验值获取百分比

        // ── 时间轴波次系统 ──
        /// <summary>一局游戏最大时长（秒）：30分钟</summary>
        public const float MaxGameSeconds = 1800f;
        /// <summary>每分钟 HP 增长倍率（叠加到频道基础值上）</summary>
        public const float HPGrowthPerMinute = 0.25f;
        /// <summary>每分钟速度增长倍率</summary>
        public const float SpeedGrowthPerMinute = 0.02f;
        /// <summary>每分钟伤害增长倍率</summary>
        public const float DamageGrowthPerMinute = 0.15f;
        /// <summary>每分钟刷新频率加速倍率（间隔缩短）</summary>
        public const float SpawnRateGrowthPerMinute = 0.1f;
        /// <summary>允许多实例后回血道具概率缩放系数</summary>
        public const float RecoverHpDropRateScaleWhenMulti = 0.22f;
        /// <summary>允许多实例后全图吸附道具固定掉落率</summary>
        public const float GetAllExpDropRateWhenMulti = 0.01f;
        /// <summary>酒掉落概率</summary>
        public const float WineDropRate = 0.03f;
        /// <summary>柠檬 Buff 掉落概率</summary>
        public const float LemonBuffDropRate = 0.025f;
        /// <summary>柠檬 Buff 伤害加成（+40%）</summary>
        public const float LemonBuffDamageBonus = 0.4f;
        /// <summary>柠檬 Buff 持续时间（秒）</summary>
        public const float LemonBuffDurationSeconds = 6f;
        /// <summary>死神出现的游戏时间（秒）</summary>
        public const float ReaperSpawnTimeSeconds = 1800f;
        /// <summary>死神预制体名称</summary>
        public const string ReaperPrefabName = "Enemy_Reaper";
    }
}
