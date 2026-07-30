using L5R.Engine.Abilities;
using L5R.Engine.State;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// way-of-the-phoenix: choose a ring; the opponent cannot declare a conflict with that
/// ring's element for the rest of the phase. Needs a ring-scoped bulk target (no ring
/// equivalent of allCardsMatching exists) and a player-filter-function effect value.
/// Appends a RingDeclarationRestriction (see its own doc comment for why this is a
/// queryable fact - GameState.CannotDeclareRingWith - rather than an enforced pipeline,
/// since no generic "declare a conflict" action exists to consult it automatically).
/// "max: perPhase(1)" needs no work, matching every other "max"/"limit" field's
/// established no-op precedent.
/// </summary>
public sealed class WayOfThePhoenixPreventOpponentDeclaringRingElement : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var wayOfThePhoenix = context.Source;

        var ring = context.TargetRing
            ?? throw new InvalidOperationException($"'{wayOfThePhoenix.Id}' requires context.TargetRing to be set.");

        var opponent = context.Game.Opponent(context.Player);

        context.Game.RingDeclarationRestrictions.Add(new RingDeclarationRestriction
        {
            Player = opponent,
            Element = ring.Element,
            Duration = "untilEndOfPhase"
        });
    }
}
