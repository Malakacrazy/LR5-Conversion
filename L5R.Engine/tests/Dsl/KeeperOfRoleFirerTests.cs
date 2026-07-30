using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Dsl;

public class KeeperOfRoleFirerTests
{
    private static (GameState game, Player p1) NewScenario(string roleId, string element)
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        p1.Role = new Card { Id = roleId, Type = CardType.Role, Controller = p1 };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p2 };
        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1, Winner = p1 };
        conflict.Elements.Add(element);
        game.CurrentConflict = conflict;
        return (game, p1);
    }

    [Test]
    public void FireIfLegal_WhenDefendingAndWinningTheMatchingElement_GainsOneFate()
    {
        var (game, p1) = NewScenario("keeper-of-water", "water");

        KeeperOfRoleFirer.FireIfLegal(game, p1);

        Assert.That(p1.Fate, Is.EqualTo(1));
    }

    [Test]
    public void FireIfLegal_WhenTheElementDoesNotMatch_DoesNotFire()
    {
        var (game, p1) = NewScenario("keeper-of-water", "fire");

        KeeperOfRoleFirer.FireIfLegal(game, p1);

        Assert.That(p1.Fate, Is.EqualTo(0));
    }

    [Test]
    public void FireIfLegal_WithNoMatchingRole_DoesNotFire()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p2 };
        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1, Winner = p1 };
        conflict.Elements.Add("water");
        game.CurrentConflict = conflict;

        Assert.DoesNotThrow(() => KeeperOfRoleFirer.FireIfLegal(game, p1));
        Assert.That(p1.Fate, Is.EqualTo(0));
    }
}
