using L5R.Engine.GameSteps.BotActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.GameSteps.BotActions;

public class SolemnScholarBotActionTests
{
    private static GameState NewGameWithClaimedEarth(out Player p1, out Player p2, out Card scholar, out Card attacker)
    {
        p1 = new Player { Name = "Player1" };
        p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        game.Rings.Single(r => r.Element == "earth").ClaimedBy = p1;

        scholar = new Card { Id = "solemn-scholar", Type = CardType.Character, Controller = p1 };
        attacker = new Card { Id = "attacker", Type = CardType.Character, Controller = p2 };
        p1.PlayArea.Add(scholar);
        p2.PlayArea.Add(attacker);

        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1 };
        conflict.Attackers.Add(attacker);
        game.CurrentConflict = conflict;

        return game;
    }

    [Test]
    public void IsLegal_WhenEarthClaimedAndAnAttackerExists_True()
    {
        var game = NewGameWithClaimedEarth(out var p1, out _, out var scholar, out _);

        Assert.That(new SolemnScholarBotAction().IsLegal(game, scholar, p1), Is.True);
    }

    [Test]
    public void IsLegal_WhenEarthNotClaimedByThisPlayer_False()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var scholar = new Card { Id = "solemn-scholar", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(scholar);

        Assert.That(new SolemnScholarBotAction().IsLegal(game, scholar, p1), Is.False);
    }

    [Test]
    public void Invoke_BowsTheFirstAttacker()
    {
        var game = NewGameWithClaimedEarth(out var p1, out _, out var scholar, out var attacker);

        new SolemnScholarBotAction().Invoke(game, scholar, p1);

        Assert.That(attacker.Bowed, Is.True);
    }
}
