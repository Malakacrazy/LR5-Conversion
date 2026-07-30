using L5R.Engine.Abilities;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// pilgrimage: during conflicts declared against this province, cancel the ring's own
/// effect (unless the province itself is broken or blanked). Needs raw event registration
/// across two event/stage pairs with a bespoke shared handler, far beyond a single
/// triggeredAbility - matches this card's own scriptOverride reason. Rather than a
/// targeted card restriction (no "target card" makes sense for "the ring's own effect
/// can't resolve"), this sets Conflict.RingEffectsCancelled directly - see its own doc
/// comment for why. The caller invokes this once, exactly like every other passive/
/// reactive script this session, to represent "pilgrimage successfully suppressed this";
/// ResolveConflictRingGameActionHandler checks the flag before resolving anything.
/// </summary>
public sealed class PilgrimageCancelRingEffectsAtThisProvince : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var pilgrimage = context.Source;

        var conflict = context.Game.CurrentConflict
            ?? throw new InvalidOperationException($"'{pilgrimage.Id}' requires an active conflict.");

        if (conflict.DeclaredProvince != pilgrimage)
            throw new InvalidOperationException($"'{pilgrimage.Id}' can only cancel ring effects for a conflict declared against it.");

        if (pilgrimage.Broken)
            throw new InvalidOperationException($"'{pilgrimage.Id}' cannot cancel ring effects once broken.");

        if (context.Game.IsBlanked(pilgrimage))
            throw new InvalidOperationException($"'{pilgrimage.Id}' cannot cancel ring effects while blanked.");

        conflict.RingEffectsCancelled = true;
    }
}
