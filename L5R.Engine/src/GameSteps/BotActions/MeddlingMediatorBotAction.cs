using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps.BotActions;

/// <summary>Phase B adapter for meddling-mediator. No target needed - just a ChosenChoice; the bot always takes fate (an arbitrary but consistent default, matching "first option" style choices elsewhere).</summary>
public sealed class MeddlingMediatorBotAction : IBotScriptAction
{
    public bool IsLegal(GameState game, Card source, Player actingPlayer) =>
        game.CurrentPhase == Phase.Conflict
        && game.ConflictDeclarationsThisPhase.Count(d => d.Player == game.Opponent(actingPlayer) && !d.Passed) > 1;

    public void Invoke(GameState game, Card source, Player actingPlayer)
    {
        var context = new AbilityContext { Game = game, Player = actingPlayer, Source = source, ChosenChoice = "Take 1 fate" };
        new MeddlingMediatorTakeFateOrHonorWhenDoublyAttacked().Execute(context);
    }
}
