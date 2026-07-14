namespace CodexUsageClassIsland;

public enum UsageStatus
{
    Unknown,
    Success,
    NotLoggedIn,
    NetworkError,
    IncompatibleResponse,
    ServerRejected
}

public sealed record UsageAllowance(int RemainingPercent, DateTimeOffset? ResetAt);

public sealed record UsageSnapshot(
    UsageStatus Status,
    int? RemainingPercent,
    DateTimeOffset? ResetAt,
    DateTimeOffset UpdatedAt,
    string Message)
{
    public UsageAllowance? FreeResetUsage { get; init; }
    public int? FreeResetCount { get; init; }

    public static UsageSnapshot Unknown(string message) => new(UsageStatus.Unknown, null, null, DateTimeOffset.Now, message);
}
