using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class AkodoToturiTests
{
    [Test]
    public void WhenClaimingARingDuringAMilitaryConflictWhileParticipating_ResolvesTheRing()
    {
        var p1 = new Player { Name = "Player1", Honor = 3 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var toturi = new Card { Id = "akodo-toturi", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(toturi);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, ConflictType = "military", Elements = new List<string> { "air" } };
        conflict.Attackers.Add(toturi);
        game.CurrentConflict = conflict;

        var ring = new Ring { Element = "air", ConflictType = "military", Claimed = true, ClaimedBy = p1 };
        var context = new AbilityContext { Game = game, Player = p1, Source = toturi, TargetRing = ring, ChosenChoice = "Gain 2 Honor" };

        new AkodoToturiResolveRingOnClaimDuringMilitary().Execute(context);

        Assert.That(p1.Honor, Is.EqualTo(5));
    }

    [Test]
    public void WhenTheOpponentClaimsTheRing_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var toturi = new Card { Id = "akodo-toturi", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(toturi);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, ConflictType = "military" };
        conflict.Attackers.Add(toturi);
        game.CurrentConflict = conflict;

        var ring = new Ring { Element = "air", ConflictType = "military", Claimed = true, ClaimedBy = p2 };
        var context = new AbilityContext { Game = game, Player = p1, Source = toturi, TargetRing = ring };

        Assert.Throws<InvalidOperationException>(() => new AkodoToturiResolveRingOnClaimDuringMilitary().Execute(context));
    }

    [Test]
    public void DuringAPoliticalConflict_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var toturi = new Card { Id = "akodo-toturi", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(toturi);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, ConflictType = "political" };
        conflict.Attackers.Add(toturi);
        game.CurrentConflict = conflict;

        var ring = new Ring { Element = "air", ConflictType = "political", Claimed = true, ClaimedBy = p1 };
        var context = new AbilityContext { Game = game, Player = p1, Source = toturi, TargetRing = ring };

        Assert.Throws<InvalidOperationException>(() => new AkodoToturiResolveRingOnClaimDuringMilitary().Execute(context));
    }
}
