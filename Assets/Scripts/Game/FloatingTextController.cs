using UnityEngine;
using QFramework;
using UnityEngine.UI;

namespace VampireSurvivorLike
{
	public partial class FloatingTextController : ViewController
	{
		void Start()
		{
			FloatingText.Hide();
		}

		public static void Play(Vector2 position,string text,bool critical=false)
        {
			if (!_mDefault || !_mDefault.FloatingText) return;

			var go = ObjectPoolSystem.Spawn(_mDefault.FloatingText.gameObject, _mDefault.transform, true);
			if (!go) return;
			go.transform.position = position;

			var item = go.GetComponent<FloatingTextItem>();
			if (!item) item = go.AddComponent<FloatingTextItem>();
			item.Play(position, text, critical);
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
