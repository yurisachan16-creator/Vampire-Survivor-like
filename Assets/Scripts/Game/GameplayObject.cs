using QFramework;
using UnityEngine;

namespace VampireSurvivorLike
{
    public abstract class GameplayObject : ViewController
    {
        protected abstract Collider2D Collider2D { get; }
        /// <summary>
        /// 当物体进入摄像机视野时启用碰撞器，离开视野时禁用碰撞器
        /// </summary>
        private void OnBecameVisible()
        {
            Collider2D.enabled = true;
        }

        /// <summary>
        /// 当物体离开摄像机视野时禁用碰撞器
        /// </summary>
        private void OnBecameInvisible()
        {
            Collider2D.enabled = false;
        }
    }

}
