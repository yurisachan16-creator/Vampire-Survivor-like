using UnityEngine;
using QFramework;

namespace VampireSurvivorLike
{
	public partial class PowerUpManager : ViewController
	{
		public static PowerUpManager Default { get; private set; }

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

            // 确保道具模板不会在运行时被误吃掉/销毁
            PrepareSceneTemplate(Exp);
            PrepareSceneTemplate(Coin);
            PrepareSceneTemplate(RecoverHP);
            PrepareSceneTemplate(Bomb);
            PrepareSceneTemplate(GetAllExp);
            PrepareSceneTemplate(TreasureChest);
            PrepareSceneTemplate(SuperBomb);
        }

        void OnDestroy()
        {
            if (Default == this)
				Default = null;
        }
    }
}
