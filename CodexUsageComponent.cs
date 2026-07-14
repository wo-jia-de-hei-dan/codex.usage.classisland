using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Threading;
using ClassIsland.Core.Abstractions.Controls;
using ClassIsland.Core.Attributes;

namespace CodexUsageClassIsland;

[ComponentInfo("6dc9d6ab-2c4a-4d3c-9b8e-1ce8b7d4d6e2", "Codex 使用量", "\uE8D2")]
public sealed class CodexUsageComponent : ComponentBase
{
    private readonly UsageService service;
    private readonly TextBlock percentText = new() { Text = "等待刷新", FontSize = 13, TextWrapping = Avalonia.Media.TextWrapping.Wrap };
    private readonly TextBlock statusText = new() { Text = "正在读取登录状态…", FontSize = 10, TextWrapping = Avalonia.Media.TextWrapping.Wrap };
    private readonly ProgressBar progress = new() { Minimum = 0, Maximum = 100, Height = 5, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch };
    private readonly TextBlock freeResetTitle = new() { Text = "免费重置额度", FontSize = 11, Opacity = 0.8, Margin = new Avalonia.Thickness(0, 8, 0, 0) };
    private readonly TextBlock freeResetPercentText = new() { Text = "暂未提供", FontSize = 13, TextWrapping = Avalonia.Media.TextWrapping.Wrap };
    private readonly TextBlock freeResetStatusText = new() { Text = "免费重置额度暂未提供", FontSize = 10, TextWrapping = Avalonia.Media.TextWrapping.Wrap };
    private readonly ProgressBar freeResetProgress = new() { Minimum = 0, Maximum = 100, Height = 5, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch, IsVisible = false };

    public CodexUsageComponent(UsageService service)
    {
        this.service = service;
        Plugin.Service = service;
        Content = new Border
        {
            Padding = new Avalonia.Thickness(8, 5),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            Child = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 16,
                Children =
                {
                    new StackPanel { Children = { new TextBlock { Text = "Codex 每周额度", FontSize = 11, Opacity = 0.8 }, percentText } },
                    new StackPanel { Children = { freeResetTitle, freeResetPercentText } }
                }
            }
        };
        service.SnapshotUpdated += OnSnapshotUpdated;
        service.Start();
    }

    private void OnSnapshotUpdated(object? sender, UsageSnapshot snapshot)
    {
        Dispatcher.UIThread.Post(() =>
        {
            percentText.Text = snapshot.RemainingPercent is int value ? $"剩余 {value}%" : snapshot.Message;
            progress.Value = snapshot.RemainingPercent ?? 0;
            statusText.Text = snapshot.ResetAt is { } reset ? $"重置 {reset.LocalDateTime:MM/dd HH:mm}" : snapshot.Message;
            freeResetPercentText.Text = snapshot.FreeResetCount is not null
                ? UsageDisplayFormatter.FreeResetCountText(snapshot.FreeResetCount)
                : UsageDisplayFormatter.FreeResetPercent(snapshot.FreeResetUsage);
            freeResetProgress.IsVisible = snapshot.FreeResetCount is null && snapshot.FreeResetUsage is not null;
            freeResetProgress.Value = snapshot.FreeResetUsage?.RemainingPercent ?? 0;
            freeResetStatusText.Text = snapshot.FreeResetCount is int
                ? "额度耗尽时可使用免费重置"
                : snapshot.FreeResetUsage is { ResetAt: { } freeReset }
                ? $"重置 {freeReset.LocalDateTime:MM/dd HH:mm}"
                : snapshot.FreeResetUsage is null ? "免费重置额度暂未提供" : "免费重置时间暂未提供";
        });
    }
}
