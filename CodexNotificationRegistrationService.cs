using ClassIsland.Core.Abstractions.Services;
using Microsoft.Extensions.Hosting;

namespace CodexUsageClassIsland;

public sealed class CodexNotificationRegistrationService(
    INotificationHostService notificationHost,
    CodexNotificationProvider provider) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        notificationHost.RegisterNotificationProvider(provider);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
