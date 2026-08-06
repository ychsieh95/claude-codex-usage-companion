using CodexUsageCompanion.Platform;

namespace CodexUsageCompanion.Configuration;

public static class UsageLogOptions
{
    public const string Text = "txt";
    public const string Csv = "csv";
    public const string JsonLines = "jsonl";

    public static IReadOnlyList<string> Formats { get; } = Array.AsReadOnly(
    [
        Csv,
        Text,
        JsonLines
    ]);

    public static string NormalizeFormat(string? format)
    {
        return format?.ToLowerInvariant() switch
        {
            Csv => Csv,
            Text => Text,
            JsonLines => JsonLines,
            _ => Csv
        };
    }

    public static string DefaultFilePath(string? format = null)
    {
        var normalized = NormalizeFormat(format);
        return Path.Combine(LinuxPaths.StateDirectory, $"usage-history.{normalized}");
    }

    public static string NormalizeFilePath(string? path, string? format = null)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return DefaultFilePath(format);
        }

        var normalizedFormat = NormalizeFormat(format);
        var trimmed = path.Trim();
        try
        {
            string fullPath;
            if (trimmed == "~")
            {
                fullPath = LinuxPaths.UserHome;
            }
            else if (trimmed.StartsWith("~/", StringComparison.Ordinal))
            {
                fullPath = Path.GetFullPath(Path.Combine(LinuxPaths.UserHome, trimmed[2..]));
            }
            else
            {
                fullPath = Path.IsPathFullyQualified(trimmed)
                    ? Path.GetFullPath(trimmed)
                    : Path.GetFullPath(Path.Combine(LinuxPaths.StateDirectory, trimmed));
            }

            return ChangeFileExtension(fullPath, normalizedFormat);
        }
        catch (Exception exception) when (
            exception is ArgumentException or
            NotSupportedException)
        {
            return DefaultFilePath(format);
        }
    }

    public static string ChangeFileExtension(string? path, string? format)
    {
        var normalizedFormat = NormalizeFormat(format);
        if (string.IsNullOrWhiteSpace(path))
        {
            return DefaultFilePath(normalizedFormat);
        }

        try
        {
            return Path.ChangeExtension(path.Trim(), normalizedFormat)
                ?? DefaultFilePath(normalizedFormat);
        }
        catch (Exception exception) when (
            exception is ArgumentException or
            NotSupportedException)
        {
            return DefaultFilePath(normalizedFormat);
        }
    }
}
