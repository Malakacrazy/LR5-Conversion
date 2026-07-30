using L5R.Engine.GameSteps.BotActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.GameSteps.BotActions;

public class ShinjoTatsuoBotActionTests
{
    [Test]
    public void IsLegal_WithAnActiveConflictNotYetParticipating_True()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var tatsuo = new Card { Id = "shinjo-tatsuo", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(tatsuo);
        game.CurrentConflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };

        Assert.That(new ShinjoTatsuoBotAction().IsLegal(game, tatsuo, p1), Is.True);
    }

    [Test]
    public void IsLegal_WhenAlreadyParticipating_False()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var tatsuo = new Card { Id = "shinjo-tatsuo", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(tatsuo);
        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(tatsuo);
        game.CurrentConflict = conflict;

        Assert.That(new ShinjoTatsuoBotAction().IsLegal(game, tatsuo, p1), Is.False);
    }

    [Test]
    public void Invoke_MovesItselfIntoTheConflictAsAnAttacker()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var tatsuo = new Card { Id = "shinjo-tatsuo", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(tatsuo);
        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        game.CurrentConflict = conflict;

        new ShinjoTatsuoBotAction().Invoke(game, tatsuo, p1);

        Assert.That(conflict.Attackers, Contains.Item(tatsuo));
    }
}
