namespace CodexUsageCompanion.Configuration;

public static class OverlayDesignOptions
{
    public const string Latest = "latest";
    public const string Previous = "previous";

    public static IReadOnlyList<string> Values { get; } = Array.AsReadOnly([Latest, Previous]);

    public static string Normalize(string? value) =>
        string.Equals(value, Previous, StringComparison.OrdinalIgnoreCase) ? Previous : Latest;
}
