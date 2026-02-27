using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;

namespace VampireSurvivorLike.Tests
{
    [TestFixture]
    public class AgentMemoryStoreTests
    {
        private string _tempRoot;

        [SetUp]
        public void SetUp()
        {
            _tempRoot = Path.Combine(Path.GetTempPath(), "vs_like_agent_memory_tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempRoot);
            AgentMemory.ConfigureStore(new FileAgentMemoryStore(_tempRoot));
        }

        [TearDown]
        public void TearDown()
        {
            AgentMemory.ResetDefaultStore();

            try
            {
                if (Directory.Exists(_tempRoot))
                {
                    Directory.Delete(_tempRoot, true);
                }
            }
            catch
            {
                // Ignore cleanup failures in CI temp paths.
            }
        }

        [Test]
        public void Set_Then_TryGet_ReturnsValue()
        {
            var scope = CreateScope("agent-a");
            AgentMemory.Set("theme", "dark", scope, "codex");

            string value;
            var found = AgentMemory.TryGet("theme", scope, out value);

            Assert.That(found, Is.True);
            Assert.That(value, Is.EqualTo("dark"));
        }

        [Test]
        public void Set_SameKeySameScope_UpdatesInsteadOfDuplicating()
        {
            var scope = CreateScope("agent-a");
            AgentMemory.Set("difficulty", "normal", scope, "codex");
            AgentMemory.Set("difficulty", "hard", scope, "codex");

            string value;
            var found = AgentMemory.TryGet("difficulty", scope, out value);
            var results = AgentMemory.Search(new MemoryQuery
            {
                scope = scope,
                includeExpired = true,
                limit = 10
            });

            var duplicates = results.Count(r =>
                string.Equals(r.entry.type, "KeyValue", StringComparison.Ordinal) &&
                string.Equals(r.entry.key, "difficulty", StringComparison.OrdinalIgnoreCase));

            Assert.That(found, Is.True);
            Assert.That(value, Is.EqualTo("hard"));
            Assert.That(duplicates, Is.EqualTo(1));
        }

        [Test]
        public void AddKnowledge_SearchByKeyword_ReturnsRankedResults()
        {
            var scope = CreateScope("agent-a");
            var idLower = AgentMemory.AddKnowledge(
                "Wave note",
                "spawn timing basics",
                new[] { "combat" },
                scope,
                "codex",
                importance: 0.2f);

            Thread.Sleep(30);

            var idHigher = AgentMemory.AddKnowledge(
                "Spawn timing handbook",
                "spawn timing advanced strategy",
                new[] { "combat" },
                scope,
                "codex",
                importance: 0.2f);

            var results = AgentMemory.Search(new MemoryQuery
            {
                scope = scope,
                text = "spawn timing",
                limit = 10
            });

            Assert.That(results.Count, Is.GreaterThanOrEqualTo(2));
            Assert.That(results[0].entry.id, Is.EqualTo(idHigher));
            Assert.That(results.Any(r => r.entry.id == idLower), Is.True);
        }

        [Test]
        public void AddKnowledge_SearchByTagsAndScope_FiltersCorrectly()
        {
            var targetScope = new global::VampireSurvivorLike.MemoryScope("project-a", "agent-1", "battle");
            var otherScope = new global::VampireSurvivorLike.MemoryScope("project-b", "agent-1", "battle");

            var wantedId = AgentMemory.AddKnowledge(
                "Boss behavior",
                "phase and hitbox notes",
                new[] { "combat", "boss" },
                targetScope,
                "codex");

            AgentMemory.AddKnowledge(
                "Other project",
                "same tags but another project",
                new[] { "combat", "boss" },
                otherScope,
                "codex");

            AgentMemory.AddKnowledge(
                "Missing tag",
                "only one tag",
                new[] { "combat" },
                targetScope,
                "codex");

            var results = AgentMemory.Search(new MemoryQuery
            {
                scope = targetScope,
                tags = new[] { "combat", "boss" },
                limit = 10
            });

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0].entry.id, Is.EqualTo(wantedId));
        }

