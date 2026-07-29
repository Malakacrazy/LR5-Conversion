using System.Text.Json;
using L5R.Engine.Abilities;

namespace L5R.Engine.Dsl.GameActions;

/// <summary>
/// ringteki FlipDynastyAction: flips a facedown province card faceup. The real canAffect
/// also checks isInProvince()/not-stronghold/isDynasty, but every ported card's own
/// cardCondition already filters to "location": "province" + "isFacedown" before this ever
/// runs (daidoji-nerishma/staging-ground) - ResolveLegalTargets is what a test uses to prove
/// legality; Execute itself doesn't re-derive it, same trust-the-caller convention used
/// throughout this engine.
/// </summary>
public sealed class FlipDynastyGameActionHandler : IGameActionHandler
{
    public bool CanAffect(AbilityContext context, JsonElement? parameters) =>
        context.Target is { Facedown: true };

    public void Execute(AbilityContext context, JsonElement? parameters)
    {
        if (context.Target is null)
            throw new InvalidOperationException("flipDynasty requires context.Target to be set.");

        context.Target.Facedown = false;
    }
}
