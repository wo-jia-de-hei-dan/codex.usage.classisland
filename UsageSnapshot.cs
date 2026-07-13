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

public sealed record UsageSnapshot(
    UsageStatus Status,
    int? RemainingPercent,
    DateTimeOffset? ResetAt,
    DateTimeOffset UpdatedAt,
    string Message)
{
    public static UsageSnapshot Unknown(string message) => new(UsageStatus.Unknown, null, null, DateTimeOffset.Now, message);
}
