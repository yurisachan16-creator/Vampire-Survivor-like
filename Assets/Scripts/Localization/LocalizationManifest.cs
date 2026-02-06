using System;
using System.Collections.Generic;

namespace VampireSurvivorLike
{
    [Serializable]
    public class LocalizationManifest
    {
        public int Version = 1;
        public List<string> Languages = new List<string>();
        public List<string> Tables = new List<string>();
    }
}
