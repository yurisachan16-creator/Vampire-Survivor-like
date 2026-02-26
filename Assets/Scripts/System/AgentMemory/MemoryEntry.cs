using System;
using System.Globalization;

namespace VampireSurvivorLike
{
    [Serializable]
    public class MemoryEntry
    {
        public string id;
        public string type;
        public string project;
        public string agent;
        public string channel;
        public string key;
        public string value;
        public string title;
        public string content;
        public string[] tags;
        public string sourceAgent;
        public float importance;
        public string createdAtUtc;
        public string updatedAtUtc;
        public string expiresAtUtc;
        public bool deleted;

        public MemoryScope GetScope()
        {
            return new MemoryScope(project, agent, channel);
        }

        public MemoryEntryType GetEntryType()
        {
            MemoryEntryType parsed;
            if (Enum.TryParse(type, true, out parsed))
            {
                return parsed;
            }

            return MemoryEntryType.Knowledge;
        }

        public void SetEntryType(MemoryEntryType entryType)
        {
            type = entryType.ToString();
        }

        public bool IsExpired(DateTime nowUtc)
        {
            if (string.IsNullOrEmpty(expiresAtUtc))
            {
                return false;
            }

            DateTime expires;
            if (!TryParseUtc(expiresAtUtc, out expires))
            {
                return false;
            }

            return expires < nowUtc;
        }

        public static bool TryParseUtc(string iso, out DateTime utc)
        {
            return DateTime.TryParse(
                iso,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind | DateTimeStyles.AssumeUniversal,
                out utc);
        }

        public static string ToIsoUtc(DateTime utc)
        {
            return utc.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);
        }
    }
}
