using UnityEngine;
using QFramework;
using System.Collections.Generic;
using System.Threading;
using System;

namespace VampireSurvivorLike
{
	public partial class BasketBallAbility : ViewController
	{
		private List<Ball> _mBasketBalls = new List<Ball>();
		void Start()
		{
			void CreateBall()
            {
                _mBasketBalls.Add(Ball.Instantiate()
						.SyncPosition2DFrom(this)
						.Show());
            }

			void CreateBalls()
            {
                var ballCount2Create = Global.BasketBallCount.Value + Global.AdditionalFlyThingCount.Value - _mBasketBalls.Count;

				for(int i = 0; i < ballCount2Create; i++)
                {
                    CreateBall();
                }
            }

            Global.BasketBallCount.Or(Global.AdditionalFlyThingCount).Register(() =>
            {
				CreateBalls();
                        
            }).UnRegisterWhenGameObjectDestroyed(gameObject);

			CreateBalls();
		}
	}
}
