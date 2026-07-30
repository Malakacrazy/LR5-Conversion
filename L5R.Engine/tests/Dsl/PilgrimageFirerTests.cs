using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Dsl;

public class PilgrimageFirerTests
{
    [Test]
    public void FireIfLegal_WhenDeclaredAgainstIt_CancelsRingEffects()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p2 };
        var province = new Card { Id = "pilgrimage", Type = CardType.Province, Controller = p1 };
        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1, DeclaredProvince = province };
        game.CurrentConflict = conflict;

        PilgrimageFirer.FireIfLegal(game, province);

        Assert.That(conflict.RingEffectsCancelled, Is.True);
    }

    [Test]
    public void FireIfLegal_WhenBroken_DoesNotFire()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p2 };
        var province = new Card { Id = "pilgrimage", Type = CardType.Province, Controller = p1, Broken = true };
        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1, DeclaredProvince = province };
        game.CurrentConflict = conflict;

        PilgrimageFirer.FireIfLegal(game, province);

        Assert.That(conflict.RingEffectsCancelled, Is.False);
    }
}
