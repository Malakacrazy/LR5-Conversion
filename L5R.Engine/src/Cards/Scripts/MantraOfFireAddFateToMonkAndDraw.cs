using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// mantra-of-fire: after an opponent declares a fire conflict, place 1 fate on a monk
/// character (or one with a monk attachment) and draw 1 card. Needs Conflict.Elements/
/// AttackingPlayer field inspection beyond the closed predicate vocabulary's event.card-only
/// convention, since onConflictDeclared events carry ring/conflict objects rather than a
/// card field.
/// </summary>
public sealed class MantraOfFireAddFateToMonkAndDraw : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var mantra = context.Source;

        var conflict = context.Game.CurrentConflict
            ?? throw new InvalidOperationException($"'{mantra.Id}' requires an active conflict.");

        if (!conflict.Elements.Contains("fire"))
            throw new InvalidOperationException($"'{mantra.Id}' can only trigger on a conflict declared with the fire element.");

        if (conflict.AttackingPlayer != context.Game.Opponent(context.Player))
            throw new InvalidOperationException($"'{mantra.Id}' can only trigger when the opponent declares the conflict.");

        var target = context.Target
            ?? throw new InvalidOperationException($"'{mantra.Id}' requires context.Target to be set.");

        var isMonk = target.Traits.Contains("monk")
            || context.Game.AllCards().Any(c => c.AttachedTo == target && c.Traits.Contains("monk"));
        if (!isMonk)
            throw new InvalidOperationException($"'{target.Id}' must be a monk, or have a monk attachment.");

        new PlaceFateGameActionHandler().Execute(context, null);
        new DrawGameActionHandler().Execute(context, null);
    }
}
