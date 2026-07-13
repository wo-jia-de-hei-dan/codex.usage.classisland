using ClassIsland.Core.Abstractions.Automation;
using ClassIsland.Core.Attributes;

namespace CodexUsageClassIsland;

[TriggerInfo("codex.usage.low", "Codex 剩余额度低于阈值", "\uE8D2")]
public sealed class CodexUsageLowTrigger : TriggerBase
{
    private readonly UsageService service;

    public CodexUsageLowTrigger(UsageService service) => this.service = service;

    public override void Loaded() => service.ThresholdCrossed += OnThresholdCrossed;

    public override void UnLoaded() => service.ThresholdCrossed -= OnThresholdCrossed;

    private void OnThresholdCrossed(object? sender, UsageSnapshot snapshot) => Trigger();
}
