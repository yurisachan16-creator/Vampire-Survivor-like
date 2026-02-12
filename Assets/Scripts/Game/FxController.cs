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
			if (!_mDefault || !_mDefault.EnemyDieFx || !sprite) return;

			var go = ObjectPoolSystem.Spawn(_mDefault.EnemyDieFx.gameObject, null, true);
			if (!go) return;

			go.transform.position = sprite.Position();
			go.transform.localScale = sprite.Scale();

			var sr = go.GetComponent<SpriteRenderer>();
			if (sr) sr.sprite = sprite.sprite;

			var dissolve = go.GetComponent<Dissolve>();
			if (dissolve) dissolve.DissovleColor = dissolveColor;

        }
    }	
}
