using ClassIsland.Core.Abstractions.Services.NotificationProviders;
using ClassIsland.Core.Attributes;
using ClassIsland.Core.Models.Notification;

namespace CodexUsageClassIsland;

[NotificationProviderInfo("a6ebfb1f-19d5-4d63-89f0-a9f3c0fdc9d1", "Codex 使用量", "\uE8D2")]
public sealed class CodexNotificationProvider : NotificationProviderBase
{
    private readonly UsageService service;
    private readonly PluginSettings settings;

    public CodexNotificationProvider(UsageService service, PluginSettings settings)
    {
        this.service = service;
        this.settings = settings;
        service.ThresholdCrossed += OnThresholdCrossed;
    }

    private void OnThresholdCrossed(object? sender, UsageSnapshot snapshot)
    {
        if (!settings.EnableLowUsageNotification || snapshot.RemainingPercent is not int percent) return;
        var reset = snapshot.ResetAt is { } time ? $"，将在 {time.LocalDateTime:g} 重置" : string.Empty;
        ShowNotification(new NotificationRequest
        {
            OverlayContent = NotificationContent.CreateSimpleTextContent($"Codex 剩余额度 {percent}%{reset}")
        });
    }
}
