using L5R.Engine.GameSteps.BotActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.GameSteps.BotActions;

public class SecludedTempleBotActionTests
{
    private static GameState NewScenario(out Player p1, out Player p2, out Card temple, out Card opponentCharacter)
    {
        p1 = new Player { Name = "Player1" };
        p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };

        temple = new Card { Id = "secluded-temple", Type = CardType.Holding, Controller = p1 };
        opponentCharacter = new Card { Id = "opponent-character", Type = CardType.Character, Controller = p2, Fate = 2 };
        p1.Provinces.Add(temple);
        p2.PlayArea.Add(opponentCharacter);
        p2.PlayArea.Add(new Card { Id = "opponent-character-2", Type = CardType.Character, Controller = p2 });

        return game;
    }

    [Test]
    public void IsLegal_WhenOutnumbered_True()
    {
        var game = NewScenario(out var p1, out _, out var temple, out _);

        Assert.That(new SecludedTempleBotAction().IsLegal(game, temple, p1), Is.True);
    }

    [Test]
    public void IsLegal_WhenNotOutnumbered_False()
    {
        var game = NewScenario(out var p1, out _, out var temple, out _);
        p1.PlayArea.Add(new Card { Id = "my-character", Type = CardType.Character, Controller = p1 });
        p1.PlayArea.Add(new Card { Id = "my-character-2", Type = CardType.Character, Controller = p1 });

        Assert.That(new SecludedTempleBotAction().IsLegal(game, temple, p1), Is.False);
    }

    [Test]
    public void Invoke_RemovesOneFateFromAnOpponentCharacter()
    {
        var game = NewScenario(out var p1, out _, out var temple, out var opponentCharacter);

        new SecludedTempleBotAction().Invoke(game, temple, p1);

        Assert.That(opponentCharacter.Fate, Is.EqualTo(1));
    }
}
