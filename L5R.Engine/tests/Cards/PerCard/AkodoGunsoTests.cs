using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class AkodoGunsoTests
{
    [Test]
    public void RefillsTheSlotItEnteredPlayFromWithTheTopOfTheDeckFaceup()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var gunso = new Card { Id = "akodo-gunso", Type = CardType.Character, Controller = p1 };
        var topOfDeck = new Card { Id = "top-of-deck", Type = CardType.Character, Controller = p1, Facedown = true };
        p1.PlayArea.Add(gunso);
        p1.Deck.Add(topOfDeck);

        var context = new AbilityContext { Game = game, Player = p1, Source = gunso, ProvinceSlot = "province-3" };

        new AkodoGunsoRefillProvinceOnEnteringFromProvince().Execute(context);

        Assert.That(p1.Provinces, Does.Contain(topOfDeck));
        Assert.That(topOfDeck.ProvinceSlot, Is.EqualTo("province-3"));
        Assert.That(topOfDeck.Facedown, Is.False);
        Assert.That(p1.Deck, Does.Not.Contain(topOfDeck));
    }

    [Test]
    public void WithNoProvinceSlotSupplied_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var gunso = new Card { Id = "akodo-gunso", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(gunso);
        p1.Deck.Add(new Card { Id = "top-of-deck", Type = CardType.Character, Controller = p1 });

        var context = new AbilityContext { Game = game, Player = p1, Source = gunso };

        Assert.Throws<InvalidOperationException>(() => new AkodoGunsoRefillProvinceOnEnteringFromProvince().Execute(context));
    }

    [Test]
    public void WithAnEmptyDeck_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var gunso = new Card { Id = "akodo-gunso", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(gunso);

        var context = new AbilityContext { Game = game, Player = p1, Source = gunso, ProvinceSlot = "province-3" };

        Assert.Throws<InvalidOperationException>(() => new AkodoGunsoRefillProvinceOnEnteringFromProvince().Execute(context));
    }
}
