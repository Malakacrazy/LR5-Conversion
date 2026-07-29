using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.State;

/// <summary>
/// know-the-world (the only ported card using returnRing/takeRing) relies on card-schema.json's
/// "targets" (plural) multi-target shape and a ring-scoped "isController" predicate, neither of
/// which is supported yet (see the ring-claiming-state scope notes) - so these two gameActions
/// are proven correct here via direct handler invocation, the same pattern already used for
/// sashimono/pacifism/aggressive-moto's restriction checks.
/// </summary>
public class RingGameActionTests
{
    private static GameState NewGame()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        return new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
    }

    [Test]
    public void ReturnRing_ResetsAClaimedRingToUnclaimed()
    {
        var game = NewGame();
        var ring = game.Rings[0];
        ring.Claimed = true;
        ring.ClaimedBy = game.Player2;
        ring.Fate = 3;

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = new Card { Id = "know-the-world", Type = CardType.Event, Controller = game.Player1 }, TargetRing = ring };
        new ReturnRingGameActionHandler().Execute(context, parameters: null);

        Assert.That(ring.Claimed, Is.False);
        Assert.That(ring.ClaimedBy, Is.Null);
        Assert.That(ring.Fate, Is.EqualTo(3), "resetRing doesn't touch fate");
    }

    [Test]
    public void ReturnRing_CannotAffectAnAlreadyUnclaimedRing()
    {
        var game = NewGame();
        var ring = game.Rings[0];
        var context = new AbilityContext { Game = game, Player = game.Player1, Source = new Card { Id = "know-the-world", Type = CardType.Event, Controller = game.Player1 }, TargetRing = ring };

        Assert.That(new ReturnRingGameActionHandler().CanAffect(context, parameters: null), Is.False);
    }

    [Test]
    public void TakeRing_ClaimsAnUnclaimedRingAndTakesItsFate()
    {
        var game = NewGame();
        var ring = game.Rings[0];
        ring.Fate = 2;

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = new Card { Id = "know-the-world", Type = CardType.Event, Controller = game.Player1 }, TargetRing = ring };
        new TakeRingGameActionHandler().Execute(context, parameters: null);

        Assert.That(ring.Claimed, Is.True);
        Assert.That(ring.ClaimedBy, Is.EqualTo(game.Player1));
        Assert.That(ring.Contested, Is.False);
        Assert.That(ring.Fate, Is.EqualTo(0));
        Assert.That(game.Player1.Fate, Is.EqualTo(2));
    }

    [Test]
    public void TakeRing_CannotAffectARingAlreadyClaimedBySelf()
    {
        var game = NewGame();
        var ring = game.Rings[0];
        ring.Claimed = true;
        ring.ClaimedBy = game.Player1;

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = new Card { Id = "know-the-world", Type = CardType.Event, Controller = game.Player1 }, TargetRing = ring };

        Assert.That(new TakeRingGameActionHandler().CanAffect(context, parameters: null), Is.False);
    }
}
