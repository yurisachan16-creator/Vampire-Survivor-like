using System;
using System.Collections;
using System.Collections.Generic;
using QFramework;
using UnityEngine;
using UnityEngine.U2D;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VampireSurvivorLike
{
    public static class LocalizationBootstrap
    {
        private struct PendingAtlasRequest
        {
            public string AtlasName;
            public Action<SpriteAtlas> Callback;
        }

        private static bool _subscribed;
        private static bool _resKitInitStarted;
        private static bool _resKitReady;
        private static ResLoader _atlasLoader;

        private static readonly Dictionary<string, SpriteAtlas> AtlasCache =
            new Dictionary<string, SpriteAtlas>(StringComparer.OrdinalIgnoreCase);

        private static readonly List<PendingAtlasRequest> PendingRequests = new List<PendingAtlasRequest>(16);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            EnsureResKitInitialized();
            EnsureSpriteAtlasRequestedHook();
            LocalizationManager.Initialize();
        }

        private static void EnsureResKitInitialized()
        {
            if (_resKitInitStarted) return;
            _resKitInitStarted = true;

#if UNITY_WEBGL && !UNITY_EDITOR
            LocalizationRunner.Instance.StartCoroutine(InitResKitAsync());
#else
            ResKit.Init();
            _resKitReady = true;
#endif
        }

        private static IEnumerator InitResKitAsync()
        {
            yield return ResKit.InitAsync();
            _resKitReady = true;
            FlushPendingAtlasRequests();
        }

        private static void EnsureSpriteAtlasRequestedHook()
        {
            if (_subscribed) return;
            _subscribed = true;

            _atlasLoader = ResLoader.Allocate();
            SpriteAtlasManager.atlasRequested += OnAtlasRequested;
        }

        private static void OnAtlasRequested(string atlasName, Action<SpriteAtlas> callback)
        {
            if (string.IsNullOrWhiteSpace(atlasName))
            {
                callback?.Invoke(null);
                return;
            }

            if (AtlasCache.TryGetValue(atlasName, out var cached) && cached)
            {
                callback?.Invoke(cached);
                return;
            }

            if (!_resKitReady && !Application.isEditor)
            {
                PendingRequests.Add(new PendingAtlasRequest { AtlasName = atlasName, Callback = callback });
                return;
            }

            var atlas = LoadAtlasNow(atlasName);
            if (atlas) AtlasCache[atlasName] = atlas;
            callback?.Invoke(atlas);
        }

        private static SpriteAtlas LoadAtlasNow(string atlasName)
        {
#if UNITY_EDITOR
            if (Application.isEditor)
            {
                var atlas = LoadEditorAtlasByName(atlasName);
                if (atlas) return atlas;
            }
#endif
            return _atlasLoader != null ? _atlasLoader.LoadSync<SpriteAtlas>(atlasName) : null;
        }

#if UNITY_EDITOR
        private static SpriteAtlas LoadEditorAtlasByName(string atlasName)
        {
            if (atlasName.Equals("Enemy", StringComparison.OrdinalIgnoreCase))
            {
                return AssetDatabase.LoadAssetAtPath<SpriteAtlas>("Assets/Art/Sprite/Enemy/Enemy.spriteatlasv2");
            }

            if (atlasName.Equals("Icon", StringComparison.OrdinalIgnoreCase))
            {
                return AssetDatabase.LoadAssetAtPath<SpriteAtlas>("Assets/Art/Sprite/UI/Icon/Icon.spriteatlasv2");
            }

            return null;
        }
#endif

        private static void FlushPendingAtlasRequests()
        {
            if (PendingRequests.Count == 0) return;
            for (var i = 0; i < PendingRequests.Count; i++)
            {
                var req = PendingRequests[i];
                var atlas = LoadAtlasNow(req.AtlasName);
                if (atlas) AtlasCache[req.AtlasName] = atlas;
                req.Callback?.Invoke(atlas);
            }
            PendingRequests.Clear();
        }
    }
}
