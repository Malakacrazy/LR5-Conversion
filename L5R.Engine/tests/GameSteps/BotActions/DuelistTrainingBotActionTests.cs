using L5R.Engine.GameSteps.BotActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.GameSteps.BotActions;

public class DuelistTrainingBotActionTests
{
    private static GameState NewScenario(out Player p1, out Card duelistTraining, out Card host, out Card opponentParticipant, int hostBid = 0, int opponentBid = 0)
    {
        p1 = new Player { Name = "Player1", ShowBid = hostBid };
        var p2 = new Player { Name = "Player2", ShowBid = opponentBid };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };

        host = new Card { Id = "host", Type = CardType.Character, Controller = p1, PrintedMilitarySkill = 5 };
        duelistTraining = new Card { Id = "duelist-training", Type = CardType.Attachment, Controller = p1, AttachedTo = host };
        opponentParticipant = new Card { Id = "opponent-participant", Type = CardType.Character, Controller = p2, PrintedMilitarySkill = 1 };
        p1.PlayArea.Add(host);
        p1.PlayArea.Add(duelistTraining);
        p2.PlayArea.Add(opponentParticipant);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(host);
        conflict.Defenders.Add(opponentParticipant);
        game.CurrentConflict = conflict;

        return game;
    }

    [Test]
    public void IsLegal_WhileHostIsParticipatingWithAnOpponentParticipant_True()
    {
        var game = NewScenario(out var p1, out var duelistTraining, out _, out _);

        Assert.That(new DuelistTrainingBotAction().IsLegal(game, duelistTraining, p1), Is.True);
    }

    [Test]
    public void IsLegal_WhenHostIsNotParticipating_False()
    {
        var game = NewScenario(out var p1, out var duelistTraining, out var host, out _);
        game.CurrentConflict!.Attackers.Remove(host);

        Assert.That(new DuelistTrainingBotAction().IsLegal(game, duelistTraining, p1), Is.False);
    }

    [Test]
    public void Invoke_WithNoBidDifference_BowsTheLoserWithoutPayingAnyCost()
    {
        var game = NewScenario(out var p1, out var duelistTraining, out var host, out var opponentParticipant, hostBid: 2, opponentBid: 2);

        new DuelistTrainingBotAction().Invoke(game, duelistTraining, p1);

        Assert.That(opponentParticipant.Bowed, Is.True, "the host's higher skill wins the duel");
        Assert.That(p1.Honor, Is.EqualTo(0));
        Assert.That(game.Player2.Honor, Is.EqualTo(0));
    }

    [Test]
    public void Invoke_WithABidDifference_PaysWithHonorAndBowsTheLoser()
    {
        var p1Honor = 5;
        var game = NewScenario(out var p1, out var duelistTraining, out var host, out var opponentParticipant, hostBid: 1, opponentBid: 4);
        p1.Honor = p1Honor;
        game.Player2.Honor = 5;

        new DuelistTrainingBotAction().Invoke(game, duelistTraining, p1);

        Assert.That(p1.Honor, Is.EqualTo(2), "the lower bidder (host's controller) pays the 3-point difference");
        Assert.That(game.Player2.Honor, Is.EqualTo(8));
        Assert.That(opponentParticipant.Bowed, Is.True, "the host still wins on skill despite the lower bid");
    }
}
