namespace CodexUsageCompanion.Configuration;

public static class OverlayDesignOptions
{
    public const string Latest = "latest";
    public const string Rings = "rings";
    public const string Previous = "previous";

    public static IReadOnlyList<string> Values { get; } = Array.AsReadOnly([Latest, Previous, Rings]);

    public static string Normalize(string? value) =>
        value?.ToLowerInvariant() switch
        {
            Previous => Previous,
            Rings => Rings,
            _ => Latest
        };
}
