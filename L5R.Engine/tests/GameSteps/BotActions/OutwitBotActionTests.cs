using L5R.Engine.GameSteps.BotActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.GameSteps.BotActions;

public class OutwitBotActionTests
{
    [Test]
    public void IsLegal_WithAnOutclassingParticipatingCourtier_True()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var outwit = new Card { Id = "outwit", Type = CardType.Event, Controller = p1 };
        var myCourtier = new Card { Id = "my-courtier", Type = CardType.Character, Controller = p1, Traits = new[] { "courtier" }, PrintedPoliticalSkill = 5 };
        var opponentTarget = new Card { Id = "opponent-target", Type = CardType.Character, Controller = p2, PrintedPoliticalSkill = 2 };
        p1.PlayArea.Add(myCourtier);
        p2.PlayArea.Add(opponentTarget);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(myCourtier);
        game.CurrentConflict = conflict;

        Assert.That(new OutwitBotAction().IsLegal(game, outwit, p1), Is.True);
    }

    [Test]
    public void IsLegal_WithNoActiveConflict_False()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var outwit = new Card { Id = "outwit", Type = CardType.Event, Controller = p1 };
        var myCourtier = new Card { Id = "my-courtier", Type = CardType.Character, Controller = p1, Traits = new[] { "courtier" }, PrintedPoliticalSkill = 5 };
        var opponentTarget = new Card { Id = "opponent-target", Type = CardType.Character, Controller = p2, PrintedPoliticalSkill = 2 };
        p1.PlayArea.Add(myCourtier);
        p2.PlayArea.Add(opponentTarget);

        Assert.That(new OutwitBotAction().IsLegal(game, outwit, p1), Is.False, "myCourtier isn't participating without a conflict");
    }

    [Test]
    public void Invoke_SendsHomeTheOutclassedOpponent()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var outwit = new Card { Id = "outwit", Type = CardType.Event, Controller = p1 };
        var myCourtier = new Card { Id = "my-courtier", Type = CardType.Character, Controller = p1, Traits = new[] { "courtier" }, PrintedPoliticalSkill = 5 };
        var opponentTarget = new Card { Id = "opponent-target", Type = CardType.Character, Controller = p2, PrintedPoliticalSkill = 2 };
        p1.PlayArea.Add(myCourtier);
        p2.PlayArea.Add(opponentTarget);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(myCourtier);
        conflict.Defenders.Add(opponentTarget);
        game.CurrentConflict = conflict;

        new OutwitBotAction().Invoke(game, outwit, p1);

        Assert.That(conflict.Defenders, Does.Not.Contain(opponentTarget));
    }
}
