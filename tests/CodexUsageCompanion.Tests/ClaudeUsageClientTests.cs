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

    [Fact]
    public async Task ReadUsageAsyncRenewsAnExpiredTokenInsteadOfFailing()
    {
        // The reboot case: the CLI has not run since the access token lapsed.
        var credentialsPath = WriteCredentials(
            DateTimeOffset.UtcNow.AddHours(-9),
            refreshToken: "sk-ant-ort01-test");
        var usageTokens = new List<string?>();
        var handler = new RoutingHandler(
            onToken: _ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """{"access_token":"sk-ant-oat01-fresh","refresh_token":"sk-ant-ort01-next","expires_in":28800}""")
            },
            onUsage: request =>
            {
                usageTokens.Add(request.Headers.Authorization?.Parameter);
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("""{"five_hour": {"utilization": 10.0}}""")
                };
            });
        await using var client = new ClaudeUsageClient(
            new HttpClient(handler),
            locateCredentials: () => credentialsPath,
            resolveUserAgent: () => "claude-cli/9.9.9");
        try
        {
            var state = await client.ReadUsageAsync(CancellationToken.None);

            Assert.Equal(90, state.FiveHour?.RemainingPercent);
            Assert.Equal(["sk-ant-oat01-fresh"], usageTokens);

            // The rotated pair must reach disk, or the next run would reuse a spent token.
            var stored = ClaudeCredentialsReader.ReadAllowingExpired(credentialsPath);
            Assert.Equal("sk-ant-oat01-fresh", stored.AccessToken);
            Assert.Equal("sk-ant-ort01-next", stored.RefreshToken);
            Assert.False(ClaudeCredentialsReader.IsExpired(stored, DateTimeOffset.UtcNow));
        }
        finally
        {
            File.Delete(credentialsPath);
        }
    }

    [Fact]
    public async Task ReadUsageAsyncRenewsWhenTheServerRejectsAnUnexpiredToken()
    {
        var credentialsPath = WriteCredentials(
            DateTimeOffset.UtcNow.AddHours(1),
            refreshToken: "sk-ant-ort01-test");
        var usageTokens = new List<string?>();
        var handler = new RoutingHandler(
            onToken: _ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """{"access_token":"sk-ant-oat01-fresh","expires_in":28800}""")
            },
            onUsage: request =>
            {
                usageTokens.Add(request.Headers.Authorization?.Parameter);
                return usageTokens.Count == 1
                    ? new HttpResponseMessage(HttpStatusCode.Unauthorized)
                    : new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent("""{"five_hour": {"utilization": 10.0}}""")
                    };
            });
        await using var client = new ClaudeUsageClient(
            new HttpClient(handler),
            locateCredentials: () => credentialsPath,
            resolveUserAgent: () => "claude-cli/9.9.9");
        try
        {
            var state = await client.ReadUsageAsync(CancellationToken.None);

            Assert.Equal(90, state.FiveHour?.RemainingPercent);
            Assert.Equal(["sk-ant-oat01-test", "sk-ant-oat01-fresh"], usageTokens);
        }
        finally
        {
            File.Delete(credentialsPath);
        }
    }

    [Fact]
    public async Task ReadUsageAsyncStopsAfterOneRenewalWhenTheFreshTokenIsAlsoRejected()
    {
        var credentialsPath = WriteCredentials(
            DateTimeOffset.UtcNow.AddHours(1),
            refreshToken: "sk-ant-ort01-test");
        var tokenCalls = 0;
        var handler = new RoutingHandler(
            onToken: _ =>
            {
                tokenCalls++;
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        """{"access_token":"sk-ant-oat01-fresh","expires_in":28800}""")
                };
            },
            onUsage: _ => new HttpResponseMessage(HttpStatusCode.Unauthorized));
        await using var client = new ClaudeUsageClient(
            new HttpClient(handler),
            locateCredentials: () => credentialsPath,
            resolveUserAgent: () => "claude-cli/9.9.9");
        try
        {
            await Assert.ThrowsAsync<ClaudeSessionExpiredException>(
                () => client.ReadUsageAsync(CancellationToken.None));
            Assert.Equal(1, tokenCalls);
        }
        finally
        {
            File.Delete(credentialsPath);
        }
    }

    [Fact]
    public async Task ReadUsageAsyncThrowsSessionExpiredWhenExpiredCredentialsHaveNoRefreshToken()
    {
        var credentialsPath = WriteCredentials(DateTimeOffset.UtcNow.AddHours(-9), refreshToken: null);
        var handler = new RoutingHandler(
            onToken: _ => new HttpResponseMessage(HttpStatusCode.OK),
            onUsage: _ => new HttpResponseMessage(HttpStatusCode.OK));
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
    public async Task ReadUsageAsyncAdoptsTheTokenAnotherProcessRefreshedInstead()
    {
        var credentialsPath = WriteCredentials(
            DateTimeOffset.UtcNow.AddHours(-9),
            refreshToken: "sk-ant-ort01-test");
        var refreshedCredentialsPath = credentialsPath + ".refreshed";
        var tokenCalls = 0;
        string? usageToken = null;
        var handler = new RoutingHandler(
            onToken: _ =>
            {
                Interlocked.Increment(ref tokenCalls);
                return new HttpResponseMessage(HttpStatusCode.OK);
            },
            onUsage: request =>
            {
                usageToken = request.Headers.Authorization?.Parameter;
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("""{"five_hour": {"utilization": 10.0}}""")
                };
            });
        await using var client = new ClaudeUsageClient(
            new HttpClient(handler),
            locateCredentials: () => credentialsPath,
            resolveUserAgent: () => "claude-cli/9.9.9");
        try
        {
            // Stand in for the Claude CLI holding its refresh lock, then publishing a new token.
            using var cliLock = ClaudeCredentialsRefreshLock.TryAcquire(credentialsPath, TimeSpan.Zero);
            Assert.NotNull(cliLock);
            // Publish a complete file atomically, so the first read cannot see
            // an empty or partially written JSON document during replacement.
            File.WriteAllText(
                refreshedCredentialsPath,
                $$"""{"claudeAiOauth": {"accessToken": "sk-ant-oat01-cli", "refreshToken": "sk-ant-ort01-cli", "expiresAt": {{DateTimeOffset.UtcNow.AddHours(8).ToUnixTimeMilliseconds()}} } }""");
            var reading = Task.Run(() => client.ReadUsageAsync(CancellationToken.None));
            File.Move(refreshedCredentialsPath, credentialsPath, overwrite: true);
            cliLock!.Dispose();

            var state = await reading;

            Assert.Equal(90, state.FiveHour?.RemainingPercent);
            Assert.Equal("sk-ant-oat01-cli", usageToken);
            Assert.Equal(0, tokenCalls);
        }
        finally
        {
            File.Delete(refreshedCredentialsPath);
            File.Delete(credentialsPath);
        }
    }

    private static string WriteCredentials(DateTimeOffset expiresAt, string? refreshToken)
    {
        var directory = Path.Combine(Path.GetTempPath(), $"claude-client-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, ".credentials.json");
        var refreshField = refreshToken is null
            ? string.Empty
            : $$""", "refreshToken": "{{refreshToken}}" """;
        File.WriteAllText(
            path,
            $$"""{"claudeAiOauth": {"accessToken": "sk-ant-oat01-test", "expiresAt": {{expiresAt.ToUnixTimeMilliseconds()}}{{refreshField}} } }""");
        return path;
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

    private sealed class RoutingHandler(
        Func<HttpRequestMessage, HttpResponseMessage> onToken,
        Func<HttpRequestMessage, HttpResponseMessage> onUsage) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var isToken = request.RequestUri?.ToString() == ClaudeOAuthTokenRefresher.TokenEndpoint;
            return Task.FromResult(isToken ? onToken(request) : onUsage(request));
        }
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
