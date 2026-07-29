using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// endless-plains: after a conflict is declared against this province, pay its own "break
/// self" cost (Card.Broken - see its own doc comment) to force the attacking player to
/// discard an attacking character. Needs Conflict.DeclaredProvince field inspection beyond
/// the closed predicate vocabulary.
/// </summary>
public sealed class EndlessPlainsBreakAndDiscardAttacker : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var endlessPlains = context.Source;

        var conflict = context.Game.CurrentConflict
            ?? throw new InvalidOperationException($"'{endlessPlains.Id}' requires an active conflict.");

        if (conflict.DeclaredProvince != endlessPlains)
            throw new InvalidOperationException($"'{endlessPlains.Id}' can only trigger when the conflict is declared against it.");

        if (endlessPlains.Broken)
            throw new InvalidOperationException($"'{endlessPlains.Id}' is already broken.");

        var target = context.Target
            ?? throw new InvalidOperationException($"'{endlessPlains.Id}' requires context.Target to be set.");

        if (target.Controller != conflict.AttackingPlayer || !conflict.Attackers.Contains(target))
            throw new InvalidOperationException($"'{target.Id}' must be an attacking character.");

        endlessPlains.Broken = true;
        new DiscardFromPlayGameActionHandler().Execute(context, null);
    }
}
