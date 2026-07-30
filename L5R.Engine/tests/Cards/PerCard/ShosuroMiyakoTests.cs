using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class ShosuroMiyakoTests
{
    [Test]
    public void ChoosingDiscard_DiscardsAChosenCardFromTheOpponentsHand()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var miyako = new Card { Id = "shosuro-miyako", Type = CardType.Character, Controller = p1 };
        var opponentCard = new Card { Id = "opponent-card", Type = CardType.Character, Controller = p2, Location = "hand" };
        p1.PlayArea.Add(miyako);
        p2.Hand.Add(opponentCard);

        var context = new AbilityContext { Game = game, Player = p1, Source = miyako, ChosenChoice = "Discard at random", ChosenDiscardCards = new[] { opponentCard } };

        new ShosuroMiyakoDiscardOrDishonorOnCharacterPlayed().Execute(context);

        Assert.That(p2.Discard, Does.Contain(opponentCard));
    }

    [Test]
    public void ChoosingDishonor_DishonorsAnOpponentCharacter()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var miyako = new Card { Id = "shosuro-miyako", Type = CardType.Character, Controller = p1 };
        var opponentCharacter = new Card { Id = "opponent-character", Type = CardType.Character, Controller = p2 };
        p1.PlayArea.Add(miyako);
        p2.PlayArea.Add(opponentCharacter);

        var context = new AbilityContext { Game = game, Player = p1, Source = miyako, ChosenChoice = "Dishonor a character", Target = opponentCharacter };

        new ShosuroMiyakoDiscardOrDishonorOnCharacterPlayed().Execute(context);

        Assert.That(opponentCharacter.IsDishonored, Is.True);
    }

    [Test]
    public void DishonoringAFriendlyCharacter_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var miyako = new Card { Id = "shosuro-miyako", Type = CardType.Character, Controller = p1 };
        var myCharacter = new Card { Id = "my-character", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(miyako);
        p1.PlayArea.Add(myCharacter);

        var context = new AbilityContext { Game = game, Player = p1, Source = miyako, ChosenChoice = "Dishonor a character", Target = myCharacter };

        Assert.Throws<InvalidOperationException>(() => new ShosuroMiyakoDiscardOrDishonorOnCharacterPlayed().Execute(context));
    }
}
