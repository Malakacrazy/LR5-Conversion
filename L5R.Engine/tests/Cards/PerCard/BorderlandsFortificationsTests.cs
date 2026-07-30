using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class BorderlandsFortificationsTests
{
    [Test]
    public void SwapsProvinceSlotsWithAnotherProvinceCard()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var fortifications = new Card { Id = "borderlands-fortifications", Type = CardType.Holding, Controller = p1, ProvinceSlot = "province-1" };
        var other = new Card { Id = "other-province-card", Type = CardType.Character, Controller = p1, ProvinceSlot = "province-2" };
        p1.Provinces.Add(fortifications);
        p1.Provinces.Add(other);

        var context = new AbilityContext { Game = game, Player = p1, Source = fortifications, Target = other };

        new BorderlandsFortificationsSwapWithProvinceCard().Execute(context);

        Assert.That(fortifications.ProvinceSlot, Is.EqualTo("province-2"));
        Assert.That(other.ProvinceSlot, Is.EqualTo("province-1"));
    }

    [Test]
    public void SwappingWithAnOpponentsCard_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var fortifications = new Card { Id = "borderlands-fortifications", Type = CardType.Holding, Controller = p1, ProvinceSlot = "province-1" };
        var opponentCard = new Card { Id = "opponent-province-card", Type = CardType.Character, Controller = p2, ProvinceSlot = "province-1" };
        p1.Provinces.Add(fortifications);
        p2.Provinces.Add(opponentCard);

        var context = new AbilityContext { Game = game, Player = p1, Source = fortifications, Target = opponentCard };

        Assert.Throws<InvalidOperationException>(() => new BorderlandsFortificationsSwapWithProvinceCard().Execute(context));
    }

    [Test]
    public void WhenItsOwnerIsNotInAProvince_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var fortifications = new Card { Id = "borderlands-fortifications", Type = CardType.Holding, Controller = p1 };
        var other = new Card { Id = "other-province-card", Type = CardType.Character, Controller = p1, ProvinceSlot = "province-2" };
        p1.PlayArea.Add(fortifications);
        p1.Provinces.Add(other);

        var context = new AbilityContext { Game = game, Player = p1, Source = fortifications, Target = other };

        Assert.Throws<InvalidOperationException>(() => new BorderlandsFortificationsSwapWithProvinceCard().Execute(context));
    }
}
