/****************************************************************************
 * Copyright (c) 2017 snowcold
 * Copyright (c) 2017 ~ 2023 liangxie
 * 
 * https://qframework.cn
 * https://github.com/liangxiegame/QFramework
 * https://gitee.com/liangxiegame/QFramework
 ****************************************************************************/

using System;
using System.Collections;
using System.IO;
using System.Linq;

namespace QFramework
{
    using System.Collections.Generic;
    using UnityEngine;

    [MonoSingletonPath("QFramework/ResKit/ResManager")]
    public class ResMgr : MonoBehaviour,ISingleton
    {
        public static ResMgr Instance => MonoSingletonProperty<ResMgr>.Instance;

        #region ID:RKRM001 Init v0.1.0 Unity5.5.1p4

        private static bool mResMgrInited = false;

        public static bool ResMgrInited => mResMgrInited;
        
        /// <summary>
        /// 初始化bin文件
        /// </summary>
        public static void Init()
        {
            if (mResMgrInited) return;
            mResMgrInited = true;

            SafeObjectPool<AssetBundleRes>.Instance.Init(40, 20);
            SafeObjectPool<AssetRes>.Instance.Init(40, 20);
            SafeObjectPool<ResourcesRes>.Instance.Init(40, 20);
            SafeObjectPool<NetImageRes>.Instance.Init(40, 20);
            SafeObjectPool<ResSearchKeys>.Instance.Init(40, 20);
            SafeObjectPool<ResLoader>.Instance.Init(40, 20);


            Instance.InitResMgr();
        }


        public static IEnumerator InitAsync()
        {
            if (mResMgrInited) yield break;
            mResMgrInited = true;

            SafeObjectPool<AssetBundleRes>.Instance.Init(40, 20);
            SafeObjectPool<AssetRes>.Instance.Init(40, 20);
            SafeObjectPool<ResourcesRes>.Instance.Init(40, 20);
            SafeObjectPool<NetImageRes>.Instance.Init(40, 20);
            SafeObjectPool<ResSearchKeys>.Instance.Init(40, 20);
            SafeObjectPool<ResLoader>.Instance.Init(40, 20);

            yield return Instance.InitResMgrAsync();
        }

        #endregion

        public int Count => Table.Count();

        public static bool IsApplicationQuit { get;private set; }

        private void OnApplicationQuit()
        {
            IsApplicationQuit = true;
        }

        #region 字段
        
        internal ResTable Table { get; } = new ResTable();

        [SerializeField] private int mCurrentCoroutineCount;
        private int mMaxCoroutineCount = 8; //最快协成大概在6到8之间
        private LinkedList<IEnumeratorTask> mIEnumeratorTaskStack = new LinkedList<IEnumeratorTask>();

        //Res 在ResMgr中 删除的问题，ResMgr定时收集列表中的Res然后删除
        private bool mIsResMapDirty;

        #endregion

        public IEnumerator InitResMgrAsync()
        {
            if (AssetBundlePathHelper.SimulationMode)
            {
                AssetBundleSettings.AssetBundleConfigFile = ConfigFileUtility.BuildEditorDataTable();
            }
            else
            {
                AssetBundleSettings.AssetBundleConfigFile.Reset();

                var configPaths = CollectAssetConfigPaths(true);
                foreach (var outRes in configPaths)
                {
                    Debug.Log(outRes);
                    yield return AssetBundleSettings.AssetBundleConfigFile.LoadFromFileAsync(outRes);
                }
            }

            yield return null;
        }

        public void InitResMgr()
        {
            if (AssetBundlePathHelper.SimulationMode)
            {
                AssetBundleSettings.AssetBundleConfigFile = ConfigFileUtility.BuildEditorDataTable();
            }
            else
            {
#if UNITY_WEBGL
                LogKit.E("WebGL 请使用异步初始化: ResKit.InitAsync. Please use async init api: ResKit.InitAsync in WebGL Platform");
#endif
                AssetBundleSettings.AssetBundleConfigFile.Reset();

                var configPaths = CollectAssetConfigPaths(false);
                foreach (var outRes in configPaths)
                {
                    AssetBundleSettings.AssetBundleConfigFile.LoadFromFile(outRes);
                }
            }
        }

