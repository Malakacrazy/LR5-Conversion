using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps.BotActions;

/// <summary>the-perfect-gift: no conflict requirement, just needs both decks non-empty. Picks the top card of each deck as the "first legal candidate".</summary>
public sealed class ThePerfectGiftBotAction : IBotScriptAction
{
    public bool IsLegal(GameState game, Card source, Player actingPlayer) =>
        actingPlayer.Deck.Count > 0 && game.Opponent(actingPlayer).Deck.Count > 0;

    public void Invoke(GameState game, Card source, Player actingPlayer)
    {
        var opponent = game.Opponent(actingPlayer);
        var context = new AbilityContext
        {
            Game = game, Player = actingPlayer, Source = source,
            ChosenCardMenuCard = actingPlayer.Deck.First(),
            ChosenDeckSearchCard = opponent.Deck.First()
        };
        new ThePerfectGiftRevealAndGiveEachPlayerACard().Execute(context);
    }
}
