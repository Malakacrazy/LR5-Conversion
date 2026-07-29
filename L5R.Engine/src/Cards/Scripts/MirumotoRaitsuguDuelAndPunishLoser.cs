using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.State;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// mirumoto-raitsugu: while participating, duel a participating opponent character
/// (military type); the loser has 1 fate removed, or is discarded if it has none. A real
/// duel compares skill plus each side's honor bid (ringteki Duel.js), which this engine
/// doesn't model - context.DuelWinner is the caller-set fact instead (see its own doc
/// comment); the loser is derived as whichever of context.Source/context.Target isn't the
/// winner.
/// </summary>
public sealed class MirumotoRaitsuguDuelAndPunishLoser : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var raitsugu = context.Source;

        if (!IsParticipating(context.Game, raitsugu))
            throw new InvalidOperationException($"'{raitsugu.Id}' can only be used while participating.");

        var target = context.Target
            ?? throw new InvalidOperationException($"'{raitsugu.Id}' requires context.Target to be set.");

        if (target.Controller != context.Game.Opponent(context.Player))
            throw new InvalidOperationException($"'{target.Id}' must be controlled by the opponent.");

        if (!IsParticipating(context.Game, target))
            throw new InvalidOperationException($"'{target.Id}' is not participating.");

        var winner = context.DuelWinner
            ?? throw new InvalidOperationException($"'{raitsugu.Id}' requires context.DuelWinner to be set.");

        var loser = winner == raitsugu ? target : raitsugu;

        context.Target = loser;
        if (loser.Fate > 0)
            new RemoveFateGameActionHandler().Execute(context, null);
        else
            new DiscardFromPlayGameActionHandler().Execute(context, null);
    }

    private static bool IsParticipating(GameState game, Card card) =>
        game.CurrentConflict is { } conflict && (conflict.Attackers.Contains(card) || conflict.Defenders.Contains(card));
}
