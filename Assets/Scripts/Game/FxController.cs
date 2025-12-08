 using UnityEngine;
using QFramework;
using UnityEngine.UIElements;

namespace VampireSurvivorLike
{
	public partial class FxController : ViewController
	{
		private static FxController _mDefault = null;

        void Awake()
        {
            _mDefault = this;
        }


        void OnDestroy()
        {
            if (_mDefault == this)
			{
				_mDefault = null;
			}
        }

		public static void Play(SpriteRenderer sprite,Color dissolveColor)
        {
            _mDefault.EnemyDieFx.Instantiate()
			.Position(sprite.Position())
			.LocalScale(sprite.Scale())
            .Self(s =>
            {
                s.GetComponent<Dissolve>().DissovleColor = dissolveColor;
				s.sprite= sprite.sprite;
            })
			.Show();

        }
    }	
}
