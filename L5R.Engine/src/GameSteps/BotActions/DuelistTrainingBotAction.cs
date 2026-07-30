using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps.BotActions;

/// <summary>
/// duelist-training: unlike every other attachment-granted adapter this session, the script
/// itself reads context.Source directly as the host character (not context.Source.AttachedTo) -
/// its own doc comment confirms the attachment grants the action to its host, so the caller
/// is expected to already pass the host as Source. Always pays a bid-difference cost with
/// honor rather than discarding cards - TakeHonorGameActionHandler's own convention (no
/// affordability check, floors at 0) means it's always legal, avoiding the need to pick
/// specific cards from the low bidder's hand.
/// </summary>
public sealed class DuelistTrainingBotAction : IBotScriptAction
{
    public bool IsLegal(GameState game, Card source, Player actingPlayer) =>
        FindHostAndTarget(game, source, actingPlayer) is not null;

    public void Invoke(GameState game, Card source, Player actingPlayer)
    {
        var (host, target) = FindHostAndTarget(game, source, actingPlayer)
            ?? throw new InvalidOperationException($"'{source.Id}' has no legal bot duel.");

        var winner = DuelResolver.Resolve(game, host, target);
        var context = new AbilityContext
        {
            Game = game, Player = actingPlayer, Source = host, Target = target, DuelWinner = winner,
            ChosenChoice = actingPlayer.ShowBid != game.Opponent(actingPlayer).ShowBid ? "Pay with honor" : null
        };
        new DuelistTrainingGrantMilitaryDuelAction().Execute(context);
    }

    private static (Card Host, Card Target)? FindHostAndTarget(GameState game, Card source, Player actingPlayer)
    {
        var host = source.AttachedTo;
        if (host is null)
            return null;

        var conflict = game.CurrentConflict;
        if (conflict is null || (!conflict.Attackers.Contains(host) && !conflict.Defenders.Contains(host)))
            return null;

        var opponent = game.Opponent(actingPlayer);
        var target = conflict.Attackers.Concat(conflict.Defenders).FirstOrDefault(c => c.Controller == opponent);

        return target is not null ? (host, target) : null;
    }
}
