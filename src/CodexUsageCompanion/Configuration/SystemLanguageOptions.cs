using System.Globalization;

namespace CodexUsageCompanion.Configuration;

public static class SystemLanguageOptions
{
    public static string ResolveDefault(CultureInfo culture)
    {
        var name = culture.Name;
        if (name.StartsWith("zh-TW", StringComparison.OrdinalIgnoreCase) ||
            name.StartsWith("zh-HS", StringComparison.OrdinalIgnoreCase) ||
            name.StartsWith("zh-Hant", StringComparison.OrdinalIgnoreCase) ||
            name.StartsWith("zh-HK", StringComparison.OrdinalIgnoreCase) ||
            name.StartsWith("zh-MO", StringComparison.OrdinalIgnoreCase) ||
            culture.EnglishName.Contains(
                "Traditional",
                StringComparison.OrdinalIgnoreCase))
        {
            return "zh-tw";
        }

        if (name.StartsWith("zh-CN", StringComparison.OrdinalIgnoreCase) ||
            name.StartsWith("zh-Hans", StringComparison.OrdinalIgnoreCase) ||
            name.StartsWith("zh-SG", StringComparison.OrdinalIgnoreCase) ||
            culture.EnglishName.Contains(
                "Simplified",
                StringComparison.OrdinalIgnoreCase))
        {
            return "zh-cn";
        }

        return "en-US";
    }
}
