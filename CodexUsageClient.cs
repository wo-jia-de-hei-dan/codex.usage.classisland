using System.Net;
using System.Net.Http.Headers;

namespace CodexUsageClassIsland;

public sealed class CodexUsageClient
{
    private const string UsageEndpoint = "https://chatgpt.com/backend-api/wham/usage";
    private readonly HttpClient httpClient = new() { Timeout = TimeSpan.FromSeconds(20) };
    private readonly CodexAuthReader authReader;
    private readonly UsageResponseParser parser;

    public CodexUsageClient(CodexAuthReader authReader, UsageResponseParser parser)
    {
        this.authReader = authReader;
        this.parser = parser;
    }

    public async Task<UsageSnapshot> FetchAsync(string? configuredPath, CancellationToken cancellationToken)
    {
        var token = authReader.ReadAccessToken(configuredPath);
        if (string.IsNullOrWhiteSpace(token)) return new(UsageStatus.NotLoggedIn, null, null, DateTimeOffset.Now, "未找到 Codex 登录状态");
        using var request = new HttpRequestMessage(HttpMethod.Get, UsageEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var accountId = authReader.ReadAccountId(configuredPath);
        if (!string.IsNullOrWhiteSpace(accountId)) request.Headers.TryAddWithoutValidation("ChatGPT-Account-Id", accountId);
        request.Headers.TryAddWithoutValidation("OpenAI-Beta", "codex-1");
        try
        {
            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden) return new(UsageStatus.ServerRejected, null, null, DateTimeOffset.Now, "Codex 登录状态已失效");
            if (!response.IsSuccessStatusCode) return new(UsageStatus.ServerRejected, null, null, DateTimeOffset.Now, $"用量接口返回 HTTP {(int)response.StatusCode}");
            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            return parser.Parse(json);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested) { return new(UsageStatus.NetworkError, null, null, DateTimeOffset.Now, "请求超时"); }
        catch (HttpRequestException) { return new(UsageStatus.NetworkError, null, null, DateTimeOffset.Now, "网络不可用"); }
    }
}
