using System;
using System.Globalization;

namespace VampireSurvivorLike
{
    public sealed class CultureLocaleFormatter : ILocaleFormatter
    {
        public string Number(double value, string format)
        {
            return LocaleFormat.Number(value, string.IsNullOrWhiteSpace(format) ? "N0" : format);
        }

        public string DateTime(DateTime value, string format)
        {
            return LocaleFormat.DateTime(value, format);
        }

        public string Currency(decimal value, string currencySymbol)
        {
            return LocaleFormat.Currency(value, currencySymbol);
        }

        public int Compare(string a, string b, CompareOptions options)
        {
            return LocaleFormat.Compare(a, b, options);
        }
    }
}