        private static List<string> CollectAssetConfigPaths(bool includePathPrefix)
        {
            var configCandidates = new List<string>();
            var platformName = AssetBundlePathHelper.GetPlatformName();

            if (AssetBundleSettings.LoadAssetResFromStreamingAssetsPath)
            {
                configCandidates.Add(
                    $"{Application.streamingAssetsPath}/AssetBundles/{platformName}/{ResDatas.FileName}");
#if UNITY_EDITOR
                TryAddEditorPlatformFallback(configCandidates, Application.streamingAssetsPath, platformName);
#endif
            }
            else
            {
                configCandidates.Add(
                    $"{Application.persistentDataPath}/AssetBundles/{platformName}/{ResDatas.FileName}");
#if UNITY_EDITOR
                TryAddEditorPlatformFallback(configCandidates, Application.persistentDataPath, platformName);
#endif
            }

            var availablePaths = new List<string>();
            foreach (var path in configCandidates)
            {
                if (File.Exists(path))
                {
                    availablePaths.Add(includePathPrefix ? AssetBundlePathHelper.PathPrefix + path : path);
                }
            }

            if (availablePaths.Count > 0)
            {
                return availablePaths;
            }

            var fallbackPath = configCandidates[0];
            LogKit.E($"Res config not found on disk, fallback to expected path: {fallbackPath}");
            availablePaths.Add(includePathPrefix ? AssetBundlePathHelper.PathPrefix + fallbackPath : fallbackPath);
            return availablePaths;
        }

#if UNITY_EDITOR
        private static void TryAddEditorPlatformFallback(
            List<string> configCandidates,
            string rootPath,
            string activePlatformName)
        {
            var editorPlatformName = AssetBundlePathHelper.GetPlatformForAssetBundles(Application.platform);
            if (string.Equals(editorPlatformName, activePlatformName, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            configCandidates.Add($"{rootPath}/AssetBundles/{editorPlatformName}/{ResDatas.FileName}");
        }
#endif

        #region 属性

        public void ClearOnUpdate()
        {
            mIsResMapDirty = true;
        }

        public void PushIEnumeratorTask(IEnumeratorTask task)
        {
            if (task == null)
            {
                return;
            }

            mIEnumeratorTaskStack.AddLast(task);
            TryStartNextIEnumeratorTask();
        }


        public IRes GetRes(ResSearchKeys resSearchKeys, bool createNew = false)
        {
            var res = Table.GetResBySearchKeys(resSearchKeys);

            if (res != null)
            {
                return res;
            }

            if (!createNew)
            {
                Debug.LogFormat("createNew:{0}", createNew);
                return null;
            }

            res = ResFactory.Create(resSearchKeys);

            if (res != null)
            {
                Table.Add(res);
            }

            return res;
        }

        public T GetRes<T>(ResSearchKeys resSearchKeys) where T : class, IRes
        {
            return GetRes(resSearchKeys) as T;
        }

        #endregion

        #region Private Func

        private void Update()
        {
            if (mIsResMapDirty)
            {
                RemoveUnusedRes();
            }
        }

        private void RemoveUnusedRes()
        {
            if (!mIsResMapDirty)
            {
                return;
            }

            mIsResMapDirty = false;

            foreach (var res in Table.ToArray())
            {
                if (res.RefCount <= 0 && res.State != ResState.Loading)
                {
                    if (res.ReleaseRes())
                    {
                        Table.Remove(res);
                        
                        res.Recycle2Cache();
                    }
                }
            }
        }
        

        private void OnIEnumeratorTaskFinish()
        {
            --mCurrentCoroutineCount;
            TryStartNextIEnumeratorTask();
        }

        private void TryStartNextIEnumeratorTask()
        {
            if (mIEnumeratorTaskStack.Count == 0)
            {
                return;
            }

            if (mCurrentCoroutineCount >= mMaxCoroutineCount)
            {
                return;
            }

            var task = mIEnumeratorTaskStack.First.Value;
            mIEnumeratorTaskStack.RemoveFirst();

            ++mCurrentCoroutineCount;
            StartCoroutine(task.DoLoadAsync(OnIEnumeratorTaskFinish));
        }

        #endregion
        
        public void OnSingletonInit()
        {
            
        }
    }
}
