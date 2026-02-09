using System;

namespace VampireSurvivorLike
{
    [Serializable]
    public struct LanguageId : IEquatable<LanguageId>
    {
        public string Code;

        public LanguageId(string code)
        {
            Code = Normalize(code);
        }

        public static LanguageId ZhHans => new LanguageId("zh-Hans");
        public static LanguageId ZhHant => new LanguageId("zh-Hant");
        public static LanguageId En => new LanguageId("en");
        public static LanguageId Ja => new LanguageId("ja");
        public static LanguageId Ko => new LanguageId("ko");

        public bool IsEmpty => string.IsNullOrWhiteSpace(Code);

        public override string ToString() => Code ?? string.Empty;

        public bool Equals(LanguageId other) => string.Equals(Normalize(Code), Normalize(other.Code), StringComparison.OrdinalIgnoreCase);

        public override bool Equals(object obj) => obj is LanguageId other && Equals(other);

        public override int GetHashCode() => (Normalize(Code) ?? string.Empty).ToLowerInvariant().GetHashCode();

        public static bool operator ==(LanguageId a, LanguageId b) => a.Equals(b);
        public static bool operator !=(LanguageId a, LanguageId b) => !a.Equals(b);

        public static string Normalize(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return string.Empty;
            return code.Trim();
        }
    }
}
