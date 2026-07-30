using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Dsl;

public class EndlessPlainsFirerTests
{
    private static (GameState game, Card province, Card attacker) NewScenario()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p2 };
        var province = new Card { Id = "endless-plains", Type = CardType.Province, Controller = p1 };
        var attacker = new Card { Id = "attacker", Type = CardType.Character, Controller = p2 };
        p2.PlayArea.Add(attacker);
        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1, DeclaredProvince = province };
        conflict.Attackers.Add(attacker);
        game.CurrentConflict = conflict;
        return (game, province, attacker);
    }

    [Test]
    public void FireIfLegal_WhenDeclaredAgainstIt_BreaksItselfAndDiscardsAnAttacker()
    {
        var (game, province, attacker) = NewScenario();

        EndlessPlainsFirer.FireIfLegal(game, province);

        Assert.That(province.Broken, Is.True);
        Assert.That(game.Player2.Discard, Contains.Item(attacker));
    }

    [Test]
    public void FireIfLegal_WhenAlreadyBroken_DoesNotFireAgain()
    {
        var (game, province, attacker) = NewScenario();
        province.Broken = true;

        EndlessPlainsFirer.FireIfLegal(game, province);

        Assert.That(game.Player2.Discard, Does.Not.Contain(attacker));
    }
}
