using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class IAmReadyTests
{
    [Test]
    public void RemovesOneFateFromABowedUnicornCharacterAndReadiesIt()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var source = new Card { Id = "i-am-ready", Type = CardType.Event, Controller = p1 };
        var bowedUnicorn = new Card { Id = "bowed-unicorn", Type = CardType.Character, Controller = p1, Faction = "unicorn", Bowed = true, Fate = 2 };
        p1.PlayArea.Add(bowedUnicorn);

        var context = new AbilityContext { Game = game, Player = p1, Source = source, CostTarget = bowedUnicorn };

        new IAmReadyReadyTheRemoveFateCostTarget().Execute(context);

        Assert.That(bowedUnicorn.Fate, Is.EqualTo(1));
        Assert.That(bowedUnicorn.Bowed, Is.False);
    }

    [Test]
    public void CannotTargetANonUnicornCharacter()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var source = new Card { Id = "i-am-ready", Type = CardType.Event, Controller = p1 };
        var bowedLion = new Card { Id = "bowed-lion", Type = CardType.Character, Controller = p1, Faction = "lion", Bowed = true, Fate = 2 };
        p1.PlayArea.Add(bowedLion);

        var context = new AbilityContext { Game = game, Player = p1, Source = source, CostTarget = bowedLion };

        Assert.Throws<InvalidOperationException>(() => new IAmReadyReadyTheRemoveFateCostTarget().Execute(context));
    }

    [Test]
    public void CannotTargetAnAlreadyReadyCharacter()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var source = new Card { Id = "i-am-ready", Type = CardType.Event, Controller = p1 };
        var readyUnicorn = new Card { Id = "ready-unicorn", Type = CardType.Character, Controller = p1, Faction = "unicorn", Bowed = false, Fate = 2 };
        p1.PlayArea.Add(readyUnicorn);

        var context = new AbilityContext { Game = game, Player = p1, Source = source, CostTarget = readyUnicorn };

        Assert.Throws<InvalidOperationException>(() => new IAmReadyReadyTheRemoveFateCostTarget().Execute(context));
    }

    [Test]
    public void CannotTargetACharacterWithNoFate()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var source = new Card { Id = "i-am-ready", Type = CardType.Event, Controller = p1 };
        var bowedUnicorn = new Card { Id = "bowed-unicorn", Type = CardType.Character, Controller = p1, Faction = "unicorn", Bowed = true, Fate = 0 };
        p1.PlayArea.Add(bowedUnicorn);

        var context = new AbilityContext { Game = game, Player = p1, Source = source, CostTarget = bowedUnicorn };

        Assert.Throws<InvalidOperationException>(() => new IAmReadyReadyTheRemoveFateCostTarget().Execute(context));
    }
}
