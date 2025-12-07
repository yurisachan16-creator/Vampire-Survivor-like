namespace VampireSurvivorLike
{
    public interface IEnemy
    {
        void Hurt(float value,bool force=false);
        void SetSpeedScale(float SpeedScale);
        void SetHPScale(float HPScale);
    }
}
