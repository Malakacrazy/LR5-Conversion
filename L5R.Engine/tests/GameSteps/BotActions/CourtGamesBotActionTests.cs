using L5R.Engine.GameSteps.BotActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.GameSteps.BotActions;

public class CourtGamesBotActionTests
{
    private static (GameState game, Card cg, Card opponentParticipant) NewScenario()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var cg = new Card { Id = "court-games", Type = CardType.Event, Controller = p1 };
        var opponentParticipant = new Card { Id = "opponent-participant", Type = CardType.Character, Controller = p2 };
        p2.PlayArea.Add(opponentParticipant);
        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, ConflictType = "political" };
        conflict.Defenders.Add(opponentParticipant);
        game.CurrentConflict = conflict;
        return (game, cg, opponentParticipant);
    }

    [Test]
    public void IsLegal_DuringAPoliticalConflict_True()
    {
        var (game, cg, _) = NewScenario();
        Assert.That(new CourtGamesBotAction().IsLegal(game, cg, game.Player1), Is.True);
    }

    [Test]
    public void Invoke_PrefersDishonoringAnOpponentParticipant()
    {
        var (game, cg, opponentParticipant) = NewScenario();
        new CourtGamesBotAction().Invoke(game, cg, game.Player1);
        Assert.That(opponentParticipant.IsDishonored, Is.True);
    }

    [Test]
    public void Invoke_WithNoOpponentParticipant_HonorsAnOwnParticipant()
    {
        var (game, cg, opponentParticipant) = NewScenario();
        game.CurrentConflict!.Defenders.Remove(opponentParticipant);
        var myParticipant = new Card { Id = "my-participant", Type = CardType.Character, Controller = game.Player1 };
        game.Player1.PlayArea.Add(myParticipant);
        game.CurrentConflict!.Attackers.Add(myParticipant);

        new CourtGamesBotAction().Invoke(game, cg, game.Player1);

        Assert.That(myParticipant.IsHonored, Is.True);
    }
}
