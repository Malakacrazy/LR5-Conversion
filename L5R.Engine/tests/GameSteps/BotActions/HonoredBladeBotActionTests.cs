using L5R.Engine.GameSteps.BotActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.GameSteps.BotActions;

public class HonoredBladeBotActionTests
{
    private static GameState NewScenario(out Player p1, out Card blade, out Card parent)
    {
        p1 = new Player { Name = "Player1", Honor = 5 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };

        parent = new Card { Id = "parent", Type = CardType.Character, Controller = p1 };
        blade = new Card { Id = "honored-blade", Type = CardType.Attachment, Controller = p1, AttachedTo = parent };
        p1.PlayArea.Add(parent);
        p1.PlayArea.Add(blade);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, Winner = p1 };
        conflict.Attackers.Add(parent);
        game.CurrentConflict = conflict;

        return game;
    }

    [Test]
    public void IsLegal_WhenTheAttachedCharactersControllerWon_True()
    {
        var game = NewScenario(out var p1, out var blade, out _);

        Assert.That(new HonoredBladeBotAction().IsLegal(game, blade, p1), Is.True);
    }

    [Test]
    public void IsLegal_WhenTheAttachedCharactersControllerDidNotWin_False()
    {
        var game = NewScenario(out var p1, out var blade, out _);
        game.CurrentConflict!.Winner = game.Player2;

        Assert.That(new HonoredBladeBotAction().IsLegal(game, blade, p1), Is.False);
    }

    [Test]
    public void Invoke_GrantsOneHonorToTheParentsController()
    {
        var game = NewScenario(out var p1, out var blade, out _);

        new HonoredBladeBotAction().Invoke(game, blade, p1);

        Assert.That(p1.Honor, Is.EqualTo(6));
    }
}
