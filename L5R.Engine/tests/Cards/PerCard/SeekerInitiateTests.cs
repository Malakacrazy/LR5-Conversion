using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class SeekerInitiateTests
{
    [Test]
    public void WhenClaimingARingMatchingTheRole_TakesAChosenCardToHand()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var seekerInitiate = new Card { Id = "seeker-initiate", Type = CardType.Character, Controller = p1 };
        p1.Role = new Card { Id = "seeker-of-fire", Type = CardType.Role, Controller = p1, Traits = new[] { "seeker", "fire" } };
        var topCard = new Card { Id = "top-card", Type = CardType.Character, Controller = p1 };
        p1.Deck.Add(topCard);

        var ring = new Ring { Element = "fire", ConflictType = "military", Claimed = true, ClaimedBy = p1 };
        var context = new AbilityContext { Game = game, Player = p1, Source = seekerInitiate, TargetRing = ring, ChosenDeckSearchCard = topCard };

        new SeekerInitiateSearchTopFiveOnMatchingRingClaim().Execute(context);

        Assert.That(p1.Hand, Does.Contain(topCard));
    }

    [Test]
    public void WhenTheClaimedRingDoesNotMatchTheRole_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var seekerInitiate = new Card { Id = "seeker-initiate", Type = CardType.Character, Controller = p1 };
        p1.Role = new Card { Id = "seeker-of-fire", Type = CardType.Role, Controller = p1, Traits = new[] { "seeker", "fire" } };

        var ring = new Ring { Element = "water", ConflictType = "military", Claimed = true, ClaimedBy = p1 };
        var context = new AbilityContext { Game = game, Player = p1, Source = seekerInitiate, TargetRing = ring };

        Assert.Throws<InvalidOperationException>(() => new SeekerInitiateSearchTopFiveOnMatchingRingClaim().Execute(context));
    }

    [Test]
    public void WhenTheOpponentClaimsTheMatchingRing_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var seekerInitiate = new Card { Id = "seeker-initiate", Type = CardType.Character, Controller = p1 };
        p1.Role = new Card { Id = "seeker-of-fire", Type = CardType.Role, Controller = p1, Traits = new[] { "seeker", "fire" } };

        var ring = new Ring { Element = "fire", ConflictType = "military", Claimed = true, ClaimedBy = p2 };
        var context = new AbilityContext { Game = game, Player = p1, Source = seekerInitiate, TargetRing = ring };

        Assert.Throws<InvalidOperationException>(() => new SeekerInitiateSearchTopFiveOnMatchingRingClaim().Execute(context));
    }
}
