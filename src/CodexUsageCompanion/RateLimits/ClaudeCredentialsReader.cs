using System.IO;
using System.Text.Json;

namespace CodexUsageCompanion.RateLimits;

public sealed record ClaudeCredentials(string AccessToken, long ExpiresAtUnixMs);

public sealed class ClaudeCredentialsMissingException(string message) : Exception(message);

public sealed class ClaudeSessionExpiredException(string message) : Exception(message);

public sealed class ClaudeCredentialsFormatException(string message) : Exception(message);

public static class ClaudeCredentialsReader
{
    private const string MissingMessage = "Claude credentials not found. Run 'claude' to sign in.";
    private const string ExpiredMessage = "Claude session expired. Run 'claude' to refresh your session.";
    private const string FormatMessage = "Unable to parse Claude credentials file.";

    public static ClaudeCredentials Read(string? credentialsPath) =>
        Read(credentialsPath, DateTimeOffset.UtcNow);

    public static ClaudeCredentials Read(string? credentialsPath, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(credentialsPath))
        {
            throw new ClaudeCredentialsMissingException(MissingMessage);
        }

        string json;
        try
        {
            json = File.ReadAllText(credentialsPath);
        }
        catch (Exception exception) when (
            exception is IOException or UnauthorizedAccessException)
        {
            throw new ClaudeCredentialsMissingException(MissingMessage);
        }

        var credentials = Parse(json);
        if (DateTimeOffset.FromUnixTimeMilliseconds(credentials.ExpiresAtUnixMs) <= now)
        {
            throw new ClaudeSessionExpiredException(ExpiredMessage);
        }

        return credentials;
    }

    private static ClaudeCredentials Parse(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            var oauth = document.RootElement.GetProperty("claudeAiOauth");
            var accessToken = oauth.GetProperty("accessToken").GetString();
            var expiresAt = oauth.GetProperty("expiresAt").GetInt64();
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new ClaudeCredentialsFormatException(FormatMessage);
            }

            return new ClaudeCredentials(accessToken, expiresAt);
        }
        catch (Exception exception) when (
            exception is JsonException or KeyNotFoundException or InvalidOperationException)
        {
            throw new ClaudeCredentialsFormatException(FormatMessage);
        }
    }
}
