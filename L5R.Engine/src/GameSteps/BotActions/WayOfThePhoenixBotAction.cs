using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps.BotActions;

/// <summary>Phase B adapter for way-of-the-phoenix. Ring pool is any ring the opponent isn't already restricted from declaring.</summary>
public sealed class WayOfThePhoenixBotAction : IBotScriptAction
{
    public bool IsLegal(GameState game, Card source, Player actingPlayer) =>
        FindRing(game, actingPlayer) is not null;

    public void Invoke(GameState game, Card source, Player actingPlayer)
    {
        var ring = FindRing(game, actingPlayer)
            ?? throw new InvalidOperationException($"'{source.Id}' has no legal bot ring.");

        var context = new AbilityContext { Game = game, Player = actingPlayer, Source = source, TargetRing = ring };
        new WayOfThePhoenixPreventOpponentDeclaringRingElement().Execute(context);
    }

    private static Ring? FindRing(GameState game, Player actingPlayer)
    {
        var opponent = game.Opponent(actingPlayer);
        return game.Rings.FirstOrDefault(r => !game.CannotDeclareRingWith(opponent, r.Element));
    }
}
