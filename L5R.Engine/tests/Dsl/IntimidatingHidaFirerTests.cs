using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Dsl;

public class IntimidatingHidaFirerTests
{
    [Test]
    public void FireIfLegal_WhenTheOpponentPassesAsAttacker_TheOpponentLosesOneHonor()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2", Honor = 5 };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var hida = new Card { Id = "intimidating-hida", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(hida);

        IntimidatingHidaFirer.FireIfLegal(game, p2);

        Assert.That(p2.Honor, Is.EqualTo(4));
        Assert.That(game.CurrentConflict, Is.Null, "the throwaway conflict used to assert the pass is cleared afterward");
    }

    [Test]
    public void FireIfLegal_WithNoIntimidatingHidaInPlay_DoesNothing()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2", Honor = 5 };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };

        IntimidatingHidaFirer.FireIfLegal(game, p2);

        Assert.That(p2.Honor, Is.EqualTo(5));
        Assert.That(game.CurrentConflict, Is.Null);
    }
}