        [Test]
        public void PruneExpired_RemovesOnlyExpiredEntries()
        {
            var scope = CreateScope("agent-expire");
            var now = DateTime.UtcNow;

            AgentMemory.Set("expired-key", "v1", scope, "codex", now.AddMinutes(-5));
            AgentMemory.Set("active-key", "v2", scope, "codex", now.AddMinutes(10));

            AgentMemory.AddKnowledge(
                "expired knowledge",
                "will be pruned",
                new[] { "ttl" },
                scope,
                "codex",
                now.AddMinutes(-5));

            AgentMemory.AddKnowledge(
                "active knowledge",
                "should remain",
                new[] { "ttl" },
                scope,
                "codex",
                now.AddMinutes(10));

            var removed = AgentMemory.PruneExpired(now);
            var remaining = AgentMemory.Search(new MemoryQuery
            {
                scope = scope,
                includeExpired = true,
                limit = 10
            });

            Assert.That(removed, Is.EqualTo(2));
            Assert.That(remaining.Count, Is.EqualTo(2));
            Assert.That(remaining.Any(r => r.entry.key == "expired-key"), Is.False);
            Assert.That(remaining.Any(r => r.entry.title == "expired knowledge"), Is.False);
        }

        [Test]
        public void CorruptFile_RecoveredAndBackupCreated()
        {
            var scope = CreateScope("agent-corrupt");
            var kvPath = Path.Combine(_tempRoot, "memory_kv.json");
            File.WriteAllText(kvPath, "{ this is broken json", Encoding.UTF8);

            AgentMemory.Set("recover-key", "ok", scope, "codex");

            var backups = Directory.GetFiles(_tempRoot, "memory_kv.json.corrupt.*");
            string value;
            var found = AgentMemory.TryGet("recover-key", scope, out value);

            Assert.That(backups.Length, Is.GreaterThanOrEqualTo(1));
            Assert.That(found, Is.True);
            Assert.That(value, Is.EqualTo("ok"));
        }

        [Test]
        public void DeleteById_MarksDeletedAndNotReturnedByDefault()
        {
            var scope = CreateScope("agent-delete");
            var id = AgentMemory.AddKnowledge(
                "Delete target",
                "this should disappear from search",
                new[] { "delete" },
                scope,
                "codex");

            var deleted = AgentMemory.DeleteById(id);
            var results = AgentMemory.Search(new MemoryQuery
            {
                scope = scope,
                text = "delete target",
                limit = 10
            });

            Assert.That(deleted, Is.True);
            Assert.That(results.Count, Is.EqualTo(0));
        }

        [Test]
        public void Search_RespectsLimitAndSortOrder()
        {
            var scope = CreateScope("agent-limit");
            var id1 = AgentMemory.AddKnowledge("A", "same text", Array.Empty<string>(), scope, "codex", importance: 0.5f);
            Thread.Sleep(30);
            var id2 = AgentMemory.AddKnowledge("B", "same text", Array.Empty<string>(), scope, "codex", importance: 0.5f);
            Thread.Sleep(30);
            var id3 = AgentMemory.AddKnowledge("C", "same text", Array.Empty<string>(), scope, "codex", importance: 0.5f);

            var results = AgentMemory.Search(new MemoryQuery
            {
                scope = scope,
                limit = 2,
                includeExpired = true
            });

            Assert.That(results.Count, Is.EqualTo(2));
            Assert.That(results[0].entry.id, Is.EqualTo(id3));
            Assert.That(results[1].entry.id, Is.EqualTo(id2));
            Assert.That(results.All(r => r.entry.id != id1), Is.True);
        }

        private static global::VampireSurvivorLike.MemoryScope CreateScope(string agent)
        {
            return new global::VampireSurvivorLike.MemoryScope("project-main", agent, "default");
        }
    }
}
