using L5R.Engine.GameSteps.BotActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.GameSteps.BotActions;

public class MeddlingMediatorBotActionTests
{
    [Test]
    public void IsLegal_WhenOpponentDeclaredMoreThanOneConflict_True()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        game.ConflictDeclarationsThisPhase.Add((p2, false));
        game.ConflictDeclarationsThisPhase.Add((p2, false));
        var mediator = new Card { Id = "meddling-mediator", Type = CardType.Character, Controller = p1 };

        Assert.That(new MeddlingMediatorBotAction().IsLegal(game, mediator, p1), Is.True);
    }

    [Test]
    public void IsLegal_WhenOpponentDeclaredOnlyOneConflict_False()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        game.ConflictDeclarationsThisPhase.Add((p2, false));
        var mediator = new Card { Id = "meddling-mediator", Type = CardType.Character, Controller = p1 };

        Assert.That(new MeddlingMediatorBotAction().IsLegal(game, mediator, p1), Is.False);
    }

    [Test]
    public void Invoke_TakesOneFateFromTheOpponent()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2", Fate = 3 };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        game.ConflictDeclarationsThisPhase.Add((p2, false));
        game.ConflictDeclarationsThisPhase.Add((p2, false));
        var mediator = new Card { Id = "meddling-mediator", Type = CardType.Character, Controller = p1 };

        new MeddlingMediatorBotAction().Invoke(game, mediator, p1);

        Assert.That(p1.Fate, Is.EqualTo(1));
        Assert.That(p2.Fate, Is.EqualTo(2));
    }
}
