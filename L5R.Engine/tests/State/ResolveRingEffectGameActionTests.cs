using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.State;

/// <summary>
/// shiba-tsukune is the only ported card using "resolveRingEffect" (as opposed to
/// "resolveConflictRing"), so its per-element dispatch (shared with
/// ResolveConflictRingGameActionHandler.ResolveElement) is proven directly here, the same
/// pattern already used for ResolveConflictRingGameActionTests.
/// </summary>
public class ResolveRingEffectGameActionTests
{
    private static (GameState Game, Card Source) NewGame()
    {
        var p1 = new Player { Name = "Player1", Honor = 3 };
        var p2 = new Player { Name = "Player2", Honor = 3 };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var source = new Card { Id = "ring-resolver", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(source);
        return (game, source);
    }

    [Test]
    public void ResolvesAnUnclaimedRingWithNoConflict()
    {
        var (game, source) = NewGame();
        var ring = new Ring { Element = "air", ConflictType = "military" };
        var context = new AbilityContext { Game = game, Player = game.Player1, Source = source, TargetRing = ring, ChosenChoice = "Gain 2 Honor" };

        new ResolveRingEffectGameActionHandler().Execute(context, null);

        Assert.That(game.Player1.Honor, Is.EqualTo(5));
    }

    [Test]
    public void WhenTheRingIsAlreadyClaimed_Throws()
    {
        var (game, source) = NewGame();
        var ring = new Ring { Element = "air", ConflictType = "military", Claimed = true, ClaimedBy = game.Player2 };
        var context = new AbilityContext { Game = game, Player = game.Player1, Source = source, TargetRing = ring, ChosenChoice = "Gain 2 Honor" };

        Assert.Throws<InvalidOperationException>(() => new ResolveRingEffectGameActionHandler().Execute(context, null));
    }

    [Test]
    public void WithNoTargetRing_Throws()
    {
        var (game, source) = NewGame();
        var context = new AbilityContext { Game = game, Player = game.Player1, Source = source };

        Assert.Throws<InvalidOperationException>(() => new ResolveRingEffectGameActionHandler().Execute(context, null));
    }
}
