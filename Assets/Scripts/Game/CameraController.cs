using UnityEngine;
using QFramework;
using Unity.VisualScripting;

namespace VampireSurvivorLike
{
	public partial class CameraController : ViewController
	{
		private Vector2 _mTargetPosition = Vector2.zero;

		private static CameraController _mDefault;

        void Awake()
        {
            _mDefault = this;
        }

        void Start()
        {
			//强制设置为60帧
            Application.targetFrameRate = 60;
        }

		private Vector3 _mCurrentCameraPos;
		private bool _mShake=false;
		private int _mShakeFrameCount=0;
		private float _mShakeA=4.0f;	//振幅

		public static void ShakeCamera()
        {
            _mDefault._mShake=true;
			_mDefault._mShakeFrameCount=30;
			_mDefault._mShakeA=0.25f;
        }

        void Update()
        {
            if (Player.Default)
            {
                _mTargetPosition = Player.Default.transform.position;
				_mCurrentCameraPos.x = (1.0f-Mathf.Exp(-Time.deltaTime*20)).Lerp(transform.position.x,_mTargetPosition.x);
				_mCurrentCameraPos.y = (1.0f-Mathf.Exp(-Time.deltaTime*20)).Lerp(transform.position.y,_mTargetPosition.y);
				_mCurrentCameraPos.z = transform.position.z;
				

				if (_mShake)
				{
					_mShakeFrameCount--;
					if(_mShakeFrameCount%3==0)
					{
						//振幅
						var shakeA=Mathf.Lerp(_mShakeA,0f,(_mShakeFrameCount/30f));
						
						_mCurrentCameraPos= new Vector3(_mCurrentCameraPos.x + Random.Range(-shakeA,shakeA),
						_mCurrentCameraPos.y + Random.Range(-shakeA,shakeA),_mCurrentCameraPos.z);
	
					}

                    if (_mShakeFrameCount <= 0)
                    {
                        _mShake = false;
                    }
            	}

				
                transform.position = _mCurrentCameraPos;
			}

        }

		private void OnDestroy() {
			_mDefault = null;
		}
    }
}
