using L5R.Engine.GameSteps.BotActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.GameSteps.BotActions;

public class IdeTraderBotActionTests
{
    [Test]
    public void IsLegal_WhileParticipating_True()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var trader = new Card { Id = "ide-trader", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(trader);
        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(trader);
        game.CurrentConflict = conflict;

        Assert.That(new IdeTraderBotAction().IsLegal(game, trader, p1), Is.True);
    }

    [Test]
    public void IsLegal_WhileNotParticipating_False()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var trader = new Card { Id = "ide-trader", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(trader);

        Assert.That(new IdeTraderBotAction().IsLegal(game, trader, p1), Is.False);
    }

    [Test]
    public void Invoke_GainsOneFate()
    {
        var p1 = new Player { Name = "Player1", Fate = 2 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var trader = new Card { Id = "ide-trader", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(trader);
        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(trader);
        game.CurrentConflict = conflict;

        new IdeTraderBotAction().Invoke(game, trader, p1);

        Assert.That(p1.Fate, Is.EqualTo(3));
    }
}
