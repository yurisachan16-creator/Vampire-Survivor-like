using System;
using System.Collections.Generic;

namespace VampireSurvivorLike
{
    public static class AgentMemory
    {
        private static readonly object SyncRoot = new object();
        private static IAgentMemoryStore _store;

        private static IAgentMemoryStore Store
        {
            get
            {
                lock (SyncRoot)
                {
                    if (_store == null)
                    {
                        _store = new FileAgentMemoryStore();
                    }

                    return _store;
                }
            }
        }

        public static void ConfigureStore(IAgentMemoryStore store)
        {
            if (store == null)
            {
                throw new ArgumentNullException(nameof(store));
            }

            lock (SyncRoot)
            {
                _store = store;
            }
        }

        public static void ResetDefaultStore()
        {
            lock (SyncRoot)
            {
                _store = null;
            }
        }

        public static void Set(string key, string value, MemoryScope scope, string sourceAgent, DateTime? expiresAtUtc = null)
        {
            Store.Set(key, value, scope, sourceAgent, expiresAtUtc);
        }

        public static bool TryGet(string key, MemoryScope scope, out string value)
        {
            return Store.TryGet(key, scope, out value);
        }

        public static string GetOrDefault(string key, MemoryScope scope, string defaultValue = "")
        {
            return Store.GetOrDefault(key, scope, defaultValue);
        }

        public static string AddKnowledge(
            string title,
            string content,
            IReadOnlyList<string> tags,
            MemoryScope scope,
            string sourceAgent,
            DateTime? expiresAtUtc = null,
            float importance = 0.5f)
        {
            return Store.AddKnowledge(title, content, tags, scope, sourceAgent, expiresAtUtc, importance);
        }

        public static IReadOnlyList<MemoryQueryResult> Search(MemoryQuery query)
        {
            return Store.Search(query);
        }

        public static bool DeleteById(string id)
        {
            return Store.DeleteById(id);
        }

        public static int PruneExpired(DateTime nowUtc)
        {
            return Store.PruneExpired(nowUtc);
        }

        public static void ExportSnapshot(string outputPath)
        {
            Store.ExportSnapshot(outputPath);
        }
    }
}
