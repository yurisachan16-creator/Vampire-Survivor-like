using System.Collections.Generic;
using UnityEngine;

namespace VampireSurvivorLike
{
    public static class ObjectPoolSystem
    {
        private static readonly Dictionary<int, Stack<GameObject>> PoolsByPrefabId = new Dictionary<int, Stack<GameObject>>(128);
        private static Transform _poolRoot;

        public static GameObject Spawn(GameObject prefab, Transform parent = null, bool activate = true)
        {
            if (!prefab) return null;

            EnsureRoot();
            var prefabId = prefab.GetInstanceID();

            if (!PoolsByPrefabId.TryGetValue(prefabId, out var pool))
            {
                pool = new Stack<GameObject>(32);
                PoolsByPrefabId.Add(prefabId, pool);
            }

            GameObject go = null;
            while (pool.Count > 0 && !go) go = pool.Pop();

            PooledObjectTag tag;
            if (!go)
            {
                go = Object.Instantiate(prefab);
                tag = go.GetComponent<PooledObjectTag>();
                if (!tag) tag = go.AddComponent<PooledObjectTag>();
            }
            else
            {
                tag = go.GetComponent<PooledObjectTag>();
                if (!tag) tag = go.AddComponent<PooledObjectTag>();
            }

            tag.PrefabId = prefabId;
            tag.EnsurePoolablesCached();

            if (go.activeSelf) go.SetActive(false);
            go.transform.SetParent(parent, false);
            go.SetActive(activate);

            tag.InvokeOnSpawned();

            return go;
        }

        public static void Despawn(GameObject go)
        {
            if (!go) return;
            var tag = go.GetComponent<PooledObjectTag>();
            if (!tag)
            {
                Object.Destroy(go);
                return;
            }

            if (!PoolsByPrefabId.TryGetValue(tag.PrefabId, out var pool))
            {
                pool = new Stack<GameObject>(32);
                PoolsByPrefabId.Add(tag.PrefabId, pool);
            }

            tag.EnsurePoolablesCached();
            tag.InvokeOnDespawned();

            EnsureRoot();
            if (go.activeSelf) go.SetActive(false);
            go.transform.SetParent(_poolRoot, false);
            pool.Push(go);
        }

        /// <summary>
        /// 清空所有对象池，避免快速重新进入 Game 场景时，回收的敌人/投射物短暂残留或出现在错误位置
        /// </summary>
        public static void ClearAll()
        {
            foreach (var kv in PoolsByPrefabId)
            {
                var pool = kv.Value;
                while (pool.Count > 0)
                {
                    var go = pool.Pop();
                    if (go) Object.Destroy(go);
                }
            }
            PoolsByPrefabId.Clear();
        }

        public static void RefreshPoolableCache(GameObject go)
        {
            if (!go) return;
            var tag = go.GetComponent<PooledObjectTag>();
            if (!tag) return;
            tag.RefreshPoolablesCache();
        }

        private static void EnsureRoot()
        {
            if (_poolRoot) return;
            var rootGo = new GameObject("ObjectPool");
            Object.DontDestroyOnLoad(rootGo);
            _poolRoot = rootGo.transform;
        }

        public interface IPoolable
        {
            void OnSpawned();
            void OnDespawned();
        }

        private sealed class PooledObjectTag : MonoBehaviour
        {
            public int PrefabId;
            private IPoolable[] _poolables;
            private bool _poolablesCached;

            public void EnsurePoolablesCached()
            {
                if (_poolablesCached) return;
                RefreshPoolablesCache();
            }

            public void RefreshPoolablesCache()
            {
                _poolables = GetComponents<IPoolable>();
                _poolablesCached = true;
            }

            public void InvokeOnSpawned()
            {
                EnsurePoolablesCached();
                if (_poolables == null) return;
                for (var i = 0; i < _poolables.Length; i++) _poolables[i].OnSpawned();
            }

            public void InvokeOnDespawned()
            {
                EnsurePoolablesCached();
                if (_poolables == null) return;
                for (var i = 0; i < _poolables.Length; i++) _poolables[i].OnDespawned();
            }
        }
    }
}
