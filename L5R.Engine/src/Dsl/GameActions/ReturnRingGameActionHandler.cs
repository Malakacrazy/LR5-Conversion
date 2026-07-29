using System.Text.Json;
using L5R.Engine.Abilities;

namespace L5R.Engine.Dsl.GameActions;

/// <summary>
/// ringteki ReturnRingAction: resets a claimed/contested ring back to unclaimed
/// (ring.js's resetRing() - clears Claimed/ClaimedBy/Contested; fate is untouched).
/// canAffect requires the ring not already be unclaimed.
/// </summary>
public sealed class ReturnRingGameActionHandler : IGameActionHandler
{
    public bool CanAffect(AbilityContext context, JsonElement? parameters) =>
        context.TargetRing is { IsUnclaimed: false };

    public void Execute(AbilityContext context, JsonElement? parameters)
    {
        var ring = context.TargetRing
            ?? throw new InvalidOperationException("returnRing requires context.TargetRing to be set.");

        ring.Claimed = false;
        ring.ClaimedBy = null;
        ring.Contested = false;
    }
}
