using L5R.Engine.GameSteps.BotActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.GameSteps.BotActions;

public class HidaKisadaBotActionTests
{
    private static GameState NewScenario(out Player p1, out Card kisada)
    {
        p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };

        kisada = new Card { Id = "hida-kisada", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(kisada);

        game.CurrentConflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1 };

        return game;
    }

    [Test]
    public void IsLegal_DuringAConflictWithNoPriorOpponentWinThisRound_True()
    {
        var game = NewScenario(out var p1, out var kisada);

        Assert.That(new HidaKisadaBotAction().IsLegal(game, kisada, p1), Is.True);
    }

    [Test]
    public void IsLegal_WhenTheOpponentAlreadyWonAConflictThisRound_False()
    {
        var game = NewScenario(out var p1, out var kisada);
        game.ConflictRecord.Add(new Conflict { AttackingPlayer = game.Player2, DefendingPlayer = p1, Winner = game.Player2 });

        Assert.That(new HidaKisadaBotAction().IsLegal(game, kisada, p1), Is.False);
    }

    [Test]
    public void Invoke_SetsTheCancelledFlagAndCannotFireAgainThisConflict()
    {
        var game = NewScenario(out var p1, out var kisada);

        new HidaKisadaBotAction().Invoke(game, kisada, p1);

        Assert.That(game.FirstActionCancelledThisConflict, Is.True);
        Assert.That(new HidaKisadaBotAction().IsLegal(game, kisada, p1), Is.False);
    }
}
