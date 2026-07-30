using L5R.Engine.GameSteps.BotActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.GameSteps.BotActions;

public class ThePerfectGiftBotActionTests
{
    private static (GameState game, Card tpg, Card myTop, Card opponentTop) NewScenario()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var tpg = new Card { Id = "the-perfect-gift", Type = CardType.Event, Controller = p1 };
        var myTop = new Card { Id = "my-top", Type = CardType.Character, Controller = p1 };
        var opponentTop = new Card { Id = "opponent-top", Type = CardType.Character, Controller = p2 };
        p1.Deck.Add(myTop);
        p2.Deck.Add(opponentTop);
        return (game, tpg, myTop, opponentTop);
    }

    [Test]
    public void IsLegal_WithBothDecksNonEmpty_True()
    {
        var (game, tpg, _, _) = NewScenario();
        Assert.That(new ThePerfectGiftBotAction().IsLegal(game, tpg, game.Player1), Is.True);
    }

    [Test]
    public void Invoke_GivesEachPlayerTheTopOfTheirOwnDeck()
    {
        var (game, tpg, myTop, opponentTop) = NewScenario();
        new ThePerfectGiftBotAction().Invoke(game, tpg, game.Player1);

        Assert.That(game.Player1.Hand, Contains.Item(myTop));
        Assert.That(game.Player2.Hand, Contains.Item(opponentTop));
    }
}
