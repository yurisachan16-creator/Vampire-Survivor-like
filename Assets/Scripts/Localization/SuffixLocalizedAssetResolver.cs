using System.Collections.Generic;

namespace VampireSurvivorLike
{
    public sealed class SuffixLocalizedAssetResolver : ILocalizedAssetResolver
    {
        public IEnumerable<string> GetCandidates(string baseKey, LanguageId language)
        {
            if (string.IsNullOrWhiteSpace(baseKey)) yield break;
            var code = language.IsEmpty ? string.Empty : language.ToString();
            if (!string.IsNullOrWhiteSpace(code))
            {
                var safe = code.Replace("-", "_");
                yield return $"{baseKey}_{code}";
                if (safe != code) yield return $"{baseKey}_{safe}";
            }
            yield return baseKey;
        }
    }
}
