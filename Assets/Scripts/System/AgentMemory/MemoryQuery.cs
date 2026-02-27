using System;

namespace VampireSurvivorLike
{
    [Serializable]
    public class MemoryQuery
    {
        public string text;
        public string[] tags;
        public MemoryScope scope;
        public int limit = 20;
        public bool includeExpired;

        public MemoryQuery()
        {
            tags = Array.Empty<string>();
            scope = MemoryScope.Empty;
        }
    }
}
