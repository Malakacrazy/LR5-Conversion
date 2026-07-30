using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps.BotActions;

/// <summary>Phase B adapter for obstinate-recruit. No target - a bare stat comparison. Firing discards the card itself, so it can't be re-found by a later scan in the same or a later window.</summary>
public sealed class ObstinateRecruitBotAction : IBotScriptAction
{
    public bool IsLegal(GameState game, Card source, Player actingPlayer) =>
        actingPlayer.Honor < game.Opponent(actingPlayer).Honor;

    public void Invoke(GameState game, Card source, Player actingPlayer)
    {
        var context = new AbilityContext { Game = game, Player = actingPlayer, Source = source };
        new ObstinateRecruitDiscardWhenOpponentMoreHonorable().Execute(context);
    }
}
