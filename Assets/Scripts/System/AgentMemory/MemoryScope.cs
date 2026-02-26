using System;

namespace VampireSurvivorLike
{
    [Serializable]
    public struct MemoryScope : IEquatable<MemoryScope>
    {
        public string project;
        public string agent;
        public string channel;

        public MemoryScope(string project, string agent, string channel)
        {
            this.project = NormalizePart(project);
            this.agent = NormalizePart(agent);
            this.channel = NormalizePart(channel);
        }

        public static MemoryScope Empty => new MemoryScope(string.Empty, string.Empty, string.Empty);

        public bool IsEmpty =>
            string.IsNullOrEmpty(project) &&
            string.IsNullOrEmpty(agent) &&
            string.IsNullOrEmpty(channel);

        public MemoryScope Normalized()
        {
            return new MemoryScope(project, agent, channel);
        }

        public bool MatchesFilter(MemoryScope filter)
        {
            var normalizedFilter = filter.Normalized();

            if (!string.IsNullOrEmpty(normalizedFilter.project) && !string.Equals(project, normalizedFilter.project, StringComparison.Ordinal))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(normalizedFilter.agent) && !string.Equals(agent, normalizedFilter.agent, StringComparison.Ordinal))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(normalizedFilter.channel) && !string.Equals(channel, normalizedFilter.channel, StringComparison.Ordinal))
            {
                return false;
            }

            return true;
        }

        public string ToStorageKey()
        {
            return project + "|" + agent + "|" + channel;
        }

        public bool Equals(MemoryScope other)
        {
            return string.Equals(project, other.project, StringComparison.Ordinal) &&
                   string.Equals(agent, other.agent, StringComparison.Ordinal) &&
                   string.Equals(channel, other.channel, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is MemoryScope other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = 17;
                hashCode = (hashCode * 31) + (project != null ? project.GetHashCode() : 0);
                hashCode = (hashCode * 31) + (agent != null ? agent.GetHashCode() : 0);
                hashCode = (hashCode * 31) + (channel != null ? channel.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static string NormalizePart(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value.Trim().ToLowerInvariant();
        }
    }
}
