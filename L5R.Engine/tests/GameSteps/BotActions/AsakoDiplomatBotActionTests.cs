using L5R.Engine.GameSteps.BotActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.GameSteps.BotActions;

public class AsakoDiplomatBotActionTests
{
    private static GameState NewScenario(out Player p1, out Card diplomat, out Card opponentParticipant)
    {
        p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };

        diplomat = new Card { Id = "asako-diplomat", Type = CardType.Character, Controller = p1 };
        opponentParticipant = new Card { Id = "opponent-participant", Type = CardType.Character, Controller = p2 };
        p1.PlayArea.Add(diplomat);
        p2.PlayArea.Add(opponentParticipant);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, Winner = p1 };
        conflict.Attackers.Add(diplomat);
        conflict.Defenders.Add(opponentParticipant);
        game.CurrentConflict = conflict;

        return game;
    }

    [Test]
    public void IsLegal_WhenItsControllerWonTheConflict_True()
    {
        var game = NewScenario(out var p1, out var diplomat, out _);

        Assert.That(new AsakoDiplomatBotAction().IsLegal(game, diplomat, p1), Is.True);
    }

    [Test]
    public void IsLegal_WhenItsControllerDidNotWin_False()
    {
        var game = NewScenario(out var p1, out var diplomat, out _);
        game.CurrentConflict!.Winner = game.Player2;

        Assert.That(new AsakoDiplomatBotAction().IsLegal(game, diplomat, p1), Is.False);
    }

    [Test]
    public void Invoke_DishonorsTheOpponentParticipant()
    {
        var game = NewScenario(out var p1, out var diplomat, out var opponentParticipant);

        new AsakoDiplomatBotAction().Invoke(game, diplomat, p1);

        Assert.That(opponentParticipant.IsDishonored, Is.True);
    }
}
