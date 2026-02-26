using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace VampireSurvivorLike
{
    public sealed class FileAgentMemoryStore : IAgentMemoryStore
    {
        private const int SchemaVersion = 1;
        private const string TypeKeyValue = "KeyValue";
        private const string TypeKnowledge = "Knowledge";

        private readonly string _rootDirectory;
        private readonly string _kvPath;
        private readonly string _knowledgePath;
        private readonly string _indexPath;
        private readonly string _lockPath;

        public string RootDirectory => _rootDirectory;

        [Serializable]
        private sealed class MemoryKvDocument
        {
            public int version = SchemaVersion;
            public List<MemoryEntry> entries = new List<MemoryEntry>();
        }

        [Serializable]
        private sealed class MemoryIndexDocument
        {
            public int version = SchemaVersion;
            public string updatedAtUtc;
            public int keyValueCount;
            public int knowledgeCount;
        }

        [Serializable]
        private sealed class MemorySnapshotDocument
        {
            public int version = SchemaVersion;
            public string exportedAtUtc;
            public List<MemoryEntry> keyValues;
            public List<MemoryEntry> knowledge;
        }

        public FileAgentMemoryStore(string rootDirectory = null)
        {
            _rootDirectory = string.IsNullOrWhiteSpace(rootDirectory)
                ? Path.Combine(Application.streamingAssetsPath, "Memory")
                : rootDirectory;

            _kvPath = Path.Combine(_rootDirectory, "memory_kv.json");
            _knowledgePath = Path.Combine(_rootDirectory, "memory_knowledge.jsonl");
            _indexPath = Path.Combine(_rootDirectory, "memory_index.json");
            _lockPath = Path.Combine(_rootDirectory, "memory.lock");
        }

        public void Set(string key, string value, MemoryScope scope, string sourceAgent, DateTime? expiresAtUtc = null)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Key cannot be null or whitespace.", nameof(key));
            }

            var nowUtc = DateTime.UtcNow;
            var normalizedScope = scope.Normalized();
            var normalizedKey = key.Trim();

            WithWriteLock(() =>
            {
                var kvDoc = LoadKvDocumentSafe();
                var existing = kvDoc.entries.FirstOrDefault(e =>
                    !e.deleted &&
                    string.Equals(e.type, TypeKeyValue, StringComparison.Ordinal) &&
                    string.Equals(e.key, normalizedKey, StringComparison.OrdinalIgnoreCase) &&
                    e.GetScope().Equals(normalizedScope));

                if (existing == null)
                {
                    existing = CreateEntryBase(MemoryEntryType.KeyValue, normalizedScope, sourceAgent, nowUtc, expiresAtUtc);
                    existing.key = normalizedKey;
                    kvDoc.entries.Add(existing);
                }

                existing.value = value ?? string.Empty;
                existing.updatedAtUtc = MemoryEntry.ToIsoUtc(nowUtc);
                existing.expiresAtUtc = expiresAtUtc.HasValue ? MemoryEntry.ToIsoUtc(expiresAtUtc.Value) : string.Empty;
                existing.deleted = false;

                SaveKvDocument(kvDoc);
                SaveIndex(nowUtc, kvDoc, LoadKnowledgeEntriesSafe());
            });
        }

        public bool TryGet(string key, MemoryScope scope, out string value)
        {
            value = string.Empty;
            if (string.IsNullOrWhiteSpace(key))
            {
                return false;
            }

            var nowUtc = DateTime.UtcNow;
            var normalizedScope = scope.Normalized();
            var normalizedKey = key.Trim();
            var kvDoc = LoadKvDocumentSafe();

            MemoryEntry candidate = null;
            foreach (var entry in kvDoc.entries)
            {
                if (entry == null || entry.deleted) continue;
                if (!string.Equals(entry.type, TypeKeyValue, StringComparison.Ordinal)) continue;
                if (entry.IsExpired(nowUtc)) continue;
                if (!string.Equals(entry.key, normalizedKey, StringComparison.OrdinalIgnoreCase)) continue;
                if (!entry.GetScope().Equals(normalizedScope)) continue;

                if (candidate == null || CompareUpdatedAt(entry, candidate) > 0)
                {
                    candidate = entry;
                }
            }

            if (candidate == null)
            {
                return false;
            }

            value = candidate.value ?? string.Empty;
            return true;
        }

        public string GetOrDefault(string key, MemoryScope scope, string defaultValue = "")
        {
            string value;
            return TryGet(key, scope, out value) ? value : defaultValue;
        }

        public string AddKnowledge(
            string title,
            string content,
            IReadOnlyList<string> tags,
            MemoryScope scope,
            string sourceAgent,
            DateTime? expiresAtUtc = null,
            float importance = 0.5f)
        {
            var nowUtc = DateTime.UtcNow;
            var normalizedScope = scope.Normalized();
            var entry = CreateEntryBase(MemoryEntryType.Knowledge, normalizedScope, sourceAgent, nowUtc, expiresAtUtc);
            entry.title = title ?? string.Empty;
            entry.content = content ?? string.Empty;
            entry.tags = NormalizeTags(tags);
            entry.importance = Mathf.Clamp01(importance);

            WithWriteLock(() =>
            {
                AppendKnowledgeLine(entry);
                SaveIndex(nowUtc, LoadKvDocumentSafe(), LoadKnowledgeEntriesSafe());
            });

            return entry.id;
        }

        public IReadOnlyList<MemoryQueryResult> Search(MemoryQuery query)
        {
            var q = query ?? new MemoryQuery();
            var nowUtc = DateTime.UtcNow;
            var normalizedFilterScope = q.scope.Normalized();
            var normalizedText = NormalizeSearchText(q.text);
            var requiredTags = NormalizeTags(q.tags);
            var limit = q.limit > 0 ? q.limit : 20;

            var candidates = new List<MemoryEntry>();
            candidates.AddRange(LoadKvDocumentSafe().entries);
            candidates.AddRange(LoadKnowledgeEntriesSafe());

            var scored = new List<MemoryQueryResult>();
            foreach (var entry in candidates)
            {
                if (entry == null || entry.deleted) continue;
                if (!q.includeExpired && entry.IsExpired(nowUtc)) continue;
                if (!entry.GetScope().MatchesFilter(normalizedFilterScope)) continue;

                float score;
                if (!TryScoreEntry(entry, normalizedText, requiredTags, out score)) continue;

                scored.Add(new MemoryQueryResult
                {
                    entry = entry,
                    score = score
                });
            }

            scored.Sort((a, b) =>
            {
                var scoreCompare = b.score.CompareTo(a.score);
                if (scoreCompare != 0) return scoreCompare;
                return CompareUpdatedAt(b.entry, a.entry);
            });

            if (scored.Count > limit)
            {
                scored.RemoveRange(limit, scored.Count - limit);
            }

            return scored;
        }

        public bool DeleteById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return false;
            }

            var nowUtc = DateTime.UtcNow;
            var normalizedId = id.Trim();

            return WithWriteLock(() =>
            {
                var kvDoc = LoadKvDocumentSafe();
                var kvEntry = kvDoc.entries.FirstOrDefault(e => e != null && string.Equals(e.id, normalizedId, StringComparison.OrdinalIgnoreCase));
                if (kvEntry != null)
                {
                    kvEntry.deleted = true;
                    kvEntry.updatedAtUtc = MemoryEntry.ToIsoUtc(nowUtc);
                    SaveKvDocument(kvDoc);
                    SaveIndex(nowUtc, kvDoc, LoadKnowledgeEntriesSafe());
                    return true;
                }

                var knowledgeEntries = LoadKnowledgeEntriesSafe();
                var existing = knowledgeEntries.FirstOrDefault(e => e != null && string.Equals(e.id, normalizedId, StringComparison.OrdinalIgnoreCase));
                if (existing == null)
                {
                    return false;
                }

                var tombstone = CloneEntry(existing);
                tombstone.deleted = true;
                tombstone.updatedAtUtc = MemoryEntry.ToIsoUtc(nowUtc);
                AppendKnowledgeLine(tombstone);
                SaveIndex(nowUtc, kvDoc, LoadKnowledgeEntriesSafe());
                return true;
            });
        }

        public int PruneExpired(DateTime nowUtc)
        {
            var utcNow = nowUtc.ToUniversalTime();

            return WithWriteLock(() =>
            {
                var removed = 0;

                var kvDoc = LoadKvDocumentSafe();
                removed += kvDoc.entries.RemoveAll(e => e != null && e.IsExpired(utcNow));
                SaveKvDocument(kvDoc);

                var knowledge = LoadKnowledgeEntriesSafe();
                var filteredKnowledge = knowledge.Where(e => e != null && !e.IsExpired(utcNow)).ToList();
                removed += knowledge.Count - filteredKnowledge.Count;
                SaveKnowledgeEntries(filteredKnowledge);

                SaveIndex(utcNow, kvDoc, filteredKnowledge);
                return removed;
            });
        }

        public void ExportSnapshot(string outputPath)
        {
            if (string.IsNullOrWhiteSpace(outputPath))
            {
                throw new ArgumentException("Output path cannot be null or whitespace.", nameof(outputPath));
            }

            var nowUtc = DateTime.UtcNow;
            var keyValues = LoadKvDocumentSafe().entries
                .Where(e => e != null && !e.deleted && !e.IsExpired(nowUtc))
                .ToList();
            var knowledge = LoadKnowledgeEntriesSafe()
                .Where(e => e != null && !e.deleted && !e.IsExpired(nowUtc))
                .ToList();

            var snapshot = new MemorySnapshotDocument
            {
                version = SchemaVersion,
                exportedAtUtc = MemoryEntry.ToIsoUtc(nowUtc),
                keyValues = keyValues,
                knowledge = knowledge
            };

            var json = JsonUtility.ToJson(snapshot, true);
            AtomicWriteAllText(outputPath, json);
        }

        private static MemoryEntry CreateEntryBase(
            MemoryEntryType type,
            MemoryScope scope,
            string sourceAgent,
            DateTime nowUtc,
            DateTime? expiresAtUtc)
        {
            var entry = new MemoryEntry
            {
                id = Guid.NewGuid().ToString("N"),
                project = scope.project,
                agent = scope.agent,
                channel = scope.channel,
                key = string.Empty,
                value = string.Empty,
                title = string.Empty,
                content = string.Empty,
                tags = Array.Empty<string>(),
                sourceAgent = MemoryScope.NormalizePart(sourceAgent),
                importance = 0.5f,
                createdAtUtc = MemoryEntry.ToIsoUtc(nowUtc),
                updatedAtUtc = MemoryEntry.ToIsoUtc(nowUtc),
                expiresAtUtc = expiresAtUtc.HasValue ? MemoryEntry.ToIsoUtc(expiresAtUtc.Value) : string.Empty,
                deleted = false
            };
            entry.SetEntryType(type);
            return entry;
        }

        private MemoryKvDocument LoadKvDocumentSafe()
        {
            EnsureStorageRoot();

            if (!File.Exists(_kvPath))
            {
                return new MemoryKvDocument();
            }

            try
            {
                var content = File.ReadAllText(_kvPath, Encoding.UTF8);
                if (string.IsNullOrWhiteSpace(content))
                {
                    return new MemoryKvDocument();
                }

                var doc = JsonUtility.FromJson<MemoryKvDocument>(content);
                if (doc == null || doc.entries == null)
                {
                    throw new InvalidDataException("Invalid key-value document.");
                }

                foreach (var entry in doc.entries)
                {
                    NormalizeEntry(entry);
                }

                return doc;
            }
            catch (Exception ex)
            {
                RecoverCorruptedFile(_kvPath, ex);
                var empty = new MemoryKvDocument();
                SaveKvDocument(empty);
                return empty;
            }
        }

        private List<MemoryEntry> LoadKnowledgeEntriesSafe()
        {
            EnsureStorageRoot();

            if (!File.Exists(_knowledgePath))
            {
                return new List<MemoryEntry>();
            }

            try
            {
                var lines = File.ReadAllLines(_knowledgePath, Encoding.UTF8);
                var latestById = new Dictionary<string, MemoryEntry>(StringComparer.OrdinalIgnoreCase);

                for (var i = 0; i < lines.Length; i++)
                {
                    var line = lines[i].Trim();
                    if (string.IsNullOrEmpty(line)) continue;

                    var entry = JsonUtility.FromJson<MemoryEntry>(line);
                    if (entry == null || string.IsNullOrWhiteSpace(entry.id))
                    {
                        throw new InvalidDataException("Invalid knowledge entry line.");
                    }

                    NormalizeEntry(entry);
                    latestById[entry.id] = entry;
                }

                return latestById.Values.ToList();
            }
            catch (Exception ex)
            {
                RecoverCorruptedFile(_knowledgePath, ex);
                var empty = new List<MemoryEntry>();
                SaveKnowledgeEntries(empty);
                return empty;
            }
        }

        private void SaveKvDocument(MemoryKvDocument doc)
        {
            EnsureStorageRoot();
            if (doc == null)
            {
                doc = new MemoryKvDocument();
            }

            if (doc.entries == null)
            {
                doc.entries = new List<MemoryEntry>();
            }

            for (var i = 0; i < doc.entries.Count; i++)
            {
                NormalizeEntry(doc.entries[i]);
            }

            var json = JsonUtility.ToJson(doc, true);
            AtomicWriteAllText(_kvPath, json);
        }

        private void SaveKnowledgeEntries(List<MemoryEntry> entries)
        {
            EnsureStorageRoot();
            var safeEntries = entries ?? new List<MemoryEntry>();
            var builder = new StringBuilder(safeEntries.Count * 256);

            for (var i = 0; i < safeEntries.Count; i++)
            {
                var entry = safeEntries[i];
                if (entry == null) continue;
                NormalizeEntry(entry);
                builder.Append(JsonUtility.ToJson(entry)).Append('\n');
            }

            AtomicWriteAllText(_knowledgePath, builder.ToString());
        }

        private void AppendKnowledgeLine(MemoryEntry entry)
        {
            EnsureStorageRoot();
            NormalizeEntry(entry);

            var line = JsonUtility.ToJson(entry) + Environment.NewLine;
            if (!File.Exists(_knowledgePath))
            {
                File.WriteAllText(_knowledgePath, line, Encoding.UTF8);
                return;
            }

            File.AppendAllText(_knowledgePath, line, Encoding.UTF8);
        }

        private void SaveIndex(DateTime nowUtc, MemoryKvDocument kvDoc, List<MemoryEntry> knowledge)
        {
            EnsureStorageRoot();

            var keyValueCount = kvDoc.entries.Count(e => e != null && !e.deleted && !e.IsExpired(nowUtc));
            var knowledgeCount = knowledge.Count(e => e != null && !e.deleted && !e.IsExpired(nowUtc));

            var index = new MemoryIndexDocument
            {
                version = SchemaVersion,
                updatedAtUtc = MemoryEntry.ToIsoUtc(nowUtc),
                keyValueCount = keyValueCount,
                knowledgeCount = knowledgeCount
            };

            var json = JsonUtility.ToJson(index, true);
            AtomicWriteAllText(_indexPath, json);
        }

        private static void NormalizeEntry(MemoryEntry entry)
        {
            if (entry == null) return;

            if (string.IsNullOrWhiteSpace(entry.id))
            {
                entry.id = Guid.NewGuid().ToString("N");
            }

            var type = MemoryScope.NormalizePart(entry.type);
            if (type == "keyvalue")
            {
                entry.type = TypeKeyValue;
            }
            else if (type == "knowledge")
            {
                entry.type = TypeKnowledge;
            }
            else
            {
                entry.type = TypeKnowledge;
            }

            entry.project = MemoryScope.NormalizePart(entry.project);
            entry.agent = MemoryScope.NormalizePart(entry.agent);
            entry.channel = MemoryScope.NormalizePart(entry.channel);
            entry.key = entry.key == null ? string.Empty : entry.key.Trim();
            entry.value = entry.value ?? string.Empty;
            entry.title = entry.title ?? string.Empty;
            entry.content = entry.content ?? string.Empty;
            entry.tags = NormalizeTags(entry.tags);
            entry.sourceAgent = MemoryScope.NormalizePart(entry.sourceAgent);
            entry.importance = Mathf.Clamp01(entry.importance);

            if (string.IsNullOrWhiteSpace(entry.createdAtUtc))
            {
                entry.createdAtUtc = MemoryEntry.ToIsoUtc(DateTime.UtcNow);
            }

            if (string.IsNullOrWhiteSpace(entry.updatedAtUtc))
            {
                entry.updatedAtUtc = entry.createdAtUtc;
            }

            if (entry.expiresAtUtc == null)
            {
                entry.expiresAtUtc = string.Empty;
            }
        }

        private static string[] NormalizeTags(IReadOnlyList<string> tags)
        {
            if (tags == null || tags.Count == 0)
            {
                return Array.Empty<string>();
            }

            var normalized = new List<string>(tags.Count);
            var seen = new HashSet<string>(StringComparer.Ordinal);
            for (var i = 0; i < tags.Count; i++)
            {
                var tag = MemoryScope.NormalizePart(tags[i]);
                if (string.IsNullOrEmpty(tag)) continue;
                if (!seen.Add(tag)) continue;
                normalized.Add(tag);
            }

            return normalized.ToArray();
        }

        private static string NormalizeSearchText(string text)
        {
            return string.IsNullOrWhiteSpace(text)
                ? string.Empty
                : text.Trim().ToLowerInvariant();
        }

        private static bool TryScoreEntry(MemoryEntry entry, string normalizedText, string[] requiredTags, out float score)
        {
            score = Mathf.Clamp01(entry.importance);

            if (requiredTags != null && requiredTags.Length > 0)
            {
                var entryTags = entry.tags ?? Array.Empty<string>();
                for (var i = 0; i < requiredTags.Length; i++)
                {
                    var found = false;
                    for (var j = 0; j < entryTags.Length; j++)
                    {
                        if (string.Equals(entryTags[j], requiredTags[i], StringComparison.Ordinal))
                        {
                            found = true;
                            score += 3f;
                            break;
                        }
                    }

                    if (!found)
                    {
                        return false;
                    }
                }
            }

            if (string.IsNullOrEmpty(normalizedText))
            {
                return true;
            }

            var matched = false;
            score += ScoreContains(entry.title, normalizedText, 6f, ref matched);
            score += ScoreContains(entry.content, normalizedText, 4f, ref matched);
            score += ScoreContains(entry.key, normalizedText, 4f, ref matched);
            score += ScoreContains(entry.value, normalizedText, 2f, ref matched);
            score += ScoreContains(entry.sourceAgent, normalizedText, 1f, ref matched);

            if (entry.tags != null)
            {
                for (var i = 0; i < entry.tags.Length; i++)
                {
                    score += ScoreContains(entry.tags[i], normalizedText, 2f, ref matched);
                }
            }

            return matched;
        }

        private static float ScoreContains(string source, string text, float weight, ref bool matched)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(text))
            {
                return 0f;
            }

            if (source.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                matched = true;
                return weight;
            }

            return 0f;
        }

        private static int CompareUpdatedAt(MemoryEntry a, MemoryEntry b)
        {
            DateTime aTime;
            DateTime bTime;
            var hasA = MemoryEntry.TryParseUtc(a.updatedAtUtc, out aTime);
            var hasB = MemoryEntry.TryParseUtc(b.updatedAtUtc, out bTime);

            if (!hasA && !hasB) return 0;
            if (hasA && !hasB) return 1;
            if (!hasA && hasB) return -1;
            return aTime.CompareTo(bTime);
        }

        private MemoryEntry CloneEntry(MemoryEntry source)
        {
            if (source == null)
            {
                return null;
            }

            return new MemoryEntry
            {
                id = source.id,
                type = source.type,
                project = source.project,
                agent = source.agent,
                channel = source.channel,
                key = source.key,
                value = source.value,
                title = source.title,
                content = source.content,
                tags = source.tags != null ? source.tags.ToArray() : Array.Empty<string>(),
                sourceAgent = source.sourceAgent,
                importance = source.importance,
                createdAtUtc = source.createdAtUtc,
                updatedAtUtc = source.updatedAtUtc,
                expiresAtUtc = source.expiresAtUtc,
                deleted = source.deleted
            };
        }

        private void RecoverCorruptedFile(string path, Exception ex)
        {
            try
            {
                if (!File.Exists(path))
                {
                    return;
                }

                var backupPath = path + ".corrupt." + DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
                File.Move(path, backupPath);
                Debug.LogWarning("[FileAgentMemoryStore] Recovered corrupted file: " + path + " -> " + backupPath + ". Error: " + ex.Message);
            }
            catch (Exception moveEx)
            {
                Debug.LogWarning("[FileAgentMemoryStore] Failed to backup corrupted file: " + path + ". Error: " + moveEx.Message);
            }
        }

        private void EnsureStorageRoot()
        {
            Directory.CreateDirectory(_rootDirectory);
        }

        private T WithWriteLock<T>(Func<T> action)
        {
            EnsureStorageRoot();
            using (var lockStream = new FileStream(_lockPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read))
            {
                return action();
            }
        }

        private void WithWriteLock(Action action)
        {
            WithWriteLock(() =>
            {
                action();
                return true;
            });
        }

        private static void AtomicWriteAllText(string path, string content)
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var tempPath = path + ".tmp";
            File.WriteAllText(tempPath, content ?? string.Empty, Encoding.UTF8);

            if (File.Exists(path))
            {
                try
                {
                    File.Replace(tempPath, path, null);
                }
                catch
                {
                    File.Delete(path);
                    File.Move(tempPath, path);
                }
            }
            else
            {
                File.Move(tempPath, path);
            }
        }
    }
}
