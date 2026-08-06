using CodexUsageCompanion.RateLimits;
using Xunit;

namespace CodexUsageCompanion.Tests;

public sealed class ClaudeCredentialsReaderTests
{
    [Fact]
    public void ReadThrowsMissingExceptionWhenPathIsNull()
    {
        Assert.Throws<ClaudeCredentialsMissingException>(() => ClaudeCredentialsReader.Read(null));
    }

    [Fact]
    public void ReadThrowsMissingExceptionWhenFileDoesNotExist()
    {
        var path = Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}.json");

        Assert.Throws<ClaudeCredentialsMissingException>(() => ClaudeCredentialsReader.Read(path));
    }

    [Fact]
    public void ReadThrowsFormatExceptionForMalformedJson()
    {
        var path = WriteTempFile("{");
        try
        {
            Assert.Throws<ClaudeCredentialsFormatException>(() => ClaudeCredentialsReader.Read(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void ReadThrowsFormatExceptionWhenAccessTokenIsMissing()
    {
        var path = WriteTempFile("""{"claudeAiOauth": {"expiresAt": 9999999999999}}""");
        try
        {
            Assert.Throws<ClaudeCredentialsFormatException>(() => ClaudeCredentialsReader.Read(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void ReadThrowsSessionExpiredWhenExpiresAtIsInThePast()
    {
        var past = DateTimeOffset.UtcNow.AddMinutes(-5).ToUnixTimeMilliseconds();
        var path = WriteTempFile(
            $$"""{"claudeAiOauth": {"accessToken": "sk-ant-oat01-test", "expiresAt": {{past}} } }""");
        try
        {
            Assert.Throws<ClaudeSessionExpiredException>(() => ClaudeCredentialsReader.Read(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void ReadReturnsCredentialsWhenTokenIsStillValid()
    {
        var future = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeMilliseconds();
        var path = WriteTempFile(
            $$"""{"claudeAiOauth": {"accessToken": "sk-ant-oat01-test", "expiresAt": {{future}} } }""");
        try
        {
            var credentials = ClaudeCredentialsReader.Read(path);

            Assert.Equal("sk-ant-oat01-test", credentials.AccessToken);
            Assert.Equal(future, credentials.ExpiresAtUnixMs);
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static string WriteTempFile(string content)
    {
        var path = Path.Combine(Path.GetTempPath(), $"claude-credentials-{Guid.NewGuid():N}.json");
        File.WriteAllText(path, content);
        return path;
    }
}
