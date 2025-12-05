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
            var simpleDamageLv1 = Add(new ExpUpgradeItem()
            .WithKey("simple_damage")
            .WithDescription(lv => $"简单攻击基础伤害提升 Lv{lv}")
            .WithMaxLevel(10)
            .OnUpgrade((_,level)=>
            {
                if (level == 1)
                {
                    //TODO:
                }
                Global.SimpleAbilityDamage.Value *= 1.5f;
            }));

            

            var simpleDurationLv1 = Add(new ExpUpgradeItem()
            .WithKey("simple_duration")
            .WithMaxLevel(10)
            .WithDescription(lv=>$"简单攻击的间隔时间减少 Lv{lv}")
            .OnUpgrade((_,level)=>
            {
                if (level == 2)
                {
                    //TODO:
                }
                Global.SimpleAbilityDuration.Value *= 0.8f;
            }));

            
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
