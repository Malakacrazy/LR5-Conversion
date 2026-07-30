using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class ShibaTsukuneTests
{
    [Test]
    public void ResolvesAChosenUnclaimedRing()
    {
        var p1 = new Player { Name = "Player1", Honor = 3 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var tsukune = new Card { Id = "shiba-tsukune", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(tsukune);

        var ring = new Ring { Element = "air", ConflictType = "military" };
        var context = new AbilityContext { Game = game, Player = p1, Source = tsukune, TargetRing = ring, ChosenChoice = "Gain 2 Honor" };

        new ShibaTsukuneResolveUpToTwoRings().Execute(context);

        Assert.That(p1.Honor, Is.EqualTo(5));
    }

    [Test]
    public void CanResolveASecondRingWithASeparateCall()
    {
        var p1 = new Player { Name = "Player1", Honor = 3 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var tsukune = new Card { Id = "shiba-tsukune", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(tsukune);

        var firstRing = new Ring { Element = "air", ConflictType = "military" };
        var secondRing = new Ring { Element = "void", ConflictType = "political" };
        var target = new Card { Id = "some-character", Type = CardType.Character, Controller = p1, Fate = 2 };
        p1.PlayArea.Add(target);

        new ShibaTsukuneResolveUpToTwoRings().Execute(
            new AbilityContext { Game = game, Player = p1, Source = tsukune, TargetRing = firstRing, ChosenChoice = "Gain 2 Honor" });
        new ShibaTsukuneResolveUpToTwoRings().Execute(
            new AbilityContext { Game = game, Player = p1, Source = tsukune, TargetRing = secondRing, Target = target });

        Assert.That(p1.Honor, Is.EqualTo(5));
        Assert.That(target.Fate, Is.EqualTo(1));
    }

    [Test]
    public void OutsideTheConflictPhase_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Fate };
        var tsukune = new Card { Id = "shiba-tsukune", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(tsukune);

        var ring = new Ring { Element = "air", ConflictType = "military" };
        var context = new AbilityContext { Game = game, Player = p1, Source = tsukune, TargetRing = ring };

        Assert.Throws<InvalidOperationException>(() => new ShibaTsukuneResolveUpToTwoRings().Execute(context));
    }
}
