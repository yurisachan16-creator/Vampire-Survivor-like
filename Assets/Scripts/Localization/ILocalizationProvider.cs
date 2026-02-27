using System;
using System.Collections;
using System.Collections.Generic;

namespace VampireSurvivorLike
{
    public interface ILocalizationProvider
    {
        IEnumerator LoadManifest(Action<LocalizationManifest> onLoaded);
        IEnumerator LoadTable(string tableName, LanguageId language, Action<Dictionary<string, string>> onLoaded);
    }
}
