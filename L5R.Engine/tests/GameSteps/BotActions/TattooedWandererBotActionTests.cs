using L5R.Engine.GameSteps.BotActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.GameSteps.BotActions;

public class TattooedWandererBotActionTests
{
    private static GameState NewScenario(out Player p1, out Card wanderer, out Card ally)
    {
        p1 = new Player { Name = "Player1", Fate = 3 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };

        wanderer = new Card { Id = "tattooed-wanderer", Type = CardType.Character, Controller = p1, Location = "hand", PrintedCost = 2 };
        ally = new Card { Id = "ally", Type = CardType.Character, Controller = p1 };
        p1.Hand.Add(wanderer);
        p1.PlayArea.Add(ally);

        return game;
    }

    [Test]
    public void IsLegal_InHandWithALegalAttachTargetAndEnoughFate_True()
    {
        var game = NewScenario(out var p1, out var wanderer, out _);

        Assert.That(new TattooedWandererBotAction().IsLegal(game, wanderer, p1), Is.True);
    }

    [Test]
    public void IsLegal_WhenNotInHand_False()
    {
        var game = NewScenario(out var p1, out var wanderer, out _);
        p1.Hand.Remove(wanderer);
        p1.PlayArea.Add(wanderer);

        Assert.That(new TattooedWandererBotAction().IsLegal(game, wanderer, p1), Is.False);
    }

    [Test]
    public void Invoke_MovesItToPlayAreaAttachedToTheTarget()
    {
        var game = NewScenario(out var p1, out var wanderer, out var ally);

        new TattooedWandererBotAction().Invoke(game, wanderer, p1);

        Assert.That(p1.Hand, Does.Not.Contain(wanderer));
        Assert.That(p1.PlayArea, Contains.Item(wanderer));
        Assert.That(wanderer.AttachedTo, Is.EqualTo(ally));
        Assert.That(p1.Fate, Is.EqualTo(1));
    }
}
