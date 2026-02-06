using System.Collections.Generic;

namespace VampireSurvivorLike
{
    public interface ILocalizedAssetResolver
    {
        IEnumerable<string> GetCandidates(string baseKey, LanguageId language);
    }
}
