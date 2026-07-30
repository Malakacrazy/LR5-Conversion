using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.State;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// shiba-tsukune: as the conflict phase ends, resolve an unclaimed ring's own effect
/// (context.TargetRing), reusing the new ResolveRingEffectGameActionHandler directly.
/// Ringteki's own two-step "choose a ring, then optionally a second" selection prompt is a
/// custom UI concern this engine's trust-the-caller convention already sidesteps
/// everywhere else - "up to 2 rings" is simply the caller invoking Execute up to twice,
/// once per chosen ring, the same way every other bulk/repeatable choice this session is
/// modeled (e.g. giver-of-gifts' own single-attachment-per-call shape).
/// </summary>
public sealed class ShibaTsukuneResolveUpToTwoRings : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var tsukune = context.Source;

        if (context.Game.CurrentPhase != Phase.Conflict)
            throw new InvalidOperationException($"'{tsukune.Id}' can only trigger as the conflict phase ends.");

        new ResolveRingEffectGameActionHandler().Execute(context, null);
    }
}
