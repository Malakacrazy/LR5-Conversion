using L5R.Engine.GameSteps.BotActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.GameSteps.BotActions;

public class RadiantOratorBotActionTests
{
    private static GameState NewScenario(out Player p1, out Player p2, out Card orator, out Card opponentParticipant, int opponentGlory = 1)
    {
        p1 = new Player { Name = "Player1" };
        p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };

        orator = new Card { Id = "radiant-orator", Type = CardType.Character, Controller = p1, PrintedGlory = 5 };
        opponentParticipant = new Card { Id = "opponent-participant", Type = CardType.Character, Controller = p2, PrintedGlory = opponentGlory };
        p1.PlayArea.Add(orator);
        p2.PlayArea.Add(opponentParticipant);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(orator);
        conflict.Defenders.Add(opponentParticipant);
        game.CurrentConflict = conflict;

        return game;
    }

    [Test]
    public void IsLegal_WhenAheadOnGlory_True()
    {
        var game = NewScenario(out var p1, out _, out var orator, out _);

        Assert.That(new RadiantOratorBotAction().IsLegal(game, orator, p1), Is.True);
    }

    [Test]
    public void IsLegal_WhenNotAheadOnGlory_False()
    {
        var game = NewScenario(out var p1, out _, out var orator, out _, opponentGlory: 10);

        Assert.That(new RadiantOratorBotAction().IsLegal(game, orator, p1), Is.False);
    }

    [Test]
    public void Invoke_SendsHomeTheOpponentParticipant()
    {
        var game = NewScenario(out var p1, out _, out var orator, out var opponentParticipant);

        new RadiantOratorBotAction().Invoke(game, orator, p1);

        Assert.That(game.CurrentConflict!.Defenders, Does.Not.Contain(opponentParticipant));
    }
}
