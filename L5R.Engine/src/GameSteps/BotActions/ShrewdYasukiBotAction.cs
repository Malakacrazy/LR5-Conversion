using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps.BotActions;

/// <summary>Phase B adapter for shrewd-yasuki. Always keeps the top card of the deck (the simplest possible choice from the top-2 pool) rather than evaluating which of the two is better.</summary>
public sealed class ShrewdYasukiBotAction : IBotScriptAction
{
    public bool IsLegal(GameState game, Card source, Player actingPlayer) =>
        IsParticipating(game, source)
        && game.Player1.Provinces.Concat(game.Player2.Provinces).Any(c => c.Type == CardType.Holding && !c.Facedown)
        && actingPlayer.Deck.Count > 0;

    public void Invoke(GameState game, Card source, Player actingPlayer)
    {
        var context = new AbilityContext { Game = game, Player = actingPlayer, Source = source, ChosenDeckSearchCard = actingPlayer.Deck[0] };
        new ShrewdYasukiLookAtTopTwoKeepOne().Execute(context);
    }

    private static bool IsParticipating(GameState game, Card card) =>
        game.CurrentConflict is { } conflict && (conflict.Attackers.Contains(card) || conflict.Defenders.Contains(card));
}
