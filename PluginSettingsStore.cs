using System.Text.Json;

namespace CodexUsageClassIsland;

public sealed record PluginSettingsData(
    string? AuthFilePath = null,
    int RefreshIntervalMinutes = 5,
    int LowThresholdPercent = 20,
    bool EnableLowUsageNotification = false,
    bool NotifyOnlyOnThresholdCrossing = true);

public sealed class PluginSettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private readonly string path;

    public PluginSettingsStore(string path) => this.path = path;

    public PluginSettingsData Load()
    {
        try
        {
            if (File.Exists(path))
                return JsonSerializer.Deserialize<PluginSettingsData>(File.ReadAllText(path)) ?? new();
        }
        catch (JsonException) { }
        catch (IOException) { }
        return new();
    }

    public void Save(PluginSettingsData settings)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
        File.WriteAllText(path, JsonSerializer.Serialize(settings, JsonOptions));
    }
}
