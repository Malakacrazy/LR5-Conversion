using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class EnlightenedWarriorTests
{
    private static (GameState Game, Card Warrior, Ring Ring) NewGameWithOpponentAttackingOnAFatedRing()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var warrior = new Card { Id = "enlightened-warrior", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(warrior);

        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1 };
        game.CurrentConflict = conflict;

        var ring = game.Rings.Single(r => r.Element == "fire");
        ring.Fate = 2;

        return (game, warrior, ring);
    }

    [Test]
    public void PlacesOneFateOnItself()
    {
        var (game, warrior, ring) = NewGameWithOpponentAttackingOnAFatedRing();

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = warrior, TargetRing = ring };

        new EnlightenedWarriorGainFateOnOpponentRingSelect().Execute(context);

        Assert.That(warrior.Fate, Is.EqualTo(1));
    }

    [Test]
    public void WhenTheRingHasNoFate_Throws()
    {
        var (game, warrior, ring) = NewGameWithOpponentAttackingOnAFatedRing();
        ring.Fate = 0;

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = warrior, TargetRing = ring };

        Assert.Throws<InvalidOperationException>(() => new EnlightenedWarriorGainFateOnOpponentRingSelect().Execute(context));
    }

    [Test]
    public void WhenTheControllerIsTheAttacker_Throws()
    {
        var (game, warrior, ring) = NewGameWithOpponentAttackingOnAFatedRing();
        game.CurrentConflict = new Conflict { AttackingPlayer = game.Player1, DefendingPlayer = game.Player2 };

        var context = new AbilityContext { Game = game, Player = game.Player1, Source = warrior, TargetRing = ring };

        Assert.Throws<InvalidOperationException>(() => new EnlightenedWarriorGainFateOnOpponentRingSelect().Execute(context));
    }
}
