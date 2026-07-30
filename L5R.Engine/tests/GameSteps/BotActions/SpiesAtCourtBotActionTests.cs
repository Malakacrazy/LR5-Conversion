using L5R.Engine.GameSteps.BotActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.GameSteps.BotActions;

public class SpiesAtCourtBotActionTests
{
    private static (GameState game, Card sac, Card costTarget) NewScenario()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var sac = new Card { Id = "spies-at-court", Type = CardType.Event, Controller = p1 };
        var costTarget = new Card { Id = "my-participant", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(costTarget);
        p2.Hand.Add(new Card { Id = "hand-1", Type = CardType.Character, Controller = p2, Location = "hand" });
        p2.Hand.Add(new Card { Id = "hand-2", Type = CardType.Character, Controller = p2, Location = "hand" });
        p2.Hand.Add(new Card { Id = "hand-3", Type = CardType.Character, Controller = p2, Location = "hand" });
        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, Winner = p1, ConflictType = "political" };
        conflict.Attackers.Add(costTarget);
        game.CurrentConflict = conflict;
        return (game, sac, costTarget);
    }

    [Test]
    public void IsLegal_AfterWinningAPoliticalConflict_True()
    {
        var (game, sac, _) = NewScenario();
        Assert.That(new SpiesAtCourtBotAction().IsLegal(game, sac, game.Player1), Is.True);
    }

    [Test]
    public void Invoke_DishonorsTheCostTargetAndDiscardsTwoFromOpponentsHand()
    {
        var (game, sac, costTarget) = NewScenario();
        new SpiesAtCourtBotAction().Invoke(game, sac, game.Player1);

        Assert.That(costTarget.IsDishonored, Is.True);
        Assert.That(game.Player2.Hand, Has.Count.EqualTo(1));
        Assert.That(game.Player2.Discard, Has.Count.EqualTo(2));
    }
}
