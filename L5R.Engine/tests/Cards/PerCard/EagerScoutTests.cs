using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

/// <summary>A vanilla 0-cost/0-stat character - no "abilities" block; its entire game text is its printed stat line and traits.</summary>
public class EagerScoutTests
{
    [Test]
    public void HasItsPrintedStatsAndTraits()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var scout = new Card
        {
            Id = "eager-scout", Type = CardType.Character, Controller = p1, Faction = "crab",
            Traits = new[] { "bushi", "scout" }, PrintedCost = 0,
            PrintedMilitarySkill = 0, PrintedPoliticalSkill = 0, PrintedGlory = 0
        };
        p1.PlayArea.Add(scout);

        Assert.That(game.EffectiveMilitarySkill(scout), Is.EqualTo(0));
        Assert.That(game.EffectivePoliticalSkill(scout), Is.EqualTo(0));
        Assert.That(game.EffectiveGlory(scout), Is.EqualTo(0));
        Assert.That(scout.Traits, Is.EquivalentTo(new[] { "bushi", "scout" }));
    }
}
