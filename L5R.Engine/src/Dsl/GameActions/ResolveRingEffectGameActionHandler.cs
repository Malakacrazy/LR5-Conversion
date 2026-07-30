using System.Text.Json;
using L5R.Engine.Abilities;

namespace L5R.Engine.Dsl.GameActions;

/// <summary>
/// ringteki resolveRingEffect (shiba-tsukune's own "resolve up to 2 rings" at the end of
/// the conflict phase): resolves an arbitrary unclaimed ring's own effect (context.
/// TargetRing) with no conflict required, distinct from resolveConflictRing (which always
/// resolves the current conflict's own declared ring). Shares the actual per-element
/// dispatch with ResolveConflictRingGameActionHandler.ResolveElement rather than
/// duplicating the five elements' own effects.
/// </summary>
public sealed class ResolveRingEffectGameActionHandler : IGameActionHandler
{
    public void Execute(AbilityContext context, JsonElement? parameters)
    {
        var ring = context.TargetRing
            ?? throw new InvalidOperationException("resolveRingEffect requires context.TargetRing to be set.");

        if (ring.Claimed)
            throw new InvalidOperationException($"The {ring.Element} ring is already claimed and cannot be resolved this way.");

        ResolveConflictRingGameActionHandler.ResolveElement(ring.Element, context);
    }
}
