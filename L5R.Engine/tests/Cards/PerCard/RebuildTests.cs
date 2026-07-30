using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class RebuildTests
{
    [Test]
    public void ReplacesAnUnbrokenProvinceCardWithAHoldingFromDiscard()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var rebuild = new Card { Id = "rebuild", Type = CardType.Event, Controller = p1 };
        var oldProvinceCard = new Card { Id = "old-province-card", Type = CardType.Character, Controller = p1, ProvinceSlot = "province-1" };
        var holding = new Card { Id = "discarded-holding", Type = CardType.Holding, Controller = p1, Location = "discard", Facedown = true };
        p1.Provinces.Add(oldProvinceCard);
        p1.Discard.Add(holding);

        var context = new AbilityContext { Game = game, Player = p1, Source = rebuild, CostTarget = oldProvinceCard, Target = holding };

        new RebuildReplaceProvinceCardWithHolding().Execute(context);

        Assert.That(p1.Provinces, Does.Not.Contain(oldProvinceCard));
        Assert.That(p1.Deck, Does.Contain(oldProvinceCard));
        Assert.That(p1.Provinces, Does.Contain(holding));
        Assert.That(holding.ProvinceSlot, Is.EqualTo("province-1"));
        Assert.That(holding.Facedown, Is.False);
    }

    [Test]
    public void WithABrokenProvinceCard_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var rebuild = new Card { Id = "rebuild", Type = CardType.Event, Controller = p1 };
        var brokenProvinceCard = new Card { Id = "broken-province-card", Type = CardType.Character, Controller = p1, ProvinceSlot = "province-1", Broken = true };
        var holding = new Card { Id = "discarded-holding", Type = CardType.Holding, Controller = p1, Location = "discard" };
        p1.Provinces.Add(brokenProvinceCard);
        p1.Discard.Add(holding);

        var context = new AbilityContext { Game = game, Player = p1, Source = rebuild, CostTarget = brokenProvinceCard, Target = holding };

        Assert.Throws<InvalidOperationException>(() => new RebuildReplaceProvinceCardWithHolding().Execute(context));
    }

    [Test]
    public void WithATargetThatIsNotAHolding_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var rebuild = new Card { Id = "rebuild", Type = CardType.Event, Controller = p1 };
        var oldProvinceCard = new Card { Id = "old-province-card", Type = CardType.Character, Controller = p1, ProvinceSlot = "province-1" };
        var notAHolding = new Card { Id = "discarded-character", Type = CardType.Character, Controller = p1, Location = "discard" };
        p1.Provinces.Add(oldProvinceCard);
        p1.Discard.Add(notAHolding);

        var context = new AbilityContext { Game = game, Player = p1, Source = rebuild, CostTarget = oldProvinceCard, Target = notAHolding };

        Assert.Throws<InvalidOperationException>(() => new RebuildReplaceProvinceCardWithHolding().Execute(context));
    }
}
