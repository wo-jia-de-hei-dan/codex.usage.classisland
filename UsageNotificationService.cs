using ClassIsland.Core.Abstractions.Services.NotificationProviders;
using ClassIsland.Core.Models.Notification;

namespace CodexUsageClassIsland;

public sealed class UsageNotificationService : IDisposable
{
    private readonly PluginSettings settings;
    private readonly UsageService usage;
    private readonly INotificationSender sender;

    public UsageNotificationService(PluginSettings settings, UsageService usage, INotificationSender sender)
    {
        this.settings = settings;
        this.usage = usage;
        this.sender = sender;
        usage.ThresholdCrossed += OnThresholdCrossed;
    }

    private void OnThresholdCrossed(object? sender, UsageSnapshot snapshot)
    {
        if (!settings.EnableLowUsageNotification || snapshot.RemainingPercent is not int percent) return;
        var reset = snapshot.ResetAt is { } time ? $"，将在 {time.LocalDateTime:g} 重置" : string.Empty;
        this.sender.ShowNotification(new NotificationRequest
        {
            OverlayContent = NotificationContent.CreateSimpleTextContent($"Codex 剩余额度 {percent}%{reset}")
        });
    }

    public void Dispose() => usage.ThresholdCrossed -= OnThresholdCrossed;
}
