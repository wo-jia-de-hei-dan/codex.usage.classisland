using CommunityToolkit.Mvvm.ComponentModel;

namespace CodexUsageClassIsland;

public sealed partial class PluginSettings : ObservableObject
{
    [ObservableProperty] private string? authFilePath;
    [ObservableProperty] private int refreshIntervalMinutes = 5;
    [ObservableProperty] private int lowThresholdPercent = 20;
    [ObservableProperty] private bool enableLowUsageNotification;
    [ObservableProperty] private bool notifyOnlyOnThresholdCrossing = true;
}
