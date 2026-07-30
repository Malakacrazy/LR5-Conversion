using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Dsl;

public class WayOfTheUnicornOffererTests
{
    [Test]
    public void TryPlay_WithTheCardInHand_CancelsThePendingFirstPlayerPass()
    {
        var p1 = new Player { Name = "Player1", Fate = 2 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var card = new Card { Id = "way-of-the-unicorn", Type = CardType.Event, Controller = p1, Location = "hand", PrintedCost = 0 };
        p1.Hand.Add(card);

        WayOfTheUnicornOfferer.TryPlay(game, p1);

        Assert.That(game.FirstPlayerPassCancelled, Is.True);
        Assert.That(p1.Hand, Does.Not.Contain(card));
        Assert.That(p1.Discard, Contains.Item(card));
    }

    [Test]
    public void TryPlay_WithoutTheCardInHand_DoesNothing()
    {
        var p1 = new Player { Name = "Player1", Fate = 2 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };

        WayOfTheUnicornOfferer.TryPlay(game, p1);

        Assert.That(game.FirstPlayerPassCancelled, Is.False);
    }

    [Test]
    public void TryPlay_WhenUnaffordable_DoesNotPlayIt()
    {
        var p1 = new Player { Name = "Player1", Fate = 0 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var card = new Card { Id = "way-of-the-unicorn", Type = CardType.Event, Controller = p1, Location = "hand", PrintedCost = 2 };
        p1.Hand.Add(card);

        WayOfTheUnicornOfferer.TryPlay(game, p1);

        Assert.That(game.FirstPlayerPassCancelled, Is.False);
        Assert.That(p1.Hand, Contains.Item(card));
    }
}
