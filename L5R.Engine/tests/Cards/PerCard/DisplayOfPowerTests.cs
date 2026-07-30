using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class DisplayOfPowerTests
{
    [Test]
    public void AfterLosingAnUnopposedConflict_ClaimsTheRingForItsController()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var displayOfPower = new Card { Id = "display-of-power", Type = CardType.Event, Controller = p1 };

        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1, Loser = p1, Unopposed = true };
        game.CurrentConflict = conflict;

        var ring = new Ring { Element = "fire", ConflictType = "military" };
        var context = new AbilityContext { Game = game, Player = p1, Source = displayOfPower, TargetRing = ring };

        new DisplayOfPowerCancelAndClaimRing().Execute(context);

        Assert.That(ring.Claimed, Is.True);
        Assert.That(ring.ClaimedBy, Is.EqualTo(p1), "claimed by the loser, not the original winner");
    }

    [Test]
    public void ItsControllerCanThenResolveTheRingInsteadOfTheOriginalWinner()
    {
        var p1 = new Player { Name = "Player1", Honor = 3 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var displayOfPower = new Card { Id = "display-of-power", Type = CardType.Event, Controller = p1 };

        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1, Loser = p1, Unopposed = true, Elements = new List<string> { "air" } };
        game.CurrentConflict = conflict;

        var ring = new Ring { Element = "air", ConflictType = "military" };
        var claimContext = new AbilityContext { Game = game, Player = p1, Source = displayOfPower, TargetRing = ring };
        new DisplayOfPowerCancelAndClaimRing().Execute(claimContext);

        var resolveContext = new AbilityContext { Game = game, Player = p1, Source = displayOfPower, ChosenChoice = "Gain 2 Honor" };
        new ResolveConflictRingGameActionHandler().Execute(resolveContext, null);

        Assert.That(p1.Honor, Is.EqualTo(5), "the loser resolves the ring, not the conflict's actual winner");
    }

    [Test]
    public void WhenTheConflictWasOpposed_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var displayOfPower = new Card { Id = "display-of-power", Type = CardType.Event, Controller = p1 };

        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1, Loser = p1, Unopposed = false };
        game.CurrentConflict = conflict;

        var ring = new Ring { Element = "fire", ConflictType = "military" };
        var context = new AbilityContext { Game = game, Player = p1, Source = displayOfPower, TargetRing = ring };

        Assert.Throws<InvalidOperationException>(() => new DisplayOfPowerCancelAndClaimRing().Execute(context));
    }

    [Test]
    public void WhenItsControllerWon_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var displayOfPower = new Card { Id = "display-of-power", Type = CardType.Event, Controller = p1 };

        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1, Loser = p2, Unopposed = true };
        game.CurrentConflict = conflict;

        var ring = new Ring { Element = "fire", ConflictType = "military" };
        var context = new AbilityContext { Game = game, Player = p1, Source = displayOfPower, TargetRing = ring };

        Assert.Throws<InvalidOperationException>(() => new DisplayOfPowerCancelAndClaimRing().Execute(context));
    }
}
