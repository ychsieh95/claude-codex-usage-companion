using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace CodexUsageCompanion.RateLimits;

public sealed class ClaudeUsageClient : IAsyncDisposable
{
    private const string UsageEndpoint = "https://api.anthropic.com/api/oauth/usage";
    private readonly HttpClient _http;
    private readonly bool _ownsHttpClient;
    private readonly Func<string?> _locateCredentials;
    private readonly Func<string> _resolveUserAgent;

    public ClaudeUsageClient(
        HttpClient? httpClient = null,
        Func<string?>? locateCredentials = null,
        Func<string>? resolveUserAgent = null)
    {
        _ownsHttpClient = httpClient is null;
        _http = httpClient ?? new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        _locateCredentials = locateCredentials ?? ClaudeCredentialsLocator.Find;
        _resolveUserAgent = resolveUserAgent ?? ClaudeUserAgentResolver.Resolve;
    }

    public async Task<RateLimitState> ReadUsageAsync(CancellationToken cancellationToken)
    {
        var credentials = ClaudeCredentialsReader.Read(_locateCredentials());

        using var request = new HttpRequestMessage(HttpMethod.Get, UsageEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", credentials.AccessToken);
        request.Headers.Add("anthropic-beta", "oauth-2025-04-20");
        request.Headers.UserAgent.ParseAdd(_resolveUserAgent());

        using var response = await _http.SendAsync(request, cancellationToken);
        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            throw new ClaudeSessionExpiredException(
                "Claude session expired. Run 'claude' to refresh your session.");
        }

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        return ClaudeUsageParser.ParseResponse(body);
    }

    public ValueTask DisposeAsync()
    {
        if (_ownsHttpClient)
        {
            _http.Dispose();
        }

        return ValueTask.CompletedTask;
    }
}
