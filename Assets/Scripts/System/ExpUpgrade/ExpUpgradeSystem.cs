using System.Collections.Generic;
using System.Linq;
using QFramework;

namespace VampireSurvivorLike
{
    public class ExpUpgradeSystem : AbstractSystem
    {
        public static EasyEvent OnExpUpgradeSystemChanged = new EasyEvent();
        public List<ExpUpgradeItem> Items{get;} = new List<ExpUpgradeItem>();

        public ExpUpgradeItem Add(ExpUpgradeItem item)
        {
            Items.Add(item);
            return item;
        }
        protected override void OnInit()
        {
            ResetData();

            Global.Level.Register(_=>
            {
                Roll();
            });
        }

        public void ResetData()
        {
            Items.Clear();

            Add(new ExpUpgradeItem()
                .WithKey("simple_sword")
                .WithMaxLevel(10)
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
                .WithMaxLevel(10)
                .OnUpgrade((_, level) =>
                {
                    switch (level)
                    {
                        case 1:
                            //Global.
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

            Add(new ExpUpgradeItem()
                .WithKey("rotate_sword")
                .WithMaxLevel(10)
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
                .WithMaxLevel(10)
                .OnUpgrade((_, level) =>
                {
                    switch (level)
                    {
                        case 1:
                            //Global.
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
           
            Add(new ExpUpgradeItem()
                .WithKey("simple_knife")
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
                .WithMaxLevel(10)
                .OnUpgrade((_, level) =>
                {
                    switch (level)
                    {
                        case 1:
                            //Global.
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

                Add(new ExpUpgradeItem()
                .WithKey("baket_ball")
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
                .WithMaxLevel(10)
                .OnUpgrade((_, level) =>
                {
                    switch (level)
                    {
                        case 1:
                            //Global.
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
            
        }

        public void Roll()
        {
            foreach(var expUpgradeItem in Items)
            {
                expUpgradeItem.Visible.Value = false;
            }

            foreach(var item in Items.Where(item=>!item.UpgradeFinish).Take(3))
            {
                if(item == null)
                {
                    
                }

                else
                {
                    item.Visible.Value = true;
                }
            }
            

            
        }
    }
}
