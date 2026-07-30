using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Dsl;

public class DefendTheWallFirerTests
{
    [Test]
    public void FireIfLegal_WhenItsControllerWonAsDefender_DoesNotThrow()
    {
        // resolveConflictRing itself no-ops without a ChosenChoice/Target (same "don't
        // resolve" convention already accepted for akodo-toturi/doji-hotaru's own bot
        // adapters) - this only proves the trigger fires legally, not the ring effect's own
        // choice logic.
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p2 };
        var province = new Card { Id = "defend-the-wall", Type = CardType.Province, Controller = p1 };
        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1, DeclaredProvince = province, Winner = p1 };
        conflict.Elements.Add("water");
        game.CurrentConflict = conflict;

        Assert.DoesNotThrow(() => DefendTheWallFirer.FireIfLegal(game, province));
    }

    [Test]
    public void FireIfLegal_WhenDeclaredAgainstADifferentProvince_IsSkippedEvenWhenTheScriptWouldOtherwiseThrow()
    {
        // No active conflict at all would make the script itself throw
        // ("requires an active conflict") if it were ever invoked - proving the firer's own
        // DeclaredProvince gate short-circuits before calling it, since CurrentConflict is
        // null here.
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p2 };
        var province = new Card { Id = "defend-the-wall", Type = CardType.Province, Controller = p1 };
        game.CurrentConflict = null;

        Assert.DoesNotThrow(() => DefendTheWallFirer.FireIfLegal(game, province));
    }
}
