using System.ComponentModel;
using ClassIsland.Core.Abstractions.Services;

namespace CodexUsageClassIsland;

public sealed class UsageService : IDisposable
{
    private readonly PluginSettings settings;
    private readonly CodexUsageClient client;
    private readonly IRulesetService rulesetService;
    private readonly SemaphoreSlim refreshGate = new(1, 1);
    private CancellationTokenSource shutdown = new();
    private Timer? timer;
    private bool started;
    private UsageSnapshot? lastSuccess;

    public UsageService(PluginSettings settings, CodexUsageClient client, IRulesetService rulesetService)
    {
        this.settings = settings;
        this.client = client;
        this.rulesetService = rulesetService;
        Plugin.Service = this;
        settings.PropertyChanged += SettingsChanged;
    }

    public UsageSnapshot Current { get; private set; } = UsageSnapshot.Unknown("等待刷新");
    public bool IsBelowThreshold => Current.RemainingPercent is int value && value <= settings.LowThresholdPercent;
    public event EventHandler<UsageSnapshot>? SnapshotUpdated;
    public event EventHandler<UsageSnapshot>? ThresholdCrossed;

    public void Start()
    {
        if (started) { timer?.Change(TimeSpan.Zero, TimeSpan.FromMinutes(Math.Clamp(settings.RefreshIntervalMinutes, 1, 60))); return; }
        started = true;
        timer = new Timer(_ => _ = RefreshAsync(), null, TimeSpan.Zero, TimeSpan.FromMinutes(Math.Clamp(settings.RefreshIntervalMinutes, 1, 60)));
    }

    public async Task RefreshAsync()
    {
        if (!await refreshGate.WaitAsync(0)) return;
        try
        {
            var previousLow = IsBelowThreshold;
            var snapshot = await client.FetchAsync(settings.AuthFilePath, shutdown.Token);
            if (snapshot.Status == UsageStatus.Success) lastSuccess = snapshot;
            else if (lastSuccess is not null)
            {
                snapshot = snapshot with
                {
                    RemainingPercent = lastSuccess.RemainingPercent,
                    ResetAt = lastSuccess.ResetAt,
                    FreeResetUsage = lastSuccess.FreeResetUsage
                };
            }
            Current = snapshot;
            SnapshotUpdated?.Invoke(this, snapshot);
            rulesetService.NotifyStatusChanged();
            if (!previousLow && IsBelowThreshold) ThresholdCrossed?.Invoke(this, snapshot);
        }
        finally { refreshGate.Release(); }
    }

    private void SettingsChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PluginSettings.RefreshIntervalMinutes)) Start();
    }

    public void Dispose()
    {
        settings.PropertyChanged -= SettingsChanged;
        shutdown.Cancel();
        started = false;
        timer?.Dispose();
        refreshGate.Dispose();
        shutdown.Dispose();
    }
}
