using System;

namespace VampireSurvivorLike
{
    public class DamageSystem 
    {
        public static void CalculateDamage(float baseDamage,IEnemy enemy,int maxNormalDamage=2,float criticalDamageTimes = 5)
        {
            if (enemy == null) return;

            baseDamage *= Global.DamageRate.Value; //应用伤害倍率
            
            if (UnityEngine.Random.Range(0, 1.0f) < Global.CriticalRate.Value)
            {
                //暴击
                var criticalMax = UnityEngine.Mathf.Max(2f, criticalDamageTimes);
                var damage = baseDamage + UnityEngine.Random.Range(2f, criticalMax);
                enemy.Hurt(UnityEngine.Mathf.Max(1f, damage), false, true);
                
            }
            else
            {
                var randomAdd = UnityEngine.Random.Range(0, maxNormalDamage + 1);
                var damage = baseDamage + randomAdd;
                enemy.Hurt(UnityEngine.Mathf.Max(1f, damage), false, false);
            }
            
        }
    }
}


