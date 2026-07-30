using L5R.Engine.GameSteps.BotActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.GameSteps.BotActions;

public class FallenInBattleBotActionTests
{
    private static (GameState game, Card fib, Card opponentParticipant) NewScenario(int skillDifference = 5)
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var fib = new Card { Id = "fallen-in-battle", Type = CardType.Event, Controller = p1 };
        var opponentParticipant = new Card { Id = "opponent-participant", Type = CardType.Character, Controller = p2 };
        p2.PlayArea.Add(opponentParticipant);
        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, Winner = p1, ConflictType = "military", SkillDifference = skillDifference };
        conflict.Defenders.Add(opponentParticipant);
        game.CurrentConflict = conflict;
        return (game, fib, opponentParticipant);
    }

    [Test]
    public void IsLegal_WonByFiveOrMoreSkill_True()
    {
        var (game, fib, _) = NewScenario();
        Assert.That(new FallenInBattleBotAction().IsLegal(game, fib, game.Player1), Is.True);
    }

    [Test]
    public void IsLegal_WonByLessThanFive_False()
    {
        var (game, fib, _) = NewScenario(skillDifference: 4);
        Assert.That(new FallenInBattleBotAction().IsLegal(game, fib, game.Player1), Is.False);
    }

    [Test]
    public void Invoke_DiscardsTheOpponentParticipant()
    {
        var (game, fib, opponentParticipant) = NewScenario();
        new FallenInBattleBotAction().Invoke(game, fib, game.Player1);
        Assert.That(game.Player2.Discard, Contains.Item(opponentParticipant));
    }
}
