using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps.BotActions;

/// <summary>vengeful-oathkeeper: Conflict.Loser is settled by the post-resolution window. Must still be sitting in hand - it puts itself into play, joining the conflict on its controller's side.</summary>
public sealed class VengefulOathkeeperBotAction : IBotScriptAction
{
    public bool IsLegal(GameState game, Card source, Player actingPlayer) =>
        actingPlayer.Hand.Contains(source)
        && game.CurrentConflict is { ConflictType: "military" } conflict
        && conflict.Loser == actingPlayer;

    public void Invoke(GameState game, Card source, Player actingPlayer)
    {
        var context = new AbilityContext { Game = game, Player = actingPlayer, Source = source };
        new VengefulOathkeeperPutIntoPlayOnMilitaryLoss().Execute(context);
    }
}
