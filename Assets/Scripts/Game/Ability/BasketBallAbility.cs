using UnityEngine;
using QFramework;
using System.Collections.Generic;
using System.Threading;

namespace VampireSurvivorLike
{
	public partial class BasketBallAbility : ViewController
	{
		private List<Ball> _mBasketBalls = new List<Ball>();
		void Start()
		{
            Global.BasketBallCount.RegisterWithInitValue(count =>
            {
                if (_mBasketBalls.Count < count)
                {
                    _mBasketBalls.Add(Ball.Instantiate()
						.SyncPosition2DFrom(this)
						.Show());
                }
            }).UnRegisterWhenGameObjectDestroyed(gameObject);
		}
	}
}
