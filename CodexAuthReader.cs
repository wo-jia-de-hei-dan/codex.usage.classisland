using System.Text.Json;

namespace CodexUsageClassIsland;

public sealed class CodexAuthReader
{
    public string? ResolvePath(string? configuredPath)
    {
        if (!string.IsNullOrWhiteSpace(configuredPath) && File.Exists(configuredPath)) return configuredPath;
        var defaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".codex", "auth.json");
        return File.Exists(defaultPath) ? defaultPath : null;
    }

    public string? ReadAccessToken(string? configuredPath)
    {
        var path = ResolvePath(configuredPath);
        if (path is null) return null;
        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(path));
            if (document.RootElement.TryGetProperty("tokens", out var tokens) &&
                tokens.TryGetProperty("access_token", out var token)) return token.GetString();
            if (document.RootElement.TryGetProperty("access_token", out var direct)) return direct.GetString();
        }
        catch (JsonException) { }
        catch (IOException) { }
        return null;
    }

    public string? ReadAccountId(string? configuredPath)
    {
        var path = ResolvePath(configuredPath);
        if (path is null) return null;
        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(path));
            if (document.RootElement.TryGetProperty("tokens", out var tokens) &&
                tokens.TryGetProperty("account_id", out var account)) return account.GetString();
        }
        catch (JsonException) { }
        catch (IOException) { }
        return null;
    }
}
