using System.Net;
using System.Net.Http;
using System.Text.Json;

namespace CodexUsageCompanion.Lifecycle;

public sealed record AppUpdateResult(string Version, bool UpdateAvailable, Uri ReleaseUrl);

public sealed class AppUpdateChecker
{
    public const string RepositoryUrl = "https://github.com/ychsieh95/claude-codex-usage-companion";
    private static readonly HttpClient Client = new() { Timeout = TimeSpan.FromSeconds(15) };
    public static string CurrentVersion => typeof(AppUpdateChecker).Assembly.GetName().Version?.ToString(3) ?? "0.0.0";

    public async Task<AppUpdateResult> CheckAsync(CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get,
            "https://api.github.com/repos/ychsieh95/claude-codex-usage-companion/releases/latest");
        request.Headers.UserAgent.ParseAdd($"ClaudeCodexUsageCompanion/{CurrentVersion}");
        request.Headers.Accept.ParseAdd("application/vnd.github+json");
        using var response = await Client.SendAsync(request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
            return new(CurrentVersion, false, new Uri(RepositoryUrl + "/releases"));
        response.EnsureSuccessStatusCode();
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        return ParseRelease(json.RootElement, CurrentVersion);
    }

    internal static AppUpdateResult ParseRelease(JsonElement release, string currentVersion)
    {
        var tag = release.GetProperty("tag_name").GetString() ?? throw new FormatException("Missing release version.");
        if (!TryParseVersion(tag, out var latest) || !TryParseVersion(currentVersion, out var current))
            throw new FormatException("Unsupported release version.");
        var stable = !release.GetProperty("draft").GetBoolean() && !release.GetProperty("prerelease").GetBoolean();
        // Construct the URL from the trusted repository, never a server-provided destination.
        return new(tag, stable && latest > current,
            new Uri(RepositoryUrl + "/releases/tag/" + Uri.EscapeDataString(tag)));
    }

    private static bool TryParseVersion(string value, out Version version)
    {
        var normalized = value.Trim().TrimStart('v', 'V').Split('+')[0];
        if (!Version.TryParse(normalized, out var parsed))
        {
            version = new Version(0, 0, 0, 0);
            return false;
        }
        version = new Version(parsed.Major, parsed.Minor, Math.Max(parsed.Build, 0), Math.Max(parsed.Revision, 0));
        return true;
    }
}
