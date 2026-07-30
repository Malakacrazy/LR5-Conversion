using L5R.Engine.GameSteps.BotActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.GameSteps.BotActions;

public class YogoHiroueBotActionTests
{
    private static GameState NewScenario(out Player p1, out Card hiroue, out Card ally)
    {
        p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };

        hiroue = new Card { Id = "yogo-hiroue", Type = CardType.Character, Controller = p1 };
        ally = new Card { Id = "ally", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(hiroue);
        p1.PlayArea.Add(ally);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(hiroue);
        game.CurrentConflict = conflict;

        return game;
    }

    [Test]
    public void IsLegal_WhileParticipatingWithAnAllyNotYetInTheConflict_True()
    {
        var game = NewScenario(out var p1, out var hiroue, out _);

        Assert.That(new YogoHiroueBotAction().IsLegal(game, hiroue, p1), Is.True);
    }

    [Test]
    public void IsLegal_WhenNotParticipating_False()
    {
        var game = NewScenario(out var p1, out var hiroue, out _);
        game.CurrentConflict!.Attackers.Remove(hiroue);

        Assert.That(new YogoHiroueBotAction().IsLegal(game, hiroue, p1), Is.False);
    }

    [Test]
    public void Invoke_MovesTheAllyIntoTheConflictAsAnAttacker()
    {
        var game = NewScenario(out var p1, out var hiroue, out var ally);

        new YogoHiroueBotAction().Invoke(game, hiroue, p1);

        Assert.That(game.CurrentConflict!.Attackers, Contains.Item(ally));
    }
}
