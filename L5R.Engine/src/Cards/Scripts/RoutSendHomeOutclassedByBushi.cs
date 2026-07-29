using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.State;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// rout: send an opponent's character home if you control a participating Bushi with
/// higher military skill than it. ringteki's cardCondition needs a double-candidate
/// comparison (the target vs. an existentially-checked Bushi) - anyCardMatches's own "of"
/// predicate only has a single local candidate slot, with no way to also reference the
/// outer target at the same time. A script has no such restriction: it just scans
/// context.Player.PlayArea directly.
/// </summary>
public sealed class RoutSendHomeOutclassedByBushi : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var target = context.Target
            ?? throw new InvalidOperationException("rout requires context.Target to be set.");

        if (target.Controller != context.Game.Opponent(context.Player))
            throw new InvalidOperationException($"'{target.Id}' must be controlled by the opponent.");

        var hasOutclassingBushi = context.Player.PlayArea.Any(myCard =>
            myCard.Traits.Contains("bushi") && IsParticipating(context.Game, myCard)
            && context.Game.EffectiveMilitarySkill(myCard) > context.Game.EffectiveMilitarySkill(target));

        if (!hasOutclassingBushi)
            throw new InvalidOperationException($"No participating Bushi you control outclasses '{target.Id}' in military skill.");

        new SendHomeGameActionHandler().Execute(context, null);
    }

    private static bool IsParticipating(GameState game, Card card) =>
        game.CurrentConflict is { } conflict && (conflict.Attackers.Contains(card) || conflict.Defenders.Contains(card));
}
