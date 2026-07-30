using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class KitsuSpiritcallerTests
{
    [Test]
    public void BowsSelfAndPutsADiscardedCharacterIntoTheConflict()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var spiritcaller = new Card { Id = "kitsu-spiritcaller", Type = CardType.Character, Controller = p1 };
        var target = new Card { Id = "discarded-character", Type = CardType.Character, Controller = p1, Location = "discard" };
        p1.PlayArea.Add(spiritcaller);
        p1.Discard.Add(target);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = spiritcaller, Target = target };

        new KitsuSpiritcallerResurrectUntilConflictEnd().Execute(context);

        Assert.That(spiritcaller.Bowed, Is.True);
        Assert.That(p1.PlayArea, Does.Contain(target));
        Assert.That(conflict.Attackers, Does.Contain(target));
    }

    [Test]
    public void WhenTheConflictEnds_ReturnsTheResurrectedCharacterToTheBottomOfTheDeck()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var spiritcaller = new Card { Id = "kitsu-spiritcaller", Type = CardType.Character, Controller = p1 };
        var target = new Card { Id = "discarded-character", Type = CardType.Character, Controller = p1, Location = "discard" };
        var existingDeckCard = new Card { Id = "existing-deck-card", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(spiritcaller);
        p1.Discard.Add(target);
        p1.Deck.Add(existingDeckCard);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = spiritcaller, Target = target };
        new KitsuSpiritcallerResurrectUntilConflictEnd().Execute(context);

        game.EndConflict();

        Assert.That(p1.PlayArea, Does.Not.Contain(target));
        Assert.That(p1.Deck[^1], Is.EqualTo(target), "returns to the bottom of the deck");
        Assert.That(p1.Deck[0], Is.EqualTo(existingDeckCard));
    }

    [Test]
    public void WhenTheTargetIsNotInTheDiscardPile_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var spiritcaller = new Card { Id = "kitsu-spiritcaller", Type = CardType.Character, Controller = p1 };
        var target = new Card { Id = "not-discarded", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(spiritcaller);
        p1.Hand.Add(target);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = spiritcaller, Target = target };

        Assert.Throws<InvalidOperationException>(() => new KitsuSpiritcallerResurrectUntilConflictEnd().Execute(context));
    }
}
