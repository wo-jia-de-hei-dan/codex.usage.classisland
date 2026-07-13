using ClassIsland.Core.Abstractions;
using ClassIsland.Core.Attributes;
using ClassIsland.Core.Extensions.Registry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CodexUsageClassIsland;

[PluginEntrance]
public sealed class Plugin : PluginBase
{
    internal static UsageService? Service { get; set; }

    public override void Initialize(HostBuilderContext context, IServiceCollection services)
    {
        var settingsPath = Path.Combine(PluginConfigFolder, "settings.json");
        var store = new PluginSettingsStore(settingsPath);
        var data = store.Load();
        var settings = new PluginSettings
        {
            AuthFilePath = data.AuthFilePath,
            RefreshIntervalMinutes = data.RefreshIntervalMinutes,
            LowThresholdPercent = data.LowThresholdPercent,
            EnableLowUsageNotification = data.EnableLowUsageNotification,
            NotifyOnlyOnThresholdCrossing = data.NotifyOnlyOnThresholdCrossing
        };
        services.AddSingleton(settings);
        services.AddSingleton(store);
        services.AddSingleton<UsageResponseParser>();
        services.AddSingleton<CodexAuthReader>();
        services.AddSingleton<CodexUsageClient>();
        services.AddSingleton<UsageService>();
        services.AddSingleton<CodexNotificationProvider>();
        services.AddHostedService<CodexNotificationRegistrationService>();
        services.AddComponent<CodexUsageComponent>();
        services.AddSettingsPage<CodexUsageSettingsPage>();
        services.AddTrigger<CodexUsageLowTrigger>();
        services.AddRule("codex.usage.low", "Codex 剩余额度低于", "\uE8D2", _ => Service?.IsBelowThreshold == true);
    }
}
