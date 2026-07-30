using L5R.Engine.GameSteps.BotActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.GameSteps.BotActions;

public class AkodoToturiBotActionTests
{
    private static GameState NewScenario(out Player p1, out Card toturi)
    {
        p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };

        toturi = new Card { Id = "akodo-toturi", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(toturi);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, ConflictType = "military", RingClaimedThisConflict = true };
        conflict.Attackers.Add(toturi);
        conflict.Elements.Add("fire");
        game.CurrentConflict = conflict;

        var ring = game.Rings.Find(r => r.Element == "fire")!;
        ring.Claimed = true;
        ring.ClaimedBy = p1;

        return game;
    }

    [Test]
    public void IsLegal_AfterClaimingTheRingThisConflict_True()
    {
        var game = NewScenario(out var p1, out var toturi);

        Assert.That(new AkodoToturiBotAction().IsLegal(game, toturi, p1), Is.True);
    }

    [Test]
    public void IsLegal_WhenTheRingWasNotClaimedThisConflict_False()
    {
        var game = NewScenario(out var p1, out var toturi);
        game.CurrentConflict!.RingClaimedThisConflict = false;

        Assert.That(new AkodoToturiBotAction().IsLegal(game, toturi, p1), Is.False);
    }

    [Test]
    public void Invoke_ResolvesWithoutThrowing()
    {
        // No ChosenChoice/Target is supplied - same "don't resolve" no-op convention already
        // used by ConflictResolver's own ring-claim resolution call in production. Just
        // proves the trigger itself fires legally; the ring effect's own choice logic is
        // out of scope here.
        var game = NewScenario(out var p1, out var toturi);

        Assert.DoesNotThrow(() => new AkodoToturiBotAction().Invoke(game, toturi, p1));
    }
}
