using UnityEngine;
using QFramework;
using UnityEngine.U2D;

namespace VampireSurvivorLike
{
	public partial class PowerUpManager : ViewController
	{
		public static PowerUpManager Default { get; private set; }
        private CircleCollider2D _wineTemplate;
        private CircleCollider2D _lemonBuffTemplate;
        private ResLoader _resLoader;
        private SpriteAtlas _iconAtlas;

        private static void PrepareSceneTemplate(Object template)
        {
            if (!template) return;

            GameObject go = template as GameObject;
            if (!go)
            {
                if (template is Component c) go = c.gameObject;
            }

            if (!go) return;

            // 只处理“场景里的模板对象”（避免误改 prefab 资源）
            if (!go.scene.IsValid()) return;

            // 避免被玩家碰到/触发导致 Destroy，从而把引用打成 MissingReference
            go.SetActive(false);
            foreach (var col in go.GetComponentsInChildren<Collider2D>(true))
            {
                col.enabled = false;
            }
            foreach (var rb in go.GetComponentsInChildren<Rigidbody2D>(true))
            {
                rb.simulated = false;
            }
        }

        void Awake()
        {
            Default = this;
            _resLoader = ResLoader.Allocate();
            _iconAtlas = _resLoader.LoadSync<SpriteAtlas>("icon");

            // 确保道具模板不会在运行时被误吃掉/销毁
            PrepareSceneTemplate(Exp);
            PrepareSceneTemplate(Coin);
            PrepareSceneTemplate(RecoverHP);
            PrepareSceneTemplate(Bomb);
            PrepareSceneTemplate(GetAllExp);
            PrepareSceneTemplate(TreasureChest);
            PrepareSceneTemplate(SuperBomb);

            PrepareSceneTemplate(EnsureTemplate<Wine>(ref _wineTemplate, "WineTemplate", "rpgItems_32"));
            PrepareSceneTemplate(EnsureTemplate<LemonBuff>(ref _lemonBuffTemplate, "LemonBuffTemplate", "rpgItems_19"));
        }

        private CircleCollider2D EnsureTemplate<T>(ref CircleCollider2D cache, string templateName, string spriteName) where T : PowerUp
        {
            if (cache) return cache;
            if (!RecoverHP) return null;

            var templateGo = Object.Instantiate(RecoverHP.gameObject, transform);
            templateGo.name = templateName;

            var recoverHp = templateGo.GetComponent<RecoverHP>();
            if (recoverHp) recoverHp.enabled = false;

            if (!templateGo.GetComponent<T>())
            {
                templateGo.AddComponent<T>();
            }

            var sr = templateGo.GetComponent<SpriteRenderer>();
            if (sr && _iconAtlas)
            {
                var sprite = _iconAtlas.GetSprite(spriteName);
                if (sprite) sr.sprite = sprite;
            }

            cache = templateGo.GetComponent<CircleCollider2D>();
            return cache;
        }

        public void SpawnWine(Vector3 worldPosition)
        {
            var template = EnsureTemplate<Wine>(ref _wineTemplate, "WineTemplate", "rpgItems_32");
            if (!template) return;

            template.Instantiate()
                .Position(worldPosition)
                .Show();
        }

        public void SpawnLemonBuff(Vector3 worldPosition)
        {
            var template = EnsureTemplate<LemonBuff>(ref _lemonBuffTemplate, "LemonBuffTemplate", "rpgItems_19");
            if (!template) return;

            template.Instantiate()
                .Position(worldPosition)
                .Show();
        }

        void OnDestroy()
        {
            if (Default == this)
				Default = null;

            if (_resLoader != null)
            {
                _resLoader.Recycle2Cache();
                _resLoader = null;
            }
        }
    }
}
