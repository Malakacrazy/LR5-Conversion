using L5R.Engine.GameSteps.BotActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.GameSteps.BotActions;

public class DisplayOfPowerBotActionTests
{
    private static GameState NewScenario(out Card dop, out Player p1)
    {
        p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p2 };
        dop = new Card { Id = "display-of-power", Type = CardType.Event, Controller = p1 };
        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1, Loser = p1, Unopposed = true, Winner = p2 };
        conflict.Elements.Add("air");
        game.CurrentConflict = conflict;
        return game;
    }

    [Test]
    public void IsLegal_AfterLosingAnUnopposedConflict_True()
    {
        var game = NewScenario(out var dop, out var p1);
        Assert.That(new DisplayOfPowerBotAction().IsLegal(game, dop, p1), Is.True);
    }

    [Test]
    public void Invoke_ClaimsTheRingForItsController()
    {
        var game = NewScenario(out var dop, out var p1);
        new DisplayOfPowerBotAction().Invoke(game, dop, p1);

        var ring = game.Rings.Find(r => r.Element == "air")!;
        Assert.That(ring.Claimed, Is.True);
        Assert.That(ring.ClaimedBy, Is.EqualTo(p1));
    }
}
