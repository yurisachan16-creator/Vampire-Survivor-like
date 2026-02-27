using System;
using System.Collections.Generic;

namespace VampireSurvivorLike
{
    public interface IAgentMemoryStore
    {
        void Set(string key, string value, MemoryScope scope, string sourceAgent, DateTime? expiresAtUtc = null);
        bool TryGet(string key, MemoryScope scope, out string value);
        string GetOrDefault(string key, MemoryScope scope, string defaultValue = "");
        string AddKnowledge(string title, string content, IReadOnlyList<string> tags, MemoryScope scope, string sourceAgent, DateTime? expiresAtUtc = null, float importance = 0.5f);
        IReadOnlyList<MemoryQueryResult> Search(MemoryQuery query);
        bool DeleteById(string id);
        int PruneExpired(DateTime nowUtc);
        void ExportSnapshot(string outputPath);
    }
}
