using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps.BotActions;

/// <summary>Phase B adapter for ide-trader. No target - just a bare participation check and a ChosenChoice; the bot always takes fate over drawing.</summary>
public sealed class IdeTraderBotAction : IBotScriptAction
{
    public bool IsLegal(GameState game, Card source, Player actingPlayer) =>
        game.CurrentConflict is { } conflict && (conflict.Attackers.Contains(source) || conflict.Defenders.Contains(source));

    public void Invoke(GameState game, Card source, Player actingPlayer)
    {
        var context = new AbilityContext { Game = game, Player = actingPlayer, Source = source, ChosenChoice = "Gain 1 fate" };
        new IdeTraderGainFateOrDrawOnAllyMovingToConflict().Execute(context);
    }
}
