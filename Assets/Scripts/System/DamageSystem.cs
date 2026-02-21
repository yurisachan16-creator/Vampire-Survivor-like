using UnityEngine;

namespace VampireSurvivorLike
{
    public class DamageSystem 
    {
        public static void CalculateDamage(float baseDamage,IEnemy enemy,int maxNormalDamage=2,float criticalDamageTimes = 5)
        {
            if (enemy == null) return;

            var lemonBonus = Mathf.Max(0f, Global.LemonDamageBuffBonus.Value);
            baseDamage *= Global.DamageRate.Value * (1f + lemonBonus); //应用伤害倍率与柠檬增伤
            
            if (UnityEngine.Random.Range(0, 1.0f) < Global.CriticalRate.Value)
            {
                //暴击
                var criticalMax = Mathf.Max(2f, criticalDamageTimes);
                var damage = baseDamage + UnityEngine.Random.Range(2f, criticalMax);
                enemy.Hurt(Mathf.Max(1f, damage), false, true);
                
            }
            else
            {
                var randomAdd = UnityEngine.Random.Range(0, maxNormalDamage + 1);
                var damage = baseDamage + randomAdd;
                enemy.Hurt(Mathf.Max(1f, damage), false, false);
            }
            
        }
    }
}


