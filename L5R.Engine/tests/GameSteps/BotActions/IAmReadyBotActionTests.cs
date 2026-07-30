using L5R.Engine.GameSteps.BotActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.GameSteps.BotActions;

public class IAmReadyBotActionTests
{
    [Test]
    public void IsLegal_WithABowedUnicornWithFate_True()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var unicorn = new Card { Id = "unicorn-character", Type = CardType.Character, Controller = p1, Faction = "unicorn", Bowed = true, Fate = 2 };
        p1.PlayArea.Add(unicorn);
        var iAmReady = new Card { Id = "i-am-ready", Type = CardType.Event, Controller = p1 };

        Assert.That(new IAmReadyBotAction().IsLegal(game, iAmReady, p1), Is.True);
    }

    [Test]
    public void IsLegal_WithNoFateOnTheBowedUnicorn_False()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var unicorn = new Card { Id = "unicorn-character", Type = CardType.Character, Controller = p1, Faction = "unicorn", Bowed = true, Fate = 0 };
        p1.PlayArea.Add(unicorn);
        var iAmReady = new Card { Id = "i-am-ready", Type = CardType.Event, Controller = p1 };

        Assert.That(new IAmReadyBotAction().IsLegal(game, iAmReady, p1), Is.False);
    }

    [Test]
    public void Invoke_RemovesOneFateAndReadiesTheCharacter()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var unicorn = new Card { Id = "unicorn-character", Type = CardType.Character, Controller = p1, Faction = "unicorn", Bowed = true, Fate = 2 };
        p1.PlayArea.Add(unicorn);
        var iAmReady = new Card { Id = "i-am-ready", Type = CardType.Event, Controller = p1 };

        new IAmReadyBotAction().Invoke(game, iAmReady, p1);

        Assert.That(unicorn.Fate, Is.EqualTo(1));
        Assert.That(unicorn.Bowed, Is.False);
    }
}
