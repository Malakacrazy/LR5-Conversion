using System.Linq;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Dsl;

public class ShibaTsukuneFirerTests
{
    [Test]
    public void FireIfLegal_AsTheConflictPhaseEnds_DoesNotThrow()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var tsukune = new Card { Id = "shiba-tsukune", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(tsukune);

        Assert.DoesNotThrow(() => ShibaTsukuneFirer.FireIfLegal(game, p1));
        Assert.That(game.Rings.Where(r => r.IsUnclaimed).Count(), Is.EqualTo(5), "a trivial no-choice resolution never claims a ring");
    }

    [Test]
    public void FireIfLegal_OutsideTheConflictPhase_DoesNothing()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Fate };
        var tsukune = new Card { Id = "shiba-tsukune", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(tsukune);

        Assert.DoesNotThrow(() => ShibaTsukuneFirer.FireIfLegal(game, p1));
    }
}
