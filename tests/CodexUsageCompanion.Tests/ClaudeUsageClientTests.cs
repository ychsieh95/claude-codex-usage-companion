using System.Net;
using System.Net.Http;
using CodexUsageCompanion.RateLimits;
using Xunit;

namespace CodexUsageCompanion.Tests;

public sealed class ClaudeUsageClientTests
{
    [Fact]
    public async Task ReadUsageAsyncSendsExpectedHeadersAndParsesResponse()
    {
        HttpRequestMessage? capturedRequest = null;
        var handler = new StubHandler(request =>
        {
            capturedRequest = request;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"five_hour": {"utilization": 10.0}}""")
            };
        });
        var credentialsPath = WriteValidCredentials();
        await using var client = new ClaudeUsageClient(
            new HttpClient(handler),
            locateCredentials: () => credentialsPath,
            resolveUserAgent: () => "claude-cli/9.9.9");
        try
        {
            var state = await client.ReadUsageAsync(CancellationToken.None);

            Assert.Equal(90, state.FiveHour?.RemainingPercent);
            Assert.NotNull(capturedRequest);
            Assert.Equal("Bearer", capturedRequest!.Headers.Authorization?.Scheme);
            Assert.Equal("sk-ant-oat01-test", capturedRequest.Headers.Authorization?.Parameter);
            Assert.Contains("oauth-2025-04-20", capturedRequest.Headers.GetValues("anthropic-beta"));
            Assert.Equal("claude-cli/9.9.9", capturedRequest.Headers.UserAgent.ToString());
        }
        finally
        {
            File.Delete(credentialsPath);
        }
    }

    [Fact]
    public async Task ReadUsageAsyncThrowsSessionExpiredOn401()
    {
        var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.Unauthorized));
        var credentialsPath = WriteValidCredentials();
        await using var client = new ClaudeUsageClient(
            new HttpClient(handler),
            locateCredentials: () => credentialsPath,
            resolveUserAgent: () => "claude-cli/9.9.9");
        try
        {
            await Assert.ThrowsAsync<ClaudeSessionExpiredException>(
                () => client.ReadUsageAsync(CancellationToken.None));
        }
        finally
        {
            File.Delete(credentialsPath);
        }
    }

    [Fact]
    public async Task ReadUsageAsyncThrowsOnServerError()
    {
        var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var credentialsPath = WriteValidCredentials();
        await using var client = new ClaudeUsageClient(
            new HttpClient(handler),
            locateCredentials: () => credentialsPath,
            resolveUserAgent: () => "claude-cli/9.9.9");
        try
        {
            await Assert.ThrowsAsync<HttpRequestException>(
                () => client.ReadUsageAsync(CancellationToken.None));
        }
        finally
        {
            File.Delete(credentialsPath);
        }
    }

    [Fact]
    public async Task ReadUsageAsyncShortCircuitsWithoutNetworkCallWhenCredentialsMissing()
    {
        var called = false;
        var handler = new StubHandler(_ =>
        {
            called = true;
            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        await using var client = new ClaudeUsageClient(
            new HttpClient(handler),
            locateCredentials: () => null,
            resolveUserAgent: () => "claude-cli/9.9.9");

        await Assert.ThrowsAsync<ClaudeCredentialsMissingException>(
            () => client.ReadUsageAsync(CancellationToken.None));
        Assert.False(called);
    }

    private static string WriteValidCredentials()
    {
        var future = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeMilliseconds();
        var path = Path.Combine(Path.GetTempPath(), $"claude-credentials-{Guid.NewGuid():N}.json");
        File.WriteAllText(
            path,
            $$"""{"claudeAiOauth": {"accessToken": "sk-ant-oat01-test", "expiresAt": {{future}} } }""");
        return path;
    }

    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(respond(request));
        }
    }
}
