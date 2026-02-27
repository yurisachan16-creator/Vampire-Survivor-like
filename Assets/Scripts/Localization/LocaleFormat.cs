using System;
using System.Collections.Generic;
using System.Globalization;

namespace VampireSurvivorLike
{
    public static class LocaleFormat
    {
        public static CultureInfo GetCulture(LanguageId language)
        {
            var code = language.IsEmpty ? string.Empty : language.ToString();
            if (code.Equals("zh-Hant", StringComparison.OrdinalIgnoreCase)) return new CultureInfo("zh-TW");
            if (code.Equals("zh-Hans", StringComparison.OrdinalIgnoreCase)) return new CultureInfo("zh-CN");
            if (code.Equals("en", StringComparison.OrdinalIgnoreCase)) return new CultureInfo("en-US");
            return CultureInfo.InvariantCulture;
        }

        public static CultureInfo CurrentCulture => GetCulture(LocalizationManager.CurrentLanguage.Value);

        public static string Number(double value, string format = "N0")
        {
            return value.ToString(format, CurrentCulture);
        }

        public static string Number(int value, string format = "N0")
        {
            return value.ToString(format, CurrentCulture);
        }

        public static string DateTime(DateTime value, string format = null)
        {
            var culture = CurrentCulture;
            if (string.IsNullOrWhiteSpace(format)) return value.ToString(culture);
            return value.ToString(format, culture);
        }

        public static string Currency(decimal value, string currencySymbol = null)
        {
            var culture = (CultureInfo)CurrentCulture.Clone();
            if (!string.IsNullOrWhiteSpace(currencySymbol))
            {
                culture.NumberFormat = (NumberFormatInfo)culture.NumberFormat.Clone();
                culture.NumberFormat.CurrencySymbol = currencySymbol;
            }
            return value.ToString("C", culture);
        }

        public static int Compare(string a, string b, CompareOptions options = CompareOptions.IgnoreCase)
        {
            var culture = CurrentCulture;
            return culture.CompareInfo.Compare(a ?? string.Empty, b ?? string.Empty, options);
        }

        public static void Sort(List<string> list, CompareOptions options = CompareOptions.IgnoreCase)
        {
            if (list == null) return;
            list.Sort((a, b) => Compare(a, b, options));
        }
    }
}
