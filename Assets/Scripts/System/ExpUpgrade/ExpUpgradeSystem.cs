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
           

            
        }

        public void Roll()
        {
            foreach(var expUpgradeItem in Items)
            {
                expUpgradeItem.Visible.Value = false;
            }

            var item = Items.Where(item=>!item.UpgradeFinish).ToList().GetRandomItem();

            if(item==null)
            {
                
            }

            else
            {
                item.Visible.Value = true;
            }
        }
    }
}
