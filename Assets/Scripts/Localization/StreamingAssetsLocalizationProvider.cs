using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VampireSurvivorLike
{
    public sealed class StreamingAssetsLocalizationProvider : ILocalizationProvider
    {
        private const string ManifestPath = "Localization/manifest.json";

        public IEnumerator LoadManifest(Action<LocalizationManifest> onLoaded)
        {
            string json = null;

            #if UNITY_WEBGL && !UNITY_EDITOR
            yield return LocalizationFileLoader.LoadStreamingAssetsTextAsync(ManifestPath, t => json = t);
            #else
            json = LocalizationFileLoader.LoadStreamingAssetsTextSync(ManifestPath);
            yield return null;
            #endif

            if (string.IsNullOrWhiteSpace(json))
            {
                onLoaded?.Invoke(null);
                yield break;
            }

            LocalizationManifest manifest = null;
            try
            {
                manifest = JsonUtility.FromJson<LocalizationManifest>(json);
            }
            catch
            {
                manifest = null;
            }

            onLoaded?.Invoke(manifest);
        }

        public IEnumerator LoadTable(string tableName, LanguageId language, Action<Dictionary<string, string>> onLoaded)
        {
            var langCode = language.IsEmpty ? LanguageId.ZhHans.ToString() : language.ToString();
            var relative = $"Localization/{tableName}.{langCode}.csv";
            string csv = null;

            #if UNITY_WEBGL && !UNITY_EDITOR
            yield return LocalizationFileLoader.LoadStreamingAssetsTextAsync(relative, t => csv = t);
            #else
            csv = LocalizationFileLoader.LoadStreamingAssetsTextSync(relative);
            yield return null;
            #endif

            var dict = LocalizationCsv.ParseKeyValueTable(csv);
            onLoaded?.Invoke(dict);
        }
    }
}
