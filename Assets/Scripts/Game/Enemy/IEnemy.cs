namespace VampireSurvivorLike
{
    public interface IEnemy
    {
        void Hurt(float value,bool force=false,bool critical=false);
        void SetSpeedScale(float SpeedScale);
        void SetHPScale(float HPScale);
        void SetDamageScale(float DamageScale);
    }
}
