using L5R.Engine.GameSteps.BotActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.GameSteps.BotActions;

public class WayOfThePhoenixBotActionTests
{
    [Test]
    public void IsLegal_WhenAnyRingIsUnrestricted_True()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var source = new Card { Id = "way-of-the-phoenix", Type = CardType.Event, Controller = p1 };

        Assert.That(new WayOfThePhoenixBotAction().IsLegal(game, source, p1), Is.True);
    }

    [Test]
    public void Invoke_RestrictsTheOpponentFromDeclaringThatRing()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var source = new Card { Id = "way-of-the-phoenix", Type = CardType.Event, Controller = p1 };

        new WayOfThePhoenixBotAction().Invoke(game, source, p1);

        var firstRing = game.Rings[0];
        Assert.That(game.CannotDeclareRingWith(p2, firstRing.Element), Is.True);
    }
}
