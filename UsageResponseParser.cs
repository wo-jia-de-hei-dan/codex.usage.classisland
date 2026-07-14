using System.Text.Json;

namespace CodexUsageClassIsland;

public sealed class UsageResponseParser
{
    public UsageSnapshot Parse(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            var percent = FindNumber(root, "remaining_percent", "remainingPercentage", "remaining_percentile");
            if (percent is null)
            {
                var used = FindNumber(root, "used_percent", "usedPercentage", "usage_percent");
                if (used is not null) percent = 100 - used;
            }
            var reset = FindDate(root, "reset_at", "resetAt", "resets_at", "next_reset_at");
            var value = NormalizePercent(percent);
            if (value is null) return new(UsageStatus.IncompatibleResponse, null, reset, DateTimeOffset.Now, "接口返回中没有可识别的额度百分比");
            var snapshot = new UsageSnapshot(UsageStatus.Success, value, reset, DateTimeOffset.Now, "已更新");
            return snapshot with { FreeResetUsage = ParseFreeReset(root), FreeResetCount = ParseFreeResetCount(root) };
        }
        catch (JsonException)
        {
            return new(UsageStatus.IncompatibleResponse, null, null, DateTimeOffset.Now, "接口返回不是有效 JSON");
        }
    }

    private static double? FindNumber(JsonElement root, params string[] names)
    {
        foreach (var name in names)
        {
            if (TryFind(root, name, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out var number)) return number;
        }
        return null;
    }

    private static UsageAllowance? ParseFreeReset(JsonElement root)
    {
        if (!TryFindObject(root, out var freeReset, "free_reset", "freeReset", "free_reset_usage")) return null;
        var percent = NormalizePercent(FindNumber(freeReset, "remaining_percent", "remainingPercentage", "remaining_percentile"));
        return percent is int value ? new UsageAllowance(value, FindDate(freeReset, "reset_at", "resetAt", "resets_at", "next_reset_at")) : null;
    }

    private static int? ParseFreeResetCount(JsonElement root)
    {
        if (!TryFindObject(root, out var resetCredits, "rate_limit_reset_credits", "rateLimitResetCredits")) return null;
        var count = FindNumber(resetCredits, "available_count", "availableCount");
        return count is >= 0 and <= int.MaxValue ? (int)Math.Round(count.Value) : null;
    }

    private static int? NormalizePercent(double? percent)
    {
        if (percent is null || double.IsNaN(percent.Value) || double.IsInfinity(percent.Value)) return null;
        var normalized = percent.Value <= 1 ? percent.Value * 100 : percent.Value;
        return normalized is >= 0 and <= 100 ? (int)Math.Round(normalized) : null;
    }

    private static DateTimeOffset? FindDate(JsonElement root, params string[] names)
    {
        foreach (var name in names)
        {
            if (!TryFind(root, name, out var value)) continue;
            if (value.ValueKind == JsonValueKind.String && DateTimeOffset.TryParse(value.GetString(), out var parsed)) return parsed;
            if (value.ValueKind == JsonValueKind.Number && value.TryGetInt64(out var unix)) return DateTimeOffset.FromUnixTimeSeconds(unix);
        }
        return null;
    }

    private static bool TryFind(JsonElement element, string name, out JsonElement value)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase)) { value = property.Value; return true; }
                if (TryFind(property.Value, name, out value)) return true;
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray()) if (TryFind(item, name, out value)) return true;
        }
        value = default;
        return false;
    }

    private static bool TryFindObject(JsonElement element, out JsonElement value, params string[] names)
    {
        foreach (var name in names)
        {
            if (TryFind(element, name, out var candidate) && candidate.ValueKind == JsonValueKind.Object)
            {
                value = candidate;
                return true;
            }
        }
        value = default;
        return false;
    }
}
