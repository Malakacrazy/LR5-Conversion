using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Dsl;

public class ShamefulDisplayFirerTests
{
    [Test]
    public void FireIfLegal_WhenDeclaredAgainstIt_HonorsOwnerAndDishonorsOpponent()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p2 };
        var province = new Card { Id = "shameful-display", Type = CardType.Province, Controller = p1 };
        var defender = new Card { Id = "defender", Type = CardType.Character, Controller = p1 };
        var attacker = new Card { Id = "attacker", Type = CardType.Character, Controller = p2 };
        p1.PlayArea.Add(defender);
        p2.PlayArea.Add(attacker);
        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1, DeclaredProvince = province };
        conflict.Attackers.Add(attacker);
        conflict.Defenders.Add(defender);
        game.CurrentConflict = conflict;

        ShamefulDisplayFirer.FireIfLegal(game, province);

        Assert.That(defender.IsHonored, Is.True);
        Assert.That(attacker.IsDishonored, Is.True);
    }

    [Test]
    public void FireIfLegal_WhenDeclaredAgainstADifferentProvince_DoesNotFire()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p2 };
        var province = new Card { Id = "shameful-display", Type = CardType.Province, Controller = p1 };
        var otherProvince = new Card { Id = "other-province", Type = CardType.Province, Controller = p1 };
        var defender = new Card { Id = "defender", Type = CardType.Character, Controller = p1 };
        var attacker = new Card { Id = "attacker", Type = CardType.Character, Controller = p2 };
        p1.PlayArea.Add(defender);
        p2.PlayArea.Add(attacker);
        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1, DeclaredProvince = otherProvince };
        conflict.Attackers.Add(attacker);
        conflict.Defenders.Add(defender);
        game.CurrentConflict = conflict;

        ShamefulDisplayFirer.FireIfLegal(game, province);

        Assert.That(defender.IsHonored, Is.False);
        Assert.That(attacker.IsDishonored, Is.False);
    }
}
