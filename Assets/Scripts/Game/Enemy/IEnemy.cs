namespace VampireSurvivorLike
{
    public interface IEnemy
    {
        void Hurt(float value,bool force=false,bool critical=false);
        void SetSpeedScale(float SpeedScale);
        void SetHPScale(float HPScale);
        void SetDamageScale(float DamageScale);
        void SetBaseSpeed(float baseSpeed);
        void SetDropRates(float expRate, float coinRate, float hpRate, float bombRate);
        void SetTreasureChest(bool isTreasureChest);
        void ApplySlow(float multiplier, float durationSeconds);
    }
}
