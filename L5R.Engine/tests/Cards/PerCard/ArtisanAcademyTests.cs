using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class ArtisanAcademyTests
{
    [Test]
    public void DuringConflictPhaseWithCardsInDeck_SucceedsAndTopCardIsThenPlayable()
    {
        // Nothing needs tracking beyond this precondition check - PlayCardGameActionHandler
        // already moves context.Target regardless of its current location, so the caller can
        // play Player.Deck[0] directly afterwards, same as any other card.
        var p1 = new Player { Name = "Player1", Fate = 5 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var academy = new Card { Id = "artisan-academy", Type = CardType.Holding, Controller = p1 };
        var topOfDeck = new Card { Id = "top-of-deck", Type = CardType.Character, Controller = p1, PrintedCost = 2 };
        p1.PlayArea.Add(academy);
        p1.Deck.Add(topOfDeck);

        var context = new AbilityContext { Game = game, Player = p1, Source = academy };

        new ArtisanAcademyRevealTopCard().Execute(context);

        var playContext = new AbilityContext { Game = game, Player = p1, Source = academy, Target = topOfDeck };
        new PlayCardGameActionHandler().Execute(playContext, null);

        Assert.That(p1.PlayArea, Contains.Item(topOfDeck));
        Assert.That(p1.Deck, Does.Not.Contain(topOfDeck));
        Assert.That(p1.Fate, Is.EqualTo(3));
    }

    [Test]
    public void OutsideTheConflictPhase_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Dynasty };
        var academy = new Card { Id = "artisan-academy", Type = CardType.Holding, Controller = p1 };
        p1.Deck.Add(new Card { Id = "some-card", Type = CardType.Character, Controller = p1 });

        var context = new AbilityContext { Game = game, Player = p1, Source = academy };

        Assert.Throws<InvalidOperationException>(() => new ArtisanAcademyRevealTopCard().Execute(context));
    }

    [Test]
    public void WithAnEmptyDeck_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var academy = new Card { Id = "artisan-academy", Type = CardType.Holding, Controller = p1 };

        var context = new AbilityContext { Game = game, Player = p1, Source = academy };

        Assert.Throws<InvalidOperationException>(() => new ArtisanAcademyRevealTopCard().Execute(context));
    }
}
