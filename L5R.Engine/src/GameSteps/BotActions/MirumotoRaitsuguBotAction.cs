using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps.BotActions;

/// <summary>mirumoto-raitsugu: same shape as KakitaKaezinBotAction, punishing the duel's loser instead of sending home the uninvolved.</summary>
public sealed class MirumotoRaitsuguBotAction : IBotScriptAction
{
    public bool IsLegal(GameState game, Card source, Player actingPlayer) =>
        FindTarget(game, source, actingPlayer) is not null;

    public void Invoke(GameState game, Card source, Player actingPlayer)
    {
        var target = FindTarget(game, source, actingPlayer)
            ?? throw new InvalidOperationException($"'{source.Id}' has no legal bot duel target.");

        var winner = DuelResolver.Resolve(game, source, target);
        var context = new AbilityContext { Game = game, Player = actingPlayer, Source = source, Target = target, DuelWinner = winner };
        new MirumotoRaitsuguDuelAndPunishLoser().Execute(context);
    }

    private static Card? FindTarget(GameState game, Card source, Player actingPlayer)
    {
        var conflict = game.CurrentConflict;
        if (conflict is null || (!conflict.Attackers.Contains(source) && !conflict.Defenders.Contains(source)))
            return null;

        var opponent = game.Opponent(actingPlayer);
        return conflict.Attackers.Concat(conflict.Defenders).FirstOrDefault(c => c.Controller == opponent);
    }
}
