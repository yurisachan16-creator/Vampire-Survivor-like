using System;

namespace VampireSurvivorLike
{
    public class DamageSystem 
    {
        public static void CalculateDamage(float baseDamage,IEnemy enemy,int maxNormalDamage=2,float criticalDamageTimes = 5)
        {

            baseDamage *= Global.DamageRate.Value; //应用伤害倍率
            
            if (UnityEngine.Random.Range(0, 1.0f) < Global.CriticalRate.Value)
            {
                //暴击
                enemy.Hurt((baseDamage + UnityEngine.Random.Range(2f, criticalDamageTimes)), false, true);
                
            }
            else
            {
                enemy.Hurt(baseDamage+UnityEngine.Random.Range(-1, maxNormalDamage), false, false);   //需要伤害有一定的随机范围
            }
            
        }
    }
}


