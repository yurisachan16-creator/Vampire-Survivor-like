
using UnityEngine;
using QFramework;

namespace VampireSurvivorLike
{
    public abstract class PowerUp : GameplayObject
    {
        //吃掉exp物体时，exp不是直接消失，而是会飞向玩家
        public bool FlyingToPalyer { get; set; }
        private int _flyingToPlayerFrameCount = 0;

        protected abstract void Execute();

        private void Update()
        {
            //飞向玩家的动画逻辑
            if (FlyingToPalyer)
            {
                if(_flyingToPlayerFrameCount == 0)
                {
                    GetComponent<SpriteRenderer>().sortingOrder = 5; //确保在最前面显示
                }

                _flyingToPlayerFrameCount++;

                if (Player.Default)
                {
                    var direction = Player.Default.DirectionFrom(this);
                    var distance = direction.magnitude;

                    if(_flyingToPlayerFrameCount <= 15)
                    {
                        transform.Translate(direction.normalized * -2 * Time.deltaTime);
                    }
                    else
                    {
                        transform.Translate(direction.normalized * 7.5f * Time.deltaTime);
                    }

                    if(distance < 0.1f)
                    {
                        if (Global.IsGameOver.Value)
                        {
                            this.DestroyGameObjGracefully();
                            return;
                        }
                        Execute();
                    }
                }
            }
        }

    }

}


