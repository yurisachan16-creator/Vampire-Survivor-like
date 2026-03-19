using System;
using System.Collections.Generic;
using UnityEngine;

namespace VampireSurvivorLike
{
    public static class LeaderboardSystem
    {
        public const string PlayerPrefsKey = "Leaderboard.Top20.v1";
        public const int MaxEntries = 20;

        [Serializable]
        public class Entry
        {
            public int Score;
            public int SurvivalSeconds;
            public int WaveMinute;
            public int Level;
            public int Coins;
            public int KillCount;
            public string DeathReason;
            public long TimestampUnix;
        }

        [Serializable]
        private class EntryCollection
        {
            public List<Entry> Entries = new List<Entry>(MaxEntries);
        }

        public static event Action OnLeaderboardChanged;

        private static readonly List<Entry> s_entries = new List<Entry>(MaxEntries);
        private static bool s_loaded;
        private static int s_lastRecordedRunSessionId = int.MinValue;

        public static IReadOnlyList<Entry> GetTopEntries()
        {
            EnsureLoaded();
            return s_entries;
        }

        public static bool RecordCurrentRun(bool isClear, string deathReason)
        {
            EnsureLoaded();
            if (!WitnessModeRuntime.ShouldRecordLeaderboard) return false;

            var runSessionId = Global.RunSessionId;
            if (s_lastRecordedRunSessionId == runSessionId)
            {
                return false;
            }

            var entry = new Entry
            {
                SurvivalSeconds = Mathf.Max(0, Mathf.FloorToInt(Global.CurrentSeconds.Value)),
                WaveMinute = Mathf.Max(0, EnemyGenerator.CurrentMinute.Value),
                Level = Mathf.Max(1, Global.Level.Value),
                Coins = Mathf.Max(0, Global.RunCoinCollected),
                KillCount = Mathf.Max(0, Global.RunKillCount),
                DeathReason = BuildDeathReason(isClear, deathReason),
                TimestampUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            entry.Score = CalculateScore(entry);

            s_entries.Add(entry);
            SortAndTrim();
            Save();

            s_lastRecordedRunSessionId = runSessionId;
            OnLeaderboardChanged?.Invoke();
            return true;
        }

        public static void ClearAll()
        {
            EnsureLoaded();
            s_entries.Clear();
            Save();
            OnLeaderboardChanged?.Invoke();
        }

        public static string BuildDeathReason(bool isClear, string damageSource)
        {
            if (isClear) return "通关";
            if (string.IsNullOrWhiteSpace(damageSource)) return "未知";
            var reason = damageSource.Trim();
            return reason.Length <= 36 ? reason : reason.Substring(0, 36);
        }

        private static void EnsureLoaded()
        {
            if (s_loaded) return;
            s_loaded = true;
            s_entries.Clear();

            var json = PlayerPrefs.GetString(PlayerPrefsKey, string.Empty);
            if (string.IsNullOrWhiteSpace(json)) return;

            try
            {
                var wrapper = JsonUtility.FromJson<EntryCollection>(json);
                if (wrapper?.Entries == null) return;
                for (var i = 0; i < wrapper.Entries.Count; i++)
                {
                    var item = wrapper.Entries[i];
                    if (item == null) continue;
                    item.DeathReason = string.IsNullOrWhiteSpace(item.DeathReason) ? "未知" : item.DeathReason;
                    s_entries.Add(item);
                }
                SortAndTrim();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[LeaderboardSystem] Load failed, fallback to empty list. {e.Message}");
                s_entries.Clear();
            }
        }

        private static void Save()
        {
            SortAndTrim();
            var wrapper = new EntryCollection
            {
                Entries = new List<Entry>(s_entries)
            };
            var json = JsonUtility.ToJson(wrapper);
            PlayerPrefs.SetString(PlayerPrefsKey, json);
            PlayerPrefs.Save();
        }

        private static int CalculateScore(Entry entry)
        {
            return entry.WaveMinute * 5000
                + entry.SurvivalSeconds * 50
                + entry.Level * 200
                + entry.KillCount;
        }

        private static void SortAndTrim()
        {
            s_entries.Sort(CompareEntry);
            if (s_entries.Count > MaxEntries)
            {
                s_entries.RemoveRange(MaxEntries, s_entries.Count - MaxEntries);
            }
        }

        private static int CompareEntry(Entry a, Entry b)
        {
            if (ReferenceEquals(a, b)) return 0;
            if (a == null) return 1;
            if (b == null) return -1;

            var c = b.Score.CompareTo(a.Score);
            if (c != 0) return c;
            c = b.WaveMinute.CompareTo(a.WaveMinute);
            if (c != 0) return c;
            c = b.SurvivalSeconds.CompareTo(a.SurvivalSeconds);
            if (c != 0) return c;
            c = b.Level.CompareTo(a.Level);
            if (c != 0) return c;
            c = b.KillCount.CompareTo(a.KillCount);
            if (c != 0) return c;
            return b.TimestampUnix.CompareTo(a.TimestampUnix);
        }
    }
}
