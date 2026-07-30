using L5R.Engine.GameSteps.BotActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.GameSteps.BotActions;

public class BorderlandsFortificationsBotActionTests
{
    [Test]
    public void IsLegal_WhenAnotherProvinceCardExists_True()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var fortifications = new Card { Id = "borderlands-fortifications", Type = CardType.Holding, Controller = p1, ProvinceSlot = "province-1" };
        var other = new Card { Id = "other-province-card", Type = CardType.Character, Controller = p1, ProvinceSlot = "province-2" };
        p1.Provinces.Add(fortifications);
        p1.Provinces.Add(other);

        Assert.That(new BorderlandsFortificationsBotAction().IsLegal(game, fortifications, p1), Is.True);
    }

    [Test]
    public void IsLegal_WhenItIsTheOnlyProvinceCard_False()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var fortifications = new Card { Id = "borderlands-fortifications", Type = CardType.Holding, Controller = p1, ProvinceSlot = "province-1" };
        p1.Provinces.Add(fortifications);

        Assert.That(new BorderlandsFortificationsBotAction().IsLegal(game, fortifications, p1), Is.False);
    }

    [Test]
    public void Invoke_SwapsProvinceSlots()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var fortifications = new Card { Id = "borderlands-fortifications", Type = CardType.Holding, Controller = p1, ProvinceSlot = "province-1" };
        var other = new Card { Id = "other-province-card", Type = CardType.Character, Controller = p1, ProvinceSlot = "province-2" };
        p1.Provinces.Add(fortifications);
        p1.Provinces.Add(other);

        new BorderlandsFortificationsBotAction().Invoke(game, fortifications, p1);

        Assert.That(fortifications.ProvinceSlot, Is.EqualTo("province-2"));
        Assert.That(other.ProvinceSlot, Is.EqualTo("province-1"));
    }
}
