using L5R.Engine.Abilities;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// display-of-power: after losing an unopposed conflict, claim the ring for its
/// controller instead of the winner. Needs Conflict.Loser/Unopposed field inspection and a
/// bespoke one-shot interrupt-cancel handler far beyond a single gameAction - matches this
/// card's own scriptOverride reason. Unlike pilgrimage (which suppresses ring resolution
/// entirely), this redirects *who* resolves and claims it: this script only claims the
/// ring for its controller; "cancel the ring effect [for the original winner]" needs no
/// extra work, since nothing else calls it - the caller simply invokes
/// ResolveConflictRingGameActionHandler afterward with context.Player set to display-of-
/// power's controller (already generic over whichever player is resolving), the same
/// "no new mechanism needed, just sequence the calls" reasoning as reprieve/stand-your-
/// ground/young-rumormonger.
/// </summary>
public sealed class DisplayOfPowerCancelAndClaimRing : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var displayOfPower = context.Source;

        var conflict = context.Game.CurrentConflict
            ?? throw new InvalidOperationException($"'{displayOfPower.Id}' requires an active conflict.");

        if (conflict.Loser != context.Player)
            throw new InvalidOperationException($"'{displayOfPower.Id}' can only trigger when its controller loses the conflict.");

        if (!conflict.Unopposed)
            throw new InvalidOperationException($"'{displayOfPower.Id}' can only trigger after an unopposed conflict.");

        var ring = context.TargetRing
            ?? throw new InvalidOperationException($"'{displayOfPower.Id}' requires context.TargetRing (the contested ring) to be set.");

        ring.Claimed = true;
        ring.ClaimedBy = context.Player;
    }
}
