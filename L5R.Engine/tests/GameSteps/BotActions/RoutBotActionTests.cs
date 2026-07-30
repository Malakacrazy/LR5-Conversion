using L5R.Engine.GameSteps.BotActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.GameSteps.BotActions;

public class RoutBotActionTests
{
    [Test]
    public void IsLegal_WithAnOutclassingParticipatingBushi_True()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var rout = new Card { Id = "rout", Type = CardType.Event, Controller = p1 };
        var myBushi = new Card { Id = "my-bushi", Type = CardType.Character, Controller = p1, Traits = new[] { "bushi" }, PrintedMilitarySkill = 5 };
        var opponentTarget = new Card { Id = "opponent-target", Type = CardType.Character, Controller = p2, PrintedMilitarySkill = 2 };
        p1.PlayArea.Add(myBushi);
        p2.PlayArea.Add(opponentTarget);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(myBushi);
        game.CurrentConflict = conflict;

        Assert.That(new RoutBotAction().IsLegal(game, rout, p1), Is.True);
    }

    [Test]
    public void Invoke_SendsHomeTheOutclassedOpponent()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var rout = new Card { Id = "rout", Type = CardType.Event, Controller = p1 };
        var myBushi = new Card { Id = "my-bushi", Type = CardType.Character, Controller = p1, Traits = new[] { "bushi" }, PrintedMilitarySkill = 5 };
        var opponentTarget = new Card { Id = "opponent-target", Type = CardType.Character, Controller = p2, PrintedMilitarySkill = 2 };
        p1.PlayArea.Add(myBushi);
        p2.PlayArea.Add(opponentTarget);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(myBushi);
        conflict.Defenders.Add(opponentTarget);
        game.CurrentConflict = conflict;

        new RoutBotAction().Invoke(game, rout, p1);

        Assert.That(conflict.Defenders, Does.Not.Contain(opponentTarget));
    }
}
