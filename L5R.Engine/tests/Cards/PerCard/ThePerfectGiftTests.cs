using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class ThePerfectGiftTests
{
    private static (GameState Game, Card MyCard, Card OpponentCard) NewGameWithTopFourEach()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };

        var myCard = new Card { Id = "my-top-card", Type = CardType.Character, Controller = p1 };
        for (var i = 0; i < 4; i++)
            p1.Deck.Add(i == 0 ? myCard : new Card { Id = $"my-deck-filler-{i}", Type = CardType.Character, Controller = p1 });

        var opponentCard = new Card { Id = "opponent-top-card", Type = CardType.Character, Controller = p2 };
        for (var i = 0; i < 4; i++)
            p2.Deck.Add(i == 0 ? opponentCard : new Card { Id = $"opponent-deck-filler-{i}", Type = CardType.Character, Controller = p2 });

        return (game, myCard, opponentCard);
    }

    [Test]
    public void GivesEachPlayerTheirChosenCardFromTheirOwnTopFour()
    {
        var (game, myCard, opponentCard) = NewGameWithTopFourEach();
        var perfectGift = new Card { Id = "the-perfect-gift", Type = CardType.Event, Controller = game.Player1 };

        var context = new AbilityContext
        {
            Game = game, Player = game.Player1, Source = perfectGift,
            ChosenCardMenuCard = myCard, ChosenDeckSearchCard = opponentCard
        };

        new ThePerfectGiftRevealAndGiveEachPlayerACard().Execute(context);

        Assert.That(game.Player1.Hand, Does.Contain(myCard));
        Assert.That(game.Player2.Hand, Does.Contain(opponentCard));
    }

    [Test]
    public void ChoosingACardOutsideMyOwnTopFour_Throws()
    {
        var (game, _, opponentCard) = NewGameWithTopFourEach();
        var perfectGift = new Card { Id = "the-perfect-gift", Type = CardType.Event, Controller = game.Player1 };
        var deepCard = new Card { Id = "too-deep", Type = CardType.Character, Controller = game.Player1 };
        game.Player1.Deck.Add(deepCard);

        var context = new AbilityContext
        {
            Game = game, Player = game.Player1, Source = perfectGift,
            ChosenCardMenuCard = deepCard, ChosenDeckSearchCard = opponentCard
        };

        Assert.Throws<InvalidOperationException>(() => new ThePerfectGiftRevealAndGiveEachPlayerACard().Execute(context));
    }

    [Test]
    public void ChoosingACardOutsideTheOpponentsOwnTopFour_Throws()
    {
        var (game, myCard, _) = NewGameWithTopFourEach();
        var perfectGift = new Card { Id = "the-perfect-gift", Type = CardType.Event, Controller = game.Player1 };
        var deepCard = new Card { Id = "too-deep", Type = CardType.Character, Controller = game.Player2 };
        game.Player2.Deck.Add(deepCard);

        var context = new AbilityContext
        {
            Game = game, Player = game.Player1, Source = perfectGift,
            ChosenCardMenuCard = myCard, ChosenDeckSearchCard = deepCard
        };

        Assert.Throws<InvalidOperationException>(() => new ThePerfectGiftRevealAndGiveEachPlayerACard().Execute(context));
    }
}
