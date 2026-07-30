using L5R.Engine.GameSteps.BotActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.GameSteps.BotActions;

public class RebuildBotActionTests
{
    private static GameState NewScenario(out Player p1, out Card rebuild, out Card province, out Card holding)
    {
        p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };

        rebuild = new Card { Id = "rebuild", Type = CardType.Event, Controller = p1 };
        province = new Card { Id = "some-province-card", Type = CardType.Character, Controller = p1, ProvinceSlot = "province-1" };
        holding = new Card { Id = "discarded-holding", Type = CardType.Holding, Controller = p1, Location = "discard" };
        p1.Provinces.Add(province);
        p1.Discard.Add(holding);

        return game;
    }

    [Test]
    public void IsLegal_WithAnUnbrokenProvinceAndADiscardedHolding_True()
    {
        var game = NewScenario(out var p1, out var rebuild, out _, out _);

        Assert.That(new RebuildBotAction().IsLegal(game, rebuild, p1), Is.True);
    }

    [Test]
    public void IsLegal_WithNoHoldingInDiscard_False()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var rebuild = new Card { Id = "rebuild", Type = CardType.Event, Controller = p1 };
        p1.Provinces.Add(new Card { Id = "some-province-card", Type = CardType.Character, Controller = p1, ProvinceSlot = "province-1" });

        Assert.That(new RebuildBotAction().IsLegal(game, rebuild, p1), Is.False);
    }

    [Test]
    public void Invoke_SwapsTheProvinceCardForTheHolding()
    {
        var game = NewScenario(out var p1, out var rebuild, out var province, out var holding);

        new RebuildBotAction().Invoke(game, rebuild, p1);

        Assert.That(p1.Provinces, Does.Not.Contain(province));
        Assert.That(p1.Provinces, Contains.Item(holding));
        Assert.That(holding.ProvinceSlot, Is.EqualTo("province-1"));
        Assert.That(p1.Deck, Contains.Item(province));
    }
}
