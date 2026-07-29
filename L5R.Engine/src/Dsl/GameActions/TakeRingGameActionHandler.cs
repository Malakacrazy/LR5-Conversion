using System.Text.Json;
using L5R.Engine.Abilities;

namespace L5R.Engine.Dsl.GameActions;

/// <summary>
/// ringteki TakeRingAction: claims a ring for context.Player (ring.js's claimRing() -
/// sets Claimed/ClaimedBy, clears Contested) and, per its defaultProperties ({takeFate:
/// true}), transfers the ring's fate to context.Player and zeroes it. canAffect requires
/// the ring not already claimed by context.Player. "takeFate": false isn't supported
/// (throws) - no ported card omits it.
/// </summary>
public sealed class TakeRingGameActionHandler : IGameActionHandler
{
    public bool CanAffect(AbilityContext context, JsonElement? parameters) =>
        context.TargetRing is { } ring && ring.ClaimedBy != context.Player;

    public void Execute(AbilityContext context, JsonElement? parameters)
    {
        var ring = context.TargetRing
            ?? throw new InvalidOperationException("takeRing requires context.TargetRing to be set.");

        var takeFate = parameters?.TryGetProperty("takeFate", out var takeFateElement) == true ? takeFateElement.GetBoolean() : true;
        if (!takeFate)
            throw new NotSupportedException("takeRing does not yet support 'takeFate: false'.");

        ring.Claimed = true;
        ring.ClaimedBy = context.Player;
        ring.Contested = false;

        context.Player.Fate += ring.Fate;
        ring.Fate = 0;
    }
}
