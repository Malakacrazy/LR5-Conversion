using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.State;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// outwit: send an opponent's character home if you control a participating Courtier with
/// higher political skill than it. Same double-candidate comparison gap as
/// RoutSendHomeOutclassedByBushi (political instead of military) - scans context.Player.
/// PlayArea directly rather than needing anyCardMatches's single-candidate "of" predicate.
/// </summary>
public sealed class OutwitSendHomeOutclassedByCourtier : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var target = context.Target
            ?? throw new InvalidOperationException("outwit requires context.Target to be set.");

        if (target.Controller != context.Game.Opponent(context.Player))
            throw new InvalidOperationException($"'{target.Id}' must be controlled by the opponent.");

        var hasOutclassingCourtier = context.Player.PlayArea.Any(myCard =>
            myCard.Traits.Contains("courtier") && IsParticipating(context.Game, myCard)
            && context.Game.EffectivePoliticalSkill(myCard) > context.Game.EffectivePoliticalSkill(target));

        if (!hasOutclassingCourtier)
            throw new InvalidOperationException($"No participating Courtier you control outclasses '{target.Id}' in political skill.");

        new SendHomeGameActionHandler().Execute(context, null);
    }

    private static bool IsParticipating(GameState game, Card card) =>
        game.CurrentConflict is { } conflict && (conflict.Attackers.Contains(card) || conflict.Defenders.Contains(card));
}
