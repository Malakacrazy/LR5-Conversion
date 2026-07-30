using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Dsl;

public class AkodoGunsoFirerTests
{
    [Test]
    public void FireIfLegal_RefillsTheVacatedSlotWithTheTopOfDeck()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var gunso = new Card { Id = "akodo-gunso", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(gunso);
        var refill = new Card { Id = "refill-card", Type = CardType.Character, Controller = p1 };
        p1.Deck.Add(refill);

        AkodoGunsoFirer.FireIfLegal(game, gunso, "2");

        Assert.That(p1.Deck, Does.Not.Contain(refill));
        Assert.That(p1.Provinces, Contains.Item(refill));
        Assert.That(refill.ProvinceSlot, Is.EqualTo("2"));
        Assert.That(refill.Facedown, Is.False);
    }

    [Test]
    public void FireIfLegal_ForADifferentCard_DoesNotFire()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var otherCharacter = new Card { Id = "some-other-character", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(otherCharacter);
        var refill = new Card { Id = "refill-card", Type = CardType.Character, Controller = p1 };
        p1.Deck.Add(refill);

        AkodoGunsoFirer.FireIfLegal(game, otherCharacter, "2");

        Assert.That(p1.Deck, Contains.Item(refill));
        Assert.That(p1.Provinces, Does.Not.Contain(refill));
    }
}
