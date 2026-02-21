using System.Collections.Generic;
using System.Linq;
using QFramework;

namespace VampireSurvivorLike
{
    public class ExpUpgradeSystem : AbstractSystem
    {
        public static EasyEvent OnExpUpgradeSystemChanged = new EasyEvent();
        /// <summary>
        /// 升级面板应该显示时触发（参数为true表示有可选项，false表示无可选项）
        /// </summary>
        public static EasyEvent<bool> OnUpgradePanelShouldShow = new EasyEvent<bool>();
        public List<ExpUpgradeItem> Items{get;} = new List<ExpUpgradeItem>();
        public static bool AllUnlockedFinished = false;

        public static void CheckAllUnlockedFinish()
        {
            AllUnlockedFinished = Global.Interface.GetSystem<ExpUpgradeSystem>().Items
                .All(i=>i.UpgradeFinish);
        }

        public Dictionary<string,ExpUpgradeItem> Dictionary = new Dictionary<string, ExpUpgradeItem>();
        public Dictionary<string,string> Pairs = new Dictionary<string, string>()
        {
            {"simple_sword","simple_critical"},
            {"simple_bomb","simple_fly_count"},
            {"simple_knife","damage_rate"},
            {"basket_ball","movement_speed_rate"},
            {"rotate_sword","simple_exp_percent"},
            {"simple_axe","yellow_potion"},

            {"simple_critical","simple_sword"},
            {"simple_fly_count","simple_bomb"},
            {"damage_rate","simple_knife"},
            {"movement_speed_rate","basket_ball"},
            {"simple_exp_percent","rotate_sword"},
            {"yellow_potion","simple_axe"},
        };

        public Dictionary<string,BindableProperty<bool>> PairedProperties = 
            new()
            {
                {"simple_sword",Global.SuperSword},
                {"simple_bomb",Global.SuperBomb},
                {"simple_knife",Global.SuperKnife},
                {"basket_ball",Global.SuperBasketBall},
                {"rotate_sword",Global.SuperRotateSword},
                {"simple_axe",Global.SuperAxe},

                // {"simple_critical",Global.SuperSword},
                // {"simple_fly_count",Global.SuperBomb},
                // {"damage_rate",Global.SuperKnife},
                // {"movement_speed_rate",Global.SuperBasketBall},
                // {"simple_exp_percent",Global.SuperRotateSword},
            };

        public ExpUpgradeItem Add(ExpUpgradeItem item)
        {
            Items.Add(item);
            if (item.Key.IsNotNullAndEmpty())
            {
                Dictionary[item.Key] = item;
            }
            return item;
        }
        protected override void OnInit()
        {
            ResetData();

            Global.Level.Register(_=>
            {
                var hasItems = Roll();
                OnUpgradePanelShouldShow.Trigger(hasItems);
            });
        }

