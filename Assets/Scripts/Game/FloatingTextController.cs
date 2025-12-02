using UnityEngine;
using QFramework;
using UnityEngine.UI;
using UnityEditor;

namespace VampireSurvivorLike
{
	public partial class FloatingTextController : ViewController
	{
		void Start()
		{
			FloatingText.Hide();
		}

		public static void Play(Vector2 position,string text)
        {
            _mDefault.FloatingText.InstantiateWithParent(_mDefault.transform)
				.PositionX(position.x)
				.PositionY(position.y)
            .Self(f =>
			{
				var positionY = position.y;
                var textTransform = f.transform.Find("Text");
				var textComp = textTransform.GetComponent<Text>();
				textComp.text = text;

				//动画效果
				ActionKit.Sequence().Lerp(0,0.5f,0.5f)
					.Lerp(0, 0.5f,0.5f, (p) =>
					{
						f.PositionY(positionY + p*0.25f);	//上浮0.25单位
						textComp.LocalScaleX(Mathf.Clamp01(p * 4));
						textComp.LocalScaleY(Mathf.Clamp01(p * 4));

					})
					.Delay(0.5f)
					.Lerp(1.0f,0,0.3f,(p)=>
					{
						textComp.ColorAlpha(p);
					},()=>{f.DestroyGameObjGracefully();})
					.Start(textComp);

            }).Show();
        }

		private static FloatingTextController _mDefault;

        void Awake()
        {
            _mDefault = this;
        }

        void OnDestroy()
        {
            _mDefault = null;
        }
    }
}
