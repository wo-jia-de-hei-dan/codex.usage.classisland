namespace CodexUsageClassIsland;

public static class UsageDisplayFormatter
{
    public static string WeeklyPercent(int? remainingPercent) =>
        remainingPercent is int value ? $"剩余 {value}%" : "暂未提供";

    public static string FreeResetPercent(UsageAllowance? allowance) =>
        allowance is { } value ? $"剩余 {value.RemainingPercent}%" : "暂未提供";

    public static string ResetText(DateTimeOffset? resetAt) =>
        resetAt is { } value ? $"重置 {value.LocalDateTime:MM/dd HH:mm}" : "重置时间暂未提供";
}
