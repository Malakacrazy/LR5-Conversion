using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class WayOfThePhoenixTests
{
    [Test]
    public void PreventsTheOpponentFromDeclaringAConflictWithTheChosenRing()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var wayOfThePhoenix = new Card { Id = "way-of-the-phoenix", Type = CardType.Event, Controller = p1 };
        var ring = new Ring { Element = "fire", ConflictType = "military" };

        var context = new AbilityContext { Game = game, Player = p1, Source = wayOfThePhoenix, TargetRing = ring };

        new WayOfThePhoenixPreventOpponentDeclaringRingElement().Execute(context);

        Assert.That(game.CannotDeclareRingWith(p2, "fire"), Is.True);
        Assert.That(game.CannotDeclareRingWith(p1, "fire"), Is.False, "only restricts the opponent");
        Assert.That(game.CannotDeclareRingWith(p2, "water"), Is.False, "only restricts the chosen ring's element");
    }

    [Test]
    public void ExpiresAtTheEndOfThePhase()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var wayOfThePhoenix = new Card { Id = "way-of-the-phoenix", Type = CardType.Event, Controller = p1 };
        var ring = new Ring { Element = "fire", ConflictType = "military" };

        var context = new AbilityContext { Game = game, Player = p1, Source = wayOfThePhoenix, TargetRing = ring };
        new WayOfThePhoenixPreventOpponentDeclaringRingElement().Execute(context);

        game.AdvancePhase();

        Assert.That(game.CannotDeclareRingWith(p2, "fire"), Is.False);
    }
}