        public void ResetData()
        {
            LocalizationManager.PreloadTable("upgrade");
            Items.Clear();
            Dictionary.Clear();

            Add(new ExpUpgradeItem(true)
                .WithKey("simple_sword")
                .WithMaxLevel(10)
                .WithName("剑")
                .WithNameKey("exp_upgrade.simple_sword.name")
                .WithIconName("simple_sword_icon")
                .WithPairedName("合成后的剑")
                .WithPairedNameKey("exp_upgrade.simple_sword.paired_name")
                .WithPairedIconName("paired_simple_sword_icon")
                .WithPairedDescription("攻击力翻倍 攻击距离翻倍")
                .WithPairedDescriptionKey("exp_upgrade.simple_sword.paired_desc")
                .WithDescription(lv =>
                {
                    return lv switch
                    {
                        1=>$"剑lv{lv}:攻击身边的敌人",
                        2=>$"剑lv{lv}：\n攻击力+3 数量+2",
                        3=>$"剑lv{lv}：\n攻击力+2 间隔-0.25s",
                        4=>$"剑lv{lv}：\n攻击力+3 间隔-0.25s",
                        5=>$"剑lv{lv}：\n攻击力+4 间隔-0.25s",
                        6=>$"剑lv{lv}：\n范围+1 间隔-0.25s",
                        7=>$"剑lv{lv}：\n攻击力+3 数量+2",
                        8=>$"剑lv{lv}：\n攻击力+2 范围+1",
                        9=>$"剑lv{lv}：\n攻击力+5 间隔-0.25s",
                        10=>$"剑lv{lv}：\n攻击力+3 数量+2",
                        _=>null
                    };
                })
                .WithDescriptionKey(lv =>
                {
                    return lv switch
                    {
                        1=>"exp_upgrade.simple_sword.lv1",
                        2=>"exp_upgrade.simple_sword.lv2",
                        3=>"exp_upgrade.simple_sword.lv3",
                        4=>"exp_upgrade.simple_sword.lv4",
                        5=>"exp_upgrade.simple_sword.lv5",
                        6=>"exp_upgrade.simple_sword.lv6",
                        7=>"exp_upgrade.simple_sword.lv7",
                        8=>"exp_upgrade.simple_sword.lv8",
                        9=>"exp_upgrade.simple_sword.lv9",
                        10=>"exp_upgrade.simple_sword.lv10",
                        _=>null
                    };
                })
                .OnUpgrade((_, level) =>
                {
                    switch (level)
                    {
                        case 1:
                            //解锁简单剑
                            Global.SimpleSwordUnlocked.Value = true;
                            break;
                        case 2:
                            Global.SimpleAbilityDamage.Value += 3;
                            Global.SimpleSwordCount.Value += 2;
                            break;
                        case 3:
                            Global.SimpleAbilityDamage.Value += 2;
                            Global.SimpleAbilityDuration.Value -= 0.25f;
                            break;
                        case 4:
                            Global.SimpleAbilityDamage.Value += 3;
                            Global.SimpleAbilityDuration.Value -= 0.25f;
                            break;
                        case 5:
                            Global.SimpleAbilityDamage.Value += 4;
                            Global.SimpleAbilityDuration.Value -= 0.25f;
                            break;
                        case 6:
                            Global.SimpleSwordRange.Value += 1;
                            Global.SimpleAbilityDuration.Value -= 0.25f;
                            break;
                        case 7:
                            Global.SimpleAbilityDamage.Value += 3;
                            Global.SimpleSwordCount.Value += 2;
                            break;
                        case 8:
                            Global.SimpleAbilityDamage.Value += 2;
                            Global.SimpleSwordRange.Value += 1;
                            break;
                        case 9:
                            Global.SimpleAbilityDamage.Value += 5;
                            Global.SimpleAbilityDuration.Value -= 0.25f;
                            break;
                        case 10:
                            Global.SimpleAbilityDamage.Value += 3;
                            Global.SimpleSwordCount.Value += 2;
                            break;
                    }
                })
            );

            Add(new ExpUpgradeItem(true)
                .WithKey("rotate_sword")
                .WithMaxLevel(10)
                .WithName("守卫剑")
                .WithNameKey("exp_upgrade.rotate_sword.name")
                .WithIconName("rotate_sword_icon")
                .WithPairedName("合成后的守卫剑")
                .WithPairedNameKey("exp_upgrade.rotate_sword.paired_name")
                .WithPairedIconName("paired_rotate_sword_icon")
                .WithPairedDescription("攻击力翻倍 旋转速度翻倍")
                .WithPairedDescriptionKey("exp_upgrade.rotate_sword.paired_desc")
                .WithDescription(lv =>
                {
                    return lv switch
                    {
                        1=>$"守卫剑Lv{lv}:\n环绕主角身边的剑",
                        2=>$"守卫剑Lv{lv}：\n数量+1 攻击力+2",
                        3=>$"守卫剑Lv{lv}：\n攻击力+2 速度+25%",
                        4=>$"守卫剑Lv{lv}：\n速度+50%",
                        5=>$"守卫剑Lv{lv}：\n数量+1 攻击力+1",
                        6=>$"守卫剑Lv{lv}：\n攻击力+2 速度+25%",
                        7=>$"守卫剑Lv{lv}：\n数量+1 攻击力+1",
                        8=>$"守卫剑Lv{lv}：\n攻击力+2 速度+25%",
                        9=>$"守卫剑Lv{lv}：\n数量+1 攻击力+1",
                        10=>$"守卫剑Lv{lv}：\n攻击力+2 速度+25%",
                        _=>null
                    };
                })
                .WithDescriptionKey(lv =>
                {
                    return lv switch
                    {
                        1=>"exp_upgrade.rotate_sword.lv1",
                        2=>"exp_upgrade.rotate_sword.lv2",
                        3=>"exp_upgrade.rotate_sword.lv3",
                        4=>"exp_upgrade.rotate_sword.lv4",
                        5=>"exp_upgrade.rotate_sword.lv5",
                        6=>"exp_upgrade.rotate_sword.lv6",
                        7=>"exp_upgrade.rotate_sword.lv7",
                        8=>"exp_upgrade.rotate_sword.lv8",
                        9=>"exp_upgrade.rotate_sword.lv9",
                        10=>"exp_upgrade.rotate_sword.lv10",
                        _=>null
                    };
                })
                .OnUpgrade((_, level) =>
                {
                    switch (level)
                    {
                        case 1:
                            //解锁旋转剑
                            Global.RotateSwordUnlocked.Value = true;
                            break;
                        case 2:
                            Global.RotateSwordCount.Value += 1;
                            Global.RotateSwordDamage.Value += 2;
                            break;
                        case 3:
                            Global.RotateSwordDamage.Value += 2;
                            Global.RotateSwordSpeed.Value *= 1.25f;
                            break;
                        case 4:
                            Global.RotateSwordSpeed.Value *= 1.5f;

                            break;
                        case 5:
                            Global.RotateSwordCount.Value += 1;
                            Global.RotateSwordDamage.Value += 1;

                            break;
                        case 6:
                            Global.RotateSwordDamage.Value += 2;
                            Global.RotateSwordSpeed.Value *= 1.25f;

                            break;
                        case 7:
                            Global.RotateSwordCount.Value += 1;
                            Global.RotateSwordDamage.Value += 1;

                            break;
                        case 8:
                            Global.RotateSwordDamage.Value += 2;
                            Global.RotateSwordSpeed.Value *= 1.25f;

                            break;
                        case 9:
                            Global.RotateSwordCount.Value += 1;
                            Global.RotateSwordDamage.Value += 1;

                            break;
                        case 10:
                            Global.RotateSwordDamage.Value += 2;
                            Global.RotateSwordSpeed.Value *= 1.25f;

                            break;
                    }
                }));

            Add(new ExpUpgradeItem(false)
                .WithKey("simple_bomb")
                .WithName("炸弹")
                .WithNameKey("exp_upgrade.simple_bomb.name")
                .WithIconName("bomb_icon")
                .WithPairedName("合成后的炸弹")
                .WithPairedNameKey("exp_upgrade.simple_bomb.paired_name")
                .WithPairedIconName("paired_bomb_icon")
                .WithPairedDescription("每隔15秒爆炸一次")
                .WithPairedDescriptionKey("exp_upgrade.simple_bomb.paired_desc")
                .WithMaxLevel(10)
                .WithDescription(lv =>
                {
                    return lv switch
                    {
                        1=>$"炸弹Lv{lv}:攻击全部敌人(怪物掉落)",
                        2=>$"炸弹Lv{lv}：\n掉落概率+5% 攻击力+5",
                        3=>$"炸弹Lv{lv}：\n掉落概率+5% 攻击力+5",
                        4=>$"炸弹Lv{lv}：\n掉落概率+5% 攻击力+5",
                        5=>$"炸弹Lv{lv}：\n掉落概率+5% 攻击力+5",
                        6=>$"炸弹Lv{lv}：\n掉落概率+5% 攻击力+5",
                        7=>$"炸弹Lv{lv}：\n掉落概率+5% 攻击力+5",
                        8=>$"炸弹Lv{lv}：\n掉落概率+5% 攻击力+5",
                        9=>$"炸弹Lv{lv}：\n掉落概率+5% 攻击力+5",
                        10=>$"炸弹Lv{lv}：\n掉落概率+10% 攻击力+5",
                        _=>null
                    };
                })
                .WithDescriptionKey(lv =>
                {
                    return lv switch
                    {
                        1=>"exp_upgrade.simple_bomb.lv1",
                        2=>"exp_upgrade.simple_bomb.lv2",
                        3=>"exp_upgrade.simple_bomb.lv3",
                        4=>"exp_upgrade.simple_bomb.lv4",
                        5=>"exp_upgrade.simple_bomb.lv5",
                        6=>"exp_upgrade.simple_bomb.lv6",
                        7=>"exp_upgrade.simple_bomb.lv7",
                        8=>"exp_upgrade.simple_bomb.lv8",
                        9=>"exp_upgrade.simple_bomb.lv9",
                        10=>"exp_upgrade.simple_bomb.lv10",
                        _=>null
                    };
                })
                .OnUpgrade((_, level) =>
                {
                    switch (level)
                    {
                        case 1:
                            //解锁炸弹
                            Global.BombUnlocked.Value = true;
                            break;
                        case 2:
                            Global.BombPercent.Value += 0.05f;
                            Global.BombDamage.Value += 5;
                            break;
                        case 3:
                            Global.BombPercent.Value += 0.05f;
                            Global.BombDamage.Value += 5;
                            break;
                        case 4:
                            Global.BombPercent.Value += 0.05f;
                            Global.BombDamage.Value += 5;
                            break;
                        case 5:
                            Global.BombPercent.Value += 0.05f;
                            Global.BombDamage.Value += 5;
                            break;
                        case 6:
                            Global.BombPercent.Value += 0.05f;
                            Global.BombDamage.Value += 5;
                            break;
                        case 7:
                            Global.BombPercent.Value += 0.05f;
                            Global.BombDamage.Value += 5;
                            break;
                        case 8:
                            Global.BombPercent.Value += 0.05f;
                            Global.BombDamage.Value += 5;
                            break;
                        case 9:
                            Global.BombPercent.Value += 0.05f;
                            Global.BombDamage.Value += 5;
                            break;
                        case 10:
                            Global.BombPercent.Value += 0.10f;
                            Global.BombDamage.Value += 5;
                            break;
                    }
                }));    
           
            Add(new ExpUpgradeItem(true)
                .WithKey("simple_knife")
                .WithName("飞刀")
                .WithNameKey("exp_upgrade.simple_knife.name")
                .WithIconName("simple_knife_icon")
                .WithPairedName("合成后的飞刀")
                .WithPairedNameKey("exp_upgrade.simple_knife.paired_name")
                .WithPairedIconName("paired_simple_knife_icon")
                .WithPairedDescription("攻击力翻倍")
                .WithPairedDescriptionKey("exp_upgrade.simple_knife.paired_desc")
                .WithDescription(lv =>
                {
                    return lv switch
                    {
                        1=>$"飞刀Lv{lv}:向最近的敌人发射一把",
                        2=>$"飞刀Lv{lv}：\n攻击力+3 数量+2",
                        3=>$"飞刀Lv{lv}：\n间隔-0.1s 攻击力+1 数量+1",
                        4=>$"飞刀Lv{lv}：\n间隔-0.1s 穿透+1 数量+1",
                        5=>$"飞刀Lv{lv}：\n攻击力+3 数量+1",
                        6=>$"飞刀Lv{lv}：\n间隔-0.1s 数量+1",
                        7=>$"飞刀Lv{lv}：\n间隔-0.1s 穿透+1 数量+1",
                        8=>$"飞刀Lv{lv}：\n攻击力+3 数量+1",
                        9=>$"飞刀Lv{lv}：\n间隔-0.1s 数量+1",
                        10=>$"飞刀Lv{lv}：\n攻击力+3 数量+1",
                        _=>null
                    };
                })
                .WithDescriptionKey(lv =>
                {
                    return lv switch
                    {
                        1=>"exp_upgrade.simple_knife.lv1",
                        2=>"exp_upgrade.simple_knife.lv2",
                        3=>"exp_upgrade.simple_knife.lv3",
                        4=>"exp_upgrade.simple_knife.lv4",
                        5=>"exp_upgrade.simple_knife.lv5",
                        6=>"exp_upgrade.simple_knife.lv6",
                        7=>"exp_upgrade.simple_knife.lv7",
                        8=>"exp_upgrade.simple_knife.lv8",
                        9=>"exp_upgrade.simple_knife.lv9",
                        10=>"exp_upgrade.simple_knife.lv10",
                        _=>null
                    };
                })
                .OnUpgrade((_, level) =>
                {
                    switch (level)
                    {
                        case 1:
                            //解锁飞刀
                            Global.SimpleKnifeUnlocked.Value = true;
                            break;
                        case 2:
                            Global.SimpleKnifeDamage.Value += 3;
                            Global.SimpleKnifeCount.Value += 2;
                            break;
                        case 3:
                            Global.SimpleKnifeDuration.Value -= 0.1f;
                            Global.SimpleKnifeDamage.Value += 2;
                            Global.SimpleKnifeCount.Value += 1;
                            break;
                        case 4:
                            Global.SimpleKnifeDuration.Value -= 0.1f;
                            Global.SimpleKnifeAttackCount.Value += 1;
                            Global.SimpleKnifeCount.Value += 1;
                            break;
                        case 5:
                            Global.SimpleKnifeDamage.Value += 3;
                            Global.SimpleKnifeCount.Value += 1;
                            break;
                        case 6:
                            Global.SimpleKnifeDuration.Value -= 0.1f;
                            Global.SimpleKnifeCount.Value += 1;
                            break;
                        case 7:
                            Global.SimpleKnifeDuration.Value -= 0.1f;
                            Global.SimpleKnifeAttackCount.Value += 1;
                            Global.SimpleKnifeCount.Value += 1;
                            break;
                        case 8:
                            Global.SimpleKnifeDamage.Value += 3;
                            Global.SimpleKnifeCount.Value += 1;
                            break;
                        case 9:
                            Global.SimpleKnifeDuration.Value -= 0.1f;
                            Global.SimpleKnifeCount.Value += 1;
                            break;
                        case 10:
                            Global.SimpleKnifeDamage.Value += 3;
                            Global.SimpleKnifeCount.Value += 1;
                            break;
                    }
                }));

                Add(new ExpUpgradeItem(true)
                .WithKey("basket_ball")
                .WithName("篮球")
                .WithNameKey("exp_upgrade.basket_ball.name")
                .WithIconName("ball_icon")
                .WithPairedName("合成后的篮球")
                .WithPairedNameKey("exp_upgrade.basket_ball.paired_name")
                .WithPairedIconName("paired_ball_icon")
                .WithPairedDescription("攻击力翻倍 体型变大")
                .WithPairedDescriptionKey("exp_upgrade.basket_ball.paired_desc")
                .WithMaxLevel(10)
                .WithDescription(lv =>
                {
                    return lv switch
                    {
                        1=>$"篮球Lv{lv}:\n在屏幕内反弹的篮球",
                        2=>$"篮球Lv{lv}：\n攻击力+3",
                        3=>$"篮球Lv{lv}：\n数量+1",
                        4=>$"篮球Lv{lv}：\n攻击力+3",
                        5=>$"篮球Lv{lv}：\n数量+1",
                        6=>$"篮球Lv{lv}：\n攻击力+3",
                        7=>$"篮球Lv{lv}：\n速度+20%",
                        8=>$"篮球Lv{lv}：\n攻击力+3",
                        9=>$"篮球Lv{lv}：\n速度+20%",
                        10=>$"篮球Lv{lv}：\n数量+1",
                        _=>null
                    };
                })
                .WithDescriptionKey(lv =>
                {
                    return lv switch
                    {
                        1=>"exp_upgrade.basket_ball.lv1",
                        2=>"exp_upgrade.basket_ball.lv2",
                        3=>"exp_upgrade.basket_ball.lv3",
                        4=>"exp_upgrade.basket_ball.lv4",
                        5=>"exp_upgrade.basket_ball.lv5",
                        6=>"exp_upgrade.basket_ball.lv6",
                        7=>"exp_upgrade.basket_ball.lv7",
                        8=>"exp_upgrade.basket_ball.lv8",
                        9=>"exp_upgrade.basket_ball.lv9",
                        10=>"exp_upgrade.basket_ball.lv10",
                        _=>null
                    };
                })
                .OnUpgrade((_, level) =>
                {
                    switch (level)
                    {
                        case 1:
                            //解锁篮球
                            Global.BasketBallUnlocked.Value = true;
                            break;
                        case 2:
                            Global.BasketBallDamage.Value += 3;
                            break;
                        case 3:
                            Global.BasketBallCount.Value += 1;
                            break;
                        case 4:
                            Global.BasketBallDamage.Value += 3;
                            break;
                        case 5:
                            Global.BasketBallCount.Value += 1;
                            break;
                        case 6:
                            Global.BasketBallDamage.Value += 3;
                            break;
                        case 7:
                            Global.BasketBallSpeed.Value *= 1.2f;
                            break;
                        case 8:
                            Global.BasketBallDamage.Value += 3;
                            break;
                        case 9:
                            Global.BasketBallSpeed.Value *= 1.2f;
                            break;
                        case 10:
                            Global.BasketBallCount.Value += 1;
                            break;
                    }
                }));

                Add(new ExpUpgradeItem(true)
                .WithKey("simple_axe")
                .WithName("铜剑")
                .WithNameKey("exp_upgrade.simple_axe.name")
                .WithIconName("rpgItems_37")
                .WithPairedName("死亡旋风")
                .WithPairedNameKey("exp_upgrade.simple_axe.paired_name")
                .WithPairedIconName("rpgItems_38")
                .WithPairedDescription("360° 全屏穿透旋转投射物")
                .WithPairedDescriptionKey("exp_upgrade.simple_axe.paired_desc")
                .WithMaxLevel(10)
                .WithDescription(lv =>
                {
                    return lv switch
                    {
                        1=>$"铜剑Lv{lv}:\n向上抛出剑体，沿抛物线落下",
                        2=>$"铜剑Lv{lv}：\n攻击力+3",
                        3=>$"铜剑Lv{lv}：\n数量+1",
                        4=>$"铜剑Lv{lv}：\n攻击力+3 间隔-0.1s",
                        5=>$"铜剑Lv{lv}：\n穿透+1",
                        6=>$"铜剑Lv{lv}：\n攻击力+4 数量+1",
                        7=>$"铜剑Lv{lv}：\n间隔-0.2s",
                        8=>$"铜剑Lv{lv}：\n攻击力+5",
                        9=>$"铜剑Lv{lv}：\n数量+1 穿透+1",
                        10=>$"铜剑Lv{lv}：\n攻击力+5 间隔-0.2s",
                        _=>null
                    };
                })
                .WithDescriptionKey(lv =>
                {
                    return lv switch
                    {
                        1=>"exp_upgrade.simple_axe.lv1",
                        2=>"exp_upgrade.simple_axe.lv2",
                        3=>"exp_upgrade.simple_axe.lv3",
                        4=>"exp_upgrade.simple_axe.lv4",
                        5=>"exp_upgrade.simple_axe.lv5",
                        6=>"exp_upgrade.simple_axe.lv6",
                        7=>"exp_upgrade.simple_axe.lv7",
                        8=>"exp_upgrade.simple_axe.lv8",
                        9=>"exp_upgrade.simple_axe.lv9",
                        10=>"exp_upgrade.simple_axe.lv10",
                        _=>null
                    };
                })
                .OnUpgrade((_, level) =>
                {
                    switch (level)
                    {
                        case 1:
                            Global.SimpleAxeUnlocked.Value = true;
                            break;
                        case 2:
                            Global.SimpleAxeDamage.Value += 3f;
                            break;
                        case 3:
                            Global.SimpleAxeCount.Value += 1;
                            break;
                        case 4:
                            Global.SimpleAxeDamage.Value += 3f;
                            Global.SimpleAxeDuration.Value -= 0.1f;
                            break;
                        case 5:
                            Global.SimpleAxePierce.Value += 1;
                            break;
                        case 6:
                            Global.SimpleAxeDamage.Value += 4f;
                            Global.SimpleAxeCount.Value += 1;
                            break;
                        case 7:
                            Global.SimpleAxeDuration.Value -= 0.2f;
                            break;
                        case 8:
                            Global.SimpleAxeDamage.Value += 5f;
                            break;
                        case 9:
                            Global.SimpleAxeCount.Value += 1;
                            Global.SimpleAxePierce.Value += 1;
                            break;
                        case 10:
                            Global.SimpleAxeDamage.Value += 5f;
                            Global.SimpleAxeDuration.Value -= 0.2f;
                            break;
                    }

                    Global.SimpleAxeDuration.Value = UnityEngine.Mathf.Max(0.2f, Global.SimpleAxeDuration.Value);
                    Global.SimpleAxePierce.Value = UnityEngine.Mathf.Max(1, Global.SimpleAxePierce.Value);
                }));

                Add(new ExpUpgradeItem(false)
                .WithKey("simple_critical")
                .WithName("暴击")
                .WithNameKey("exp_upgrade.simple_critical.name")
                .WithIconName("critical_icon")
                .WithMaxLevel(5)
                .WithDescription(lv =>
                {
                    return lv switch
                    {
                        1=>$"暴击Lv{lv}:\n每次伤害15%概率暴击",
                        2=>$"暴击Lv{lv}：\n每次伤害28%概率暴击",
                        3=>$"暴击Lv{lv}：\n每次伤害40%概率暴击",
                        4=>$"暴击Lv{lv}：\n每次伤害52%概率暴击",
                        5=>$"暴击Lv{lv}：\n每次伤害65%概率暴击",
                        _=>null
                    };
                })
                .WithDescriptionKey(lv =>
                {
                    return lv switch
                    {
                        1=>"exp_upgrade.simple_critical.lv1",
                        2=>"exp_upgrade.simple_critical.lv2",
                        3=>"exp_upgrade.simple_critical.lv3",
                        4=>"exp_upgrade.simple_critical.lv4",
                        5=>"exp_upgrade.simple_critical.lv5",
                        _=>null
                    };
                })
                .OnUpgrade((_, level) =>
                {
                    switch (level)
                    {
                        case 1:
                            Global.CriticalRate.Value += 0.15f;
                            break;
                        case 2:
                            Global.CriticalRate.Value += 0.13f;
                            break;
                        case 3:
                            Global.CriticalRate.Value += 0.12f;
                            break;
                        case 4:
                            Global.CriticalRate.Value += 0.12f;
                            break;
                        case 5:
                            Global.CriticalRate.Value += 0.13f;
                            break;
                    }
                }));

                Add(new ExpUpgradeItem(false)
                .WithKey("damage_rate")
                .WithName("伤害倍率")
                .WithNameKey("exp_upgrade.damage_rate.name")
                .WithIconName("damage_icon")
                .WithMaxLevel(5)
                .WithDescription(lv =>
                {
                    return lv switch
                    {
                        1=>$"伤害倍率Lv{lv}:\n伤害提升15%",
                        2=>$"伤害倍率Lv{lv}:\n伤害提升28%",
                        3=>$"伤害倍率Lv{lv}:\n伤害提升40%",
                        4=>$"伤害倍率Lv{lv}:\n伤害提升52%",
                        5=>$"伤害倍率Lv{lv}:\n伤害提升65%",
                        _=>null
                    };
                })
                .WithDescriptionKey(lv =>
                {
                    return lv switch
                    {
                        1=>"exp_upgrade.damage_rate.lv1",
                        2=>"exp_upgrade.damage_rate.lv2",
                        3=>"exp_upgrade.damage_rate.lv3",
                        4=>"exp_upgrade.damage_rate.lv4",
                        5=>"exp_upgrade.damage_rate.lv5",
                        _=>null
                    };
                })
                .OnUpgrade((_, level) =>
                {
                    switch (level)
                    {
                        case 1:
                            Global.DamageRate.Value += 0.15f;
                            break;
                        case 2:
                            Global.DamageRate.Value += 0.13f;
                            break;
                        case 3:
                            Global.DamageRate.Value += 0.12f;
                            break;
                        case 4:
                            Global.DamageRate.Value += 0.12f;
                            break;
                        case 5:
                            Global.DamageRate.Value += 0.13f;
                            break;
                    }
                }));

                Add(new ExpUpgradeItem(false)
                .WithKey("simple_fly_count")
                .WithName("飞射物")
                .WithNameKey("exp_upgrade.simple_fly_count.name")
                .WithIconName("fly_icon")
                .WithMaxLevel(3)
                .WithDescription(lv =>
                {
                    return lv switch
                    {
                        1=>$"伤害倍率Lv{lv}:\n额外增加1个飞行物",
                        2=>$"伤害倍率Lv{lv}:\n额外增加2个飞行物",
                        3=>$"伤害倍率Lv{lv}:\n额外增加3个飞行物",
                        
                        _=>null
                    };
                })
                .WithDescriptionKey(lv =>
                {
                    return lv switch
                    {
                        1=>"exp_upgrade.simple_fly_count.lv1",
                        2=>"exp_upgrade.simple_fly_count.lv2",
                        3=>"exp_upgrade.simple_fly_count.lv3",
                        _=>null
                    };
                })
                .OnUpgrade((_, level) =>
                {
                    switch (level)
                    {
                        case 1:
                            Global.AdditionalFlyThingCount.Value += 1;
                            break;
                        case 2:
                            Global.AdditionalFlyThingCount.Value += 1;
                            break;
                        case 3:
                            Global.AdditionalFlyThingCount.Value += 1;
                            break;
                    }
                }));

                Add(new ExpUpgradeItem(false)
                .WithKey("movement_speed_rate")
                .WithName("移动速度")
                .WithNameKey("exp_upgrade.movement_speed_rate.name")
                .WithIconName("movement_icon")
                .WithMaxLevel(5)
                .WithDescription(lv =>
                {
                    return lv switch
                    {
                        1=>$"移动速度Lv{lv}:\n增加25%移动速度",
                        2=>$"移动速度Lv{lv}:\n增加50%移动速度",
                        3=>$"移动速度Lv{lv}:\n增加75%移动速度",
                        4=>$"移动速度Lv{lv}:\n增加100%移动速度",
                        5=>$"移动速度Lv{lv}:\n增加125%移动速度",
                        _=>null
                    };
                })
                .WithDescriptionKey(lv =>
                {
                    return lv switch
                    {
                        1=>"exp_upgrade.movement_speed_rate.lv1",
                        2=>"exp_upgrade.movement_speed_rate.lv2",
                        3=>"exp_upgrade.movement_speed_rate.lv3",
                        4=>"exp_upgrade.movement_speed_rate.lv4",
                        5=>"exp_upgrade.movement_speed_rate.lv5",
                        _=>null
                    };
                })
                .OnUpgrade((_, level) =>
                {
                    switch (level)
                    {
                        case 1:
                            Global.MovementSpeedRate.Value += 0.25f;
                            break;
                        case 2:
                            Global.MovementSpeedRate.Value += 0.25f;
                            break;
                        case 3:
                            Global.MovementSpeedRate.Value += 0.25f;
                            break;
                        case 4:
                            Global.MovementSpeedRate.Value += 0.25f;
                            break;
                        case 5:
                            Global.MovementSpeedRate.Value += 0.25f;
                            break;
                    }
                }));

                Add(new ExpUpgradeItem(false)
                .WithKey("simple_exp_percent")
                .WithName("经验值")
                .WithNameKey("exp_upgrade.simple_exp_percent.name")
                .WithIconName("exp_icon")
                .WithMaxLevel(5)
                .WithDescription(lv =>
                {
                    return lv switch
                    {
                        1=>$"经验值Lv{lv}:\n额外增加5%概率掉落",
                        2=>$"经验值Lv{lv}:\n额外增加10%概率掉落",
                        3=>$"经验值Lv{lv}:\n额外增加15%概率掉落",
                        4=>$"经验值Lv{lv}:\n额外增加20%概率掉落",
                        5=>$"经验值Lv{lv}:\n额外增加25%概率掉落",
                        _=>null
                    };
                })
                .WithDescriptionKey(lv =>
                {
                    return lv switch
                    {
                        1=>"exp_upgrade.simple_exp_percent.lv1",
                        2=>"exp_upgrade.simple_exp_percent.lv2",
                        3=>"exp_upgrade.simple_exp_percent.lv3",
                        4=>"exp_upgrade.simple_exp_percent.lv4",
                        5=>"exp_upgrade.simple_exp_percent.lv5",
                        _=>null
                    };
                })
                .OnUpgrade((_, level) =>
                {
                    switch (level)
                    {
                        case 1:
                            Global.AdditionalExpPercent.Value += 0.05f;
                            break;
                        case 2:
                            Global.AdditionalExpPercent.Value += 0.05f;
                            break;
                        case 3:
                            Global.AdditionalExpPercent.Value += 0.05f;
                            break;
                        case 4:
                            Global.AdditionalExpPercent.Value += 0.05f;
                            break;
                        case 5:
                            Global.AdditionalExpPercent.Value += 0.05f;
                            break;
                        
                    }
                }));

                Add(new ExpUpgradeItem(false)
                .WithKey("armor")
                .WithName("护甲")
                .WithNameKey("exp_upgrade.armor.name")
                .WithIconName("copper_helmet")
                .WithMaxLevel(5)
                .WithDescription(lv =>
                {
                    return lv switch
                    {
                        1=>$"护甲Lv{lv}:\n受到伤害-1",
                        2=>$"护甲Lv{lv}:\n受到伤害-2",
                        3=>$"护甲Lv{lv}:\n受到伤害-3",
                        4=>$"护甲Lv{lv}:\n受到伤害-4",
                        5=>$"护甲Lv{lv}:\n受到伤害-5",
                        _=>null
                    };
                })
                .WithDescriptionKey(lv =>
                {
                    return lv switch
                    {
                        1=>"exp_upgrade.armor.lv1",
                        2=>"exp_upgrade.armor.lv2",
                        3=>"exp_upgrade.armor.lv3",
                        4=>"exp_upgrade.armor.lv4",
                        5=>"exp_upgrade.armor.lv5",
                        _=>null
                    };
                })
                .OnUpgrade((_, _) =>
                {
                    Global.ArmorValue.Value += 1;
                }));

                Add(new ExpUpgradeItem(false)
                .WithKey("yellow_potion")
                .WithName("黄药水")
                .WithNameKey("exp_upgrade.yellow_potion.name")
                .WithIconName("rpgItems_3")
                .WithMaxLevel(5)
                .WithDescription(lv =>
                {
                    return lv switch
                    {
                        1=>$"黄药水Lv{lv}:\n武器范围+10%",
                        2=>$"黄药水Lv{lv}:\n武器范围+20%",
                        3=>$"黄药水Lv{lv}:\n武器范围+30%",
                        4=>$"黄药水Lv{lv}:\n武器范围+40%",
                        5=>$"黄药水Lv{lv}:\n武器范围+50%",
                        _=>null
                    };
                })
                .WithDescriptionKey(lv =>
                {
                    return lv switch
                    {
                        1=>"exp_upgrade.yellow_potion.lv1",
                        2=>"exp_upgrade.yellow_potion.lv2",
                        3=>"exp_upgrade.yellow_potion.lv3",
                        4=>"exp_upgrade.yellow_potion.lv4",
                        5=>"exp_upgrade.yellow_potion.lv5",
                        _=>null
                    };
                })
                .OnUpgrade((_, _) =>
                {
                    Global.AreaMultiplier.Value += 0.1f;
                }));

                Add(new ExpUpgradeItem(false)
                .WithKey("simple_collectable_area_radius")
                .WithName("拾取范围半径")
                .WithNameKey("exp_upgrade.simple_collectable_area_radius.name")
                .WithIconName("collectable_icon")
                .WithMaxLevel(3)
                .WithDescription(lv =>
                {
                    return lv switch
                    {
                        1=>$"拾取范围半径Lv{lv}:\n增加100%可收集物品范围半径",
                        2=>$"拾取范围半径Lv{lv}:\n增加200%可收集物品范围半径",
                        3=>$"拾取范围半径Lv{lv}:\n增加300%可收集物品范围半径",
                        _=>null
                    };
                })
                .WithDescriptionKey(lv =>
                {
                    return lv switch
                    {
                        1=>"exp_upgrade.simple_collectable_area_radius.lv1",
                        2=>"exp_upgrade.simple_collectable_area_radius.lv2",
                        3=>"exp_upgrade.simple_collectable_area_radius.lv3",
                        _=>null
                    };
                })
                .OnUpgrade((_, level) =>
                {
                    switch (level)
                    {
                        case 1:
                            Global.CollectableAreaRadius.Value += 1.0f;
                            break;
                        case 2:
                            Global.CollectableAreaRadius.Value += 1.0f;
                            break;
                        case 3:
                            Global.CollectableAreaRadius.Value += 1.0f;
                            break;
                    }
                }));

            Dictionary = Items.ToDictionary(i=>i.Key);
            
        }

        /// <summary>
        /// 随机抽取可升级项目
        /// </summary>
        /// <returns>是否有可升级项目</returns>
        public bool Roll()
        {
            foreach(var expUpgradeItem in Items)
            {
                expUpgradeItem.Visible.Value = false;
            }

            var list = Items.Where(item=>!item.UpgradeFinish).ToList();

            // 没有可升级项目
            if(list.Count == 0)
            {
                return false;
            }

            if(list.Count >= 4)
            {
                list.GetAndRemoveRandomItem().Visible.Value = true;
                list.GetAndRemoveRandomItem().Visible.Value = true;
                list.GetAndRemoveRandomItem().Visible.Value = true;
                list.GetAndRemoveRandomItem().Visible.Value = true;
            }
            else
            {
                foreach(var item in list)
                {
                    item.Visible.Value = true;
                }
            }

            return true;
        }
    }
}
