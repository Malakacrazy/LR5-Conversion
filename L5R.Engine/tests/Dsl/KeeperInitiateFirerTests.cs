using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Dsl;

public class KeeperInitiateFirerTests
{
    private static (GameState game, Card keeperInitiate) NewScenario(bool inProvinces = true)
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        p1.Role = new Card { Id = "keeper-of-earth", Type = CardType.Role, Controller = p1, Traits = new[] { "earth" } };

        var keeperInitiate = new Card { Id = "keeper-initiate", Type = CardType.Character, Controller = p1 };
        if (inProvinces)
            p1.Provinces.Add(keeperInitiate);
        else
            p1.Discard.Add(keeperInitiate);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, RingClaimedThisConflict = true };
        conflict.Elements.Add("earth");
        game.CurrentConflict = conflict;

        var ring = game.Rings.Find(r => r.Element == "earth")!;
        ring.Claimed = true;
        ring.ClaimedBy = p1;

        return (game, keeperInitiate);
    }

    [Test]
    public void FireIfLegal_WhenSittingInAProvinceAfterClaimingTheMatchingRing_PutsItIntoPlay()
    {
        var (game, keeperInitiate) = NewScenario(inProvinces: true);

        KeeperInitiateFirer.FireIfLegal(game, game.Player1);

        Assert.That(game.Player1.Provinces, Does.Not.Contain(keeperInitiate));
        Assert.That(game.Player1.PlayArea, Contains.Item(keeperInitiate));
        Assert.That(keeperInitiate.Fate, Is.EqualTo(1));
    }

    [Test]
    public void FireIfLegal_WhenSittingInDiscardAfterClaimingTheMatchingRing_PutsItIntoPlay()
    {
        var (game, keeperInitiate) = NewScenario(inProvinces: false);

        KeeperInitiateFirer.FireIfLegal(game, game.Player1);

        Assert.That(game.Player1.Discard, Does.Not.Contain(keeperInitiate));
        Assert.That(game.Player1.PlayArea, Contains.Item(keeperInitiate));
    }

    [Test]
    public void FireIfLegal_WhenTheRingWasNotClaimedThisConflict_DoesNotFire()
    {
        var (game, keeperInitiate) = NewScenario();
        game.CurrentConflict!.RingClaimedThisConflict = false;

        KeeperInitiateFirer.FireIfLegal(game, game.Player1);

        Assert.That(game.Player1.Provinces, Contains.Item(keeperInitiate));
    }
}
