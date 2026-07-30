using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Dsl;

public class ShosuroMiyakoFirerTests
{
    [Test]
    public void FireEligibleReactions_WhenTheOpponentHasACharacter_DishonorsIt()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var miyako = new Card { Id = "shosuro-miyako", Type = CardType.Character, Controller = p1 };
        var opponentCharacter = new Card { Id = "opponent-character", Type = CardType.Character, Controller = p2 };
        p1.PlayArea.Add(miyako);
        p2.PlayArea.Add(opponentCharacter);

        ShosuroMiyakoFirer.FireEligibleReactions(game, p1);

        Assert.That(opponentCharacter.IsDishonored, Is.True);
    }

    [Test]
    public void FireEligibleReactions_WhenTheOpponentHasNoCharacterButHasAHandCard_DiscardsIt()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var miyako = new Card { Id = "shosuro-miyako", Type = CardType.Character, Controller = p1 };
        var opponentHandCard = new Card { Id = "opponent-hand-card", Type = CardType.Character, Controller = p2, Location = "hand" };
        p1.PlayArea.Add(miyako);
        p2.Hand.Add(opponentHandCard);

        ShosuroMiyakoFirer.FireEligibleReactions(game, p1);

        Assert.That(p2.Discard, Does.Contain(opponentHandCard));
    }

    [Test]
    public void FireEligibleReactions_WithNoShosuroMiyakoInPlay_DoesNothing()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var opponentCharacter = new Card { Id = "opponent-character", Type = CardType.Character, Controller = p2 };
        p2.PlayArea.Add(opponentCharacter);

        ShosuroMiyakoFirer.FireEligibleReactions(game, p1);

        Assert.That(opponentCharacter.IsDishonored, Is.False);
    }
}
