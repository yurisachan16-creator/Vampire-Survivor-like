using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace VampireSurvivorLike
{
    public static class LocalizationFileLoader
    {
        public static IEnumerator LoadStreamingAssetsTextAsync(string relativePath, Action<string> onComplete)
        {
            var fullPath = Path.Combine(Application.streamingAssetsPath, relativePath);

            using (var request = UnityWebRequest.Get(fullPath))
            {
                yield return request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    onComplete?.Invoke(string.Empty);
                    yield break;
                }

                onComplete?.Invoke(request.downloadHandler.text ?? string.Empty);
            }
        }

        public static string LoadStreamingAssetsTextSync(string relativePath)
        {
            var fullPath = Path.Combine(Application.streamingAssetsPath, relativePath);
            if (!File.Exists(fullPath)) return string.Empty;
            return File.ReadAllText(fullPath, Encoding.UTF8);
        }
    }
}
