using System.Globalization;
using CodexUsageCompanion.Configuration;

namespace CodexUsageCompanion.Localization;

public enum UiLanguage
{
    English,
    TraditionalChinese,
    SimplifiedChinese
}

public static class UiLanguageResolver
{
    public static UiLanguage Resolve(string? setting, CultureInfo culture)
    {
        return setting?.ToLowerInvariant() switch
        {
            "en" or "en-us" => UiLanguage.English,
            "zh-tw" => UiLanguage.TraditionalChinese,
            "zh-cn" => UiLanguage.SimplifiedChinese,
            _ => ResolveCulture(culture)
        };
    }

    private static UiLanguage ResolveCulture(CultureInfo culture)
    {
        return SystemLanguageOptions.ResolveDefault(culture) switch
        {
            "zh-tw" => UiLanguage.TraditionalChinese,
            "zh-cn" => UiLanguage.SimplifiedChinese,
            _ => UiLanguage.English
        };
    }
}
