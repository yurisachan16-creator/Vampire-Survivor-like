using UnityEngine;
using QFramework;
using System.Collections.Generic;

namespace VampireSurvivorLike
{
	public partial class BasketBallAbility : ViewController
	{
		private readonly List<Ball> _mBasketBalls = new List<Ball>(8);
		private const float EnsureIntervalSeconds = 0.5f;
		private float _nextEnsureTime;

		void Start()
		{
            Global.BasketBallCount.Or(Global.AdditionalFlyThingCount).Register(() =>
            {
				EnsureBallCount();
            }).UnRegisterWhenGameObjectDestroyed(gameObject);

			EnsureBallCount();
		}

		private void Update()
		{
			if (Time.unscaledTime < _nextEnsureTime) return;

			_nextEnsureTime = Time.unscaledTime + EnsureIntervalSeconds;
			EnsureBallCount();
		}

		private void EnsureBallCount()
		{
			for (var i = _mBasketBalls.Count - 1; i >= 0; i--)
			{
				var ball = _mBasketBalls[i];
				if (!ball || !ball.gameObject.activeInHierarchy)
				{
					_mBasketBalls.RemoveAt(i);
				}
			}

			var targetCount = Mathf.Max(0, Global.BasketBallCount.Value + Global.AdditionalFlyThingCount.Value);
			var needCreate = targetCount - _mBasketBalls.Count;

			for (var i = 0; i < needCreate; i++)
			{
				CreateBall();
			}
		}

		private void CreateBall()
		{
			var ball = Ball.Instantiate()
				.SyncPosition2DFrom(this)
				.Show();

			if (ball)
			{
				_mBasketBalls.Add(ball);
			}
		}
	}
}
