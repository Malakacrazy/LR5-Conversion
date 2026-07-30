using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Dsl;

public class SeekerOfRoleFirerTests
{
    [Test]
    public void FireIfLegal_WhenTheRevealedProvinceMatchesTheRolesElement_GainsOneFate()
    {
        var p1 = new Player { Name = "Player1" };
        p1.Role = new Card { Id = "seeker-of-fire", Type = CardType.Role, Controller = p1 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p2 };
        var province = new Card { Id = "fire-province", Type = CardType.Province, Controller = p1, Traits = new[] { "fire" } };

        SeekerOfRoleFirer.FireIfLegal(game, province);

        Assert.That(p1.Fate, Is.EqualTo(1));
    }

    [Test]
    public void FireIfLegal_WhenTheProvinceDoesNotMatch_DoesNotFire()
    {
        var p1 = new Player { Name = "Player1" };
        p1.Role = new Card { Id = "seeker-of-fire", Type = CardType.Role, Controller = p1 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p2 };
        var province = new Card { Id = "water-province", Type = CardType.Province, Controller = p1, Traits = new[] { "water" } };

        SeekerOfRoleFirer.FireIfLegal(game, province);

        Assert.That(p1.Fate, Is.EqualTo(0));
    }

    [Test]
    public void FireIfLegal_WithNoMatchingRole_DoesNotFire()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p2 };
        var province = new Card { Id = "fire-province", Type = CardType.Province, Controller = p1, Traits = new[] { "fire" } };

        Assert.DoesNotThrow(() => SeekerOfRoleFirer.FireIfLegal(game, province));
        Assert.That(p1.Fate, Is.EqualTo(0));
    }
}
