using System;
using System.Globalization;

namespace HR_Mangment2.Localization;

public static class LocalizedName
{
    /// <summary>True when the request language selects the English name. Arabic is the default.</summary>
    public static bool IsEnglish => string.Equals(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName, "en", StringComparison.OrdinalIgnoreCase);

    public static string Pick(string nameAr, string nameEn) => IsEnglish ? nameEn : nameAr;
}
