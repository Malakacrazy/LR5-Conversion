using L5R.Engine.GameSteps.BotActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.GameSteps.BotActions;

public class AsceticVisionaryBotActionTests
{
    private static GameState NewAttackingScenario(out Player p1, out Card visionary, out Card bowedMonk)
    {
        p1 = new Player { Name = "Player1", Fate = 3 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        game.Rings[0].Claimed = false;

        visionary = new Card { Id = "ascetic-visionary", Type = CardType.Character, Controller = p1 };
        bowedMonk = new Card { Id = "bowed-monk", Type = CardType.Character, Controller = p1, Traits = new[] { "monk" }, Bowed = true };
        p1.PlayArea.Add(visionary);
        p1.PlayArea.Add(bowedMonk);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(visionary);
        game.CurrentConflict = conflict;

        return game;
    }

    [Test]
    public void IsLegal_WhileAttackingWithFateAndABowedMonk_True()
    {
        var game = NewAttackingScenario(out var p1, out var visionary, out _);

        Assert.That(new AsceticVisionaryBotAction().IsLegal(game, visionary, p1), Is.True);
    }

    [Test]
    public void IsLegal_WithoutEnoughFate_False()
    {
        var game = NewAttackingScenario(out var p1, out var visionary, out _);
        p1.Fate = 0;

        Assert.That(new AsceticVisionaryBotAction().IsLegal(game, visionary, p1), Is.False);
    }

    [Test]
    public void Invoke_PaysFateAndReadiesTheMonk()
    {
        var game = NewAttackingScenario(out var p1, out var visionary, out var bowedMonk);

        new AsceticVisionaryBotAction().Invoke(game, visionary, p1);

        Assert.That(bowedMonk.Bowed, Is.False);
        Assert.That(p1.Fate, Is.EqualTo(2));
    }
}
