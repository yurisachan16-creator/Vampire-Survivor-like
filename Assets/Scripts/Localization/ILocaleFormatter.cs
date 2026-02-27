using System;
using System.Globalization;

namespace VampireSurvivorLike
{
    public interface ILocaleFormatter
    {
        string Number(double value, string format);
        string DateTime(DateTime value, string format);
        string Currency(decimal value, string currencySymbol);
        int Compare(string a, string b, CompareOptions options);
    }
}
