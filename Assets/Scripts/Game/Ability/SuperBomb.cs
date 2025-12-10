using UnityEngine;
using QFramework;

namespace VampireSurvivorLike
{
	public partial class SuperBomb : ViewController
	{
		private float _mCurrentSecond=0;

        void Update()
        {
            _mCurrentSecond+=Time.deltaTime;
            if (_mCurrentSecond >= 15)
            {
                _mCurrentSecond=0;
				Bomb.Execute();
            }
        }
    }
}
