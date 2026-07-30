using L5R.Engine.GameSteps.BotActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.GameSteps.BotActions;

public class ObstinateRecruitBotActionTests
{
    [Test]
    public void IsLegal_WhenOpponentIsMoreHonorable_True()
    {
        var p1 = new Player { Name = "Player1", Honor = 3 };
        var p2 = new Player { Name = "Player2", Honor = 5 };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var recruit = new Card { Id = "obstinate-recruit", Type = CardType.Character, Controller = p1 };

        Assert.That(new ObstinateRecruitBotAction().IsLegal(game, recruit, p1), Is.True);
    }

    [Test]
    public void IsLegal_WhenEquallyHonorable_False()
    {
        var p1 = new Player { Name = "Player1", Honor = 5 };
        var p2 = new Player { Name = "Player2", Honor = 5 };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var recruit = new Card { Id = "obstinate-recruit", Type = CardType.Character, Controller = p1 };

        Assert.That(new ObstinateRecruitBotAction().IsLegal(game, recruit, p1), Is.False);
    }

    [Test]
    public void Invoke_DiscardsItself()
    {
        var p1 = new Player { Name = "Player1", Honor = 3 };
        var p2 = new Player { Name = "Player2", Honor = 5 };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var recruit = new Card { Id = "obstinate-recruit", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(recruit);

        new ObstinateRecruitBotAction().Invoke(game, recruit, p1);

        Assert.That(p1.Discard, Contains.Item(recruit));
        Assert.That(p1.PlayArea, Does.Not.Contain(recruit));
    }
}
