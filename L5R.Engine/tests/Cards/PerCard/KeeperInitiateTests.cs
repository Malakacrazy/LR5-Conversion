using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class KeeperInitiateTests
{
    [Test]
    public void WhenClaimingAMatchingRingFromProvinces_EntersPlayAndGainsAFate()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var keeperInitiate = new Card { Id = "keeper-initiate", Type = CardType.Character, Controller = p1 };
        p1.Role = new Card { Id = "keeper-of-water", Type = CardType.Role, Controller = p1, Traits = new[] { "keeper", "water" } };
        p1.Provinces.Add(keeperInitiate);

        var ring = new Ring { Element = "water", ConflictType = "military", Claimed = true, ClaimedBy = p1 };
        var context = new AbilityContext { Game = game, Player = p1, Source = keeperInitiate, TargetRing = ring };

        new KeeperInitiatePutIntoPlayOnMatchingRingClaim().Execute(context);

        Assert.That(p1.PlayArea, Does.Contain(keeperInitiate));
        Assert.That(p1.Provinces, Does.Not.Contain(keeperInitiate));
        Assert.That(keeperInitiate.Fate, Is.EqualTo(1));
    }

    [Test]
    public void WhenClaimingAMatchingRingFromDiscard_EntersPlay()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var keeperInitiate = new Card { Id = "keeper-initiate", Type = CardType.Character, Controller = p1 };
        p1.Role = new Card { Id = "keeper-of-water", Type = CardType.Role, Controller = p1, Traits = new[] { "keeper", "water" } };
        p1.Discard.Add(keeperInitiate);

        var ring = new Ring { Element = "water", ConflictType = "military", Claimed = true, ClaimedBy = p1 };
        var context = new AbilityContext { Game = game, Player = p1, Source = keeperInitiate, TargetRing = ring };

        new KeeperInitiatePutIntoPlayOnMatchingRingClaim().Execute(context);

        Assert.That(p1.PlayArea, Does.Contain(keeperInitiate));
    }

    [Test]
    public void WhenTheClaimedRingDoesNotMatchTheRole_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var keeperInitiate = new Card { Id = "keeper-initiate", Type = CardType.Character, Controller = p1 };
        p1.Role = new Card { Id = "keeper-of-water", Type = CardType.Role, Controller = p1, Traits = new[] { "keeper", "water" } };
        p1.Provinces.Add(keeperInitiate);

        var ring = new Ring { Element = "fire", ConflictType = "military", Claimed = true, ClaimedBy = p1 };
        var context = new AbilityContext { Game = game, Player = p1, Source = keeperInitiate, TargetRing = ring };

        Assert.Throws<InvalidOperationException>(() => new KeeperInitiatePutIntoPlayOnMatchingRingClaim().Execute(context));
    }
}
