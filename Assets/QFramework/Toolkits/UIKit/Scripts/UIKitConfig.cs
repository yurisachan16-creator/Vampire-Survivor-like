/****************************************************************************
 * Copyright (c) 2017 ~ 2020.1 liangxie
 * 
 * http://qframework.io
 * https://github.com/liangxiegame/QFramework
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;

namespace QFramework
{
    public class UIKitConfig
    {
        public virtual UIRoot Root
        {
            get { return UIRoot.Instance; }
        }
        
        public virtual IPanel LoadPanel(PanelSearchKeys panelSearchKeys)
        {
            var panelLoader = PanelLoaderPool.AllocateLoader();
            GameObject instance = null;
            
            try
            {
                // panelLoader.LoadPanelPrefab 感谢 NormalKatt、高跟鞋提供为反馈
                var panelPrefab = panelLoader.LoadPanelPrefab(panelSearchKeys);
                return CreatePanelInstance(panelSearchKeys, panelLoader, panelPrefab, ref instance);
            }
            catch
            {
                ReleaseFailedPanelLoad(panelLoader, instance);
                throw;
            }
        }

        private IPanel CreatePanelInstance(
            PanelSearchKeys panelSearchKeys,
            IPanelLoader panelLoader,
            GameObject panelPrefab,
            ref GameObject instance)
        {
            if (!panelPrefab)
            {
                throw new InvalidOperationException(BuildPanelLoadFailureMessage(
                    panelSearchKeys,
                    null,
                    null,
                    "Panel loader returned a null prefab."));
            }

            instance = Object.Instantiate(panelPrefab);
            var retScript = instance.GetComponent<UIPanel>();
            if (!retScript)
            {
                throw new InvalidOperationException(BuildPanelLoadFailureMessage(
                    panelSearchKeys,
                    panelPrefab,
                    instance,
                    "Loaded prefab does not contain a UIPanel component."));
            }

            if (!(retScript is IPanel panelInterface))
            {
                throw new InvalidOperationException(BuildPanelLoadFailureMessage(
                    panelSearchKeys,
                    panelPrefab,
                    instance,
                    "Loaded UIPanel does not implement IPanel."));
            }

            panelInterface.Loader = panelLoader;
            instance = null;
            return retScript;
        }


        public virtual void LoadPanelAsync(PanelSearchKeys panelSearchKeys, Action<IPanel> onPanelLoad)
        {
            var panelLoader = PanelLoaderPool.AllocateLoader();

            panelLoader.LoadPanelPrefabAsync(panelSearchKeys, (panelPrefab) =>
            {
                GameObject instance = null;
                try
                {
                    var panel = CreatePanelInstance(panelSearchKeys, panelLoader, panelPrefab, ref instance);
                    onPanelLoad?.Invoke(panel);
                }
                catch
                {
                    ReleaseFailedPanelLoad(panelLoader, instance);
                    throw;
                }
            });
        }

        private void ReleaseFailedPanelLoad(IPanelLoader panelLoader, GameObject instance)
        {
            if (instance)
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(instance);
                }
                else
                {
                    Object.DestroyImmediate(instance);
                }
            }

            panelLoader?.Unload();
            if (panelLoader != null)
            {
                PanelLoaderPool.RecycleLoader(panelLoader);
            }
        }

        private static string BuildPanelLoadFailureMessage(
            PanelSearchKeys panelSearchKeys,
            GameObject panelPrefab,
            GameObject instance,
            string failureReason)
        {
            var messageBuilder = new StringBuilder(256);
            messageBuilder.Append("[UIKit] Failed to load panel. ")
                .Append(failureReason)
                .Append(" SearchKeys: ")
                .Append(panelSearchKeys)
                .Append(". SimulationMode: ")
                .Append(GetSimulationModeDescription())
                .Append('.');

            if (panelPrefab)
            {
                messageBuilder.Append(" Prefab: ").Append(panelPrefab.name).Append('.');
            }

            var missingScriptCount = CountMissingScripts(instance);
            if (missingScriptCount > 0)
            {
                messageBuilder.Append(" The instantiated prefab contains ")
                    .Append(missingScriptCount)
                    .Append(" missing script reference(s), which usually means the AssetBundle is stale relative to the current assemblies.");
            }

            return messageBuilder.ToString();
        }

        private static int CountMissingScripts(GameObject instance)
        {
            if (!instance)
            {
                return 0;
            }

            var missingScriptCount = 0;
            var components = instance.GetComponentsInChildren<Component>(true);
            for (var i = 0; i < components.Length; i++)
            {
                if (!components[i])
                {
                    missingScriptCount++;
                }
            }

            return missingScriptCount;
        }

        private static string GetSimulationModeDescription()
        {
            var helperType = Type.GetType("QFramework.AssetBundlePathHelper, ResKit");
            var propertyInfo = helperType?.GetProperty("SimulationMode");
            if (propertyInfo == null)
            {
                return "unknown";
            }

            try
            {
                var value = propertyInfo.GetValue(null, null);
                return value?.ToString() ?? "unknown";
            }
            catch
            {
                return "unknown";
            }
        }


        public IPanelLoaderPool PanelLoaderPool = new DefaultPanelLoaderPool();

        public virtual void SetDefaultSizeOfPanel(IPanel panel)
        {
            var panelRectTrans = panel.Transform as RectTransform;

            panelRectTrans.offsetMin = Vector2.zero;
            panelRectTrans.offsetMax = Vector2.zero;
            panelRectTrans.anchoredPosition3D = Vector3.zero;
            panelRectTrans.anchorMin = Vector2.zero;
            panelRectTrans.anchorMax = Vector2.one;

            panelRectTrans.localScale = Vector3.one;
        }
    }

    /// <summary>
    /// 如果想要定制自己的加载器，自定义 IPanelLoader 以及
    /// </summary>
    public interface IPanelLoader
    {
        GameObject LoadPanelPrefab(PanelSearchKeys panelSearchKeys);

        void LoadPanelPrefabAsync(PanelSearchKeys panelSearchKeys, Action<GameObject> onPanelPrefabLoad);

        void Unload();
    }


    public interface IPanelLoaderPool
    {
        IPanelLoader AllocateLoader();
        void RecycleLoader(IPanelLoader panelLoader);
    }

    public abstract class AbstractPanelLoaderPool : IPanelLoaderPool
    {
        private Stack<IPanelLoader> mPool = new Stack<IPanelLoader>(16);

        public IPanelLoader AllocateLoader()
        {
            return mPool.Count > 0 ? mPool.Pop() : CreatePanelLoader();
        }

        protected abstract IPanelLoader CreatePanelLoader();

        public void RecycleLoader(IPanelLoader panelLoader)
        {
            mPool.Push(panelLoader);
        }
    }

    public class DefaultPanelLoaderPool : AbstractPanelLoaderPool
    {
        /// <summary>
        /// Default
        /// </summary>
        public class DefaultPanelLoader : IPanelLoader
        {
            private GameObject mPanelPrefab;

            public GameObject LoadPanelPrefab(PanelSearchKeys panelSearchKeys)
            {
                mPanelPrefab = Resources.Load<GameObject>(panelSearchKeys.GameObjName);
                return mPanelPrefab;
            }

            public void LoadPanelPrefabAsync(PanelSearchKeys panelSearchKeys, Action<GameObject> onPanelLoad)
            {
                var request = Resources.LoadAsync<GameObject>(panelSearchKeys.GameObjName);

                request.completed += operation => { onPanelLoad(request.asset as GameObject); };
            }

            public void Unload()
            {
                mPanelPrefab = null;
            }
        }

        protected override IPanelLoader CreatePanelLoader()
        {
            return new DefaultPanelLoader();
        }
    }
}
