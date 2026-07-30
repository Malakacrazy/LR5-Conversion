using L5R.Engine.GameSteps.BotActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.GameSteps.BotActions;

public class StrengthInNumbersBotActionTests
{
    private static GameState NewConflict(out Player p1, out Player p2, out Card attacker, out Card defender)
    {
        p1 = new Player { Name = "Player1" };
        p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };

        attacker = new Card { Id = "attacker", Type = CardType.Character, Controller = p1 };
        defender = new Card { Id = "defender", Type = CardType.Character, Controller = p2, PrintedGlory = 1 };
        p1.PlayArea.Add(attacker);
        p2.PlayArea.Add(defender);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(attacker);
        conflict.Defenders.Add(defender);
        game.CurrentConflict = conflict;

        return game;
    }

    [Test]
    public void IsLegal_AsAttackerWithALowGloryDefender_True()
    {
        var game = NewConflict(out var p1, out _, out _, out _);
        var source = new Card { Id = "strength-in-numbers", Type = CardType.Event, Controller = p1 };

        Assert.That(new StrengthInNumbersBotAction().IsLegal(game, source, p1), Is.True);
    }

    [Test]
    public void IsLegal_AsDefender_False()
    {
        var game = NewConflict(out _, out var p2, out _, out _);
        var source = new Card { Id = "strength-in-numbers", Type = CardType.Event, Controller = p2 };

        Assert.That(new StrengthInNumbersBotAction().IsLegal(game, source, p2), Is.False, "only the attacking player may use it");
    }

    [Test]
    public void Invoke_SendsHomeTheLowGloryDefender()
    {
        var game = NewConflict(out var p1, out _, out _, out var defender);
        var source = new Card { Id = "strength-in-numbers", Type = CardType.Event, Controller = p1 };

        new StrengthInNumbersBotAction().Invoke(game, source, p1);

        Assert.That(game.CurrentConflict!.Defenders, Does.Not.Contain(defender));
    }
}
