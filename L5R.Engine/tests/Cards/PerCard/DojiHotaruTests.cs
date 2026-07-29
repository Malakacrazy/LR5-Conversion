using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class DojiHotaruTests
{
    [Test]
    public void WhenClaimingARingDuringAPoliticalConflictWhileParticipating_ResolvesTheRing()
    {
        var p1 = new Player { Name = "Player1", Honor = 3 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var hotaru = new Card { Id = "doji-hotaru", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(hotaru);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, ConflictType = "political", Elements = new List<string> { "water" } };
        conflict.Attackers.Add(hotaru);
        game.CurrentConflict = conflict;

        var ring = new Ring { Element = "water", ConflictType = "political", Claimed = true, ClaimedBy = p1 };
        var context = new AbilityContext { Game = game, Player = p1, Source = hotaru, TargetRing = ring };

        new DojiHotaruResolveRingOnClaimDuringPolitical().Execute(context);
    }

    [Test]
    public void DuringAMilitaryConflict_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var hotaru = new Card { Id = "doji-hotaru", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(hotaru);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, ConflictType = "military" };
        conflict.Attackers.Add(hotaru);
        game.CurrentConflict = conflict;

        var ring = new Ring { Element = "water", ConflictType = "military", Claimed = true, ClaimedBy = p1 };
        var context = new AbilityContext { Game = game, Player = p1, Source = hotaru, TargetRing = ring };

        Assert.Throws<InvalidOperationException>(() => new DojiHotaruResolveRingOnClaimDuringPolitical().Execute(context));
    }

    [Test]
    public void WhenNotParticipating_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var hotaru = new Card { Id = "doji-hotaru", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(hotaru);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, ConflictType = "political" };
        game.CurrentConflict = conflict;

        var ring = new Ring { Element = "water", ConflictType = "political", Claimed = true, ClaimedBy = p1 };
        var context = new AbilityContext { Game = game, Player = p1, Source = hotaru, TargetRing = ring };

        Assert.Throws<InvalidOperationException>(() => new DojiHotaruResolveRingOnClaimDuringPolitical().Execute(context));
    }
}
