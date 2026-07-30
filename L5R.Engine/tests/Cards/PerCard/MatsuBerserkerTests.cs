using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

/// <summary>A vanilla character - no "abilities" block. No printed political skill (a dash), unlike doji-whisperer's/eager-scout's real printed 0.</summary>
public class MatsuBerserkerTests
{
    [Test]
    public void HasItsPrintedStatsAndTraits()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var berserker = new Card
        {
            Id = "matsu-berserker", Type = CardType.Character, Controller = p1, Faction = "lion",
            Traits = new[] { "bushi", "berserker" }, PrintedCost = 1,
            PrintedMilitarySkill = 3, PrintedGlory = 1
        };
        p1.PlayArea.Add(berserker);

        Assert.That(game.EffectiveMilitarySkill(berserker), Is.EqualTo(3));
        Assert.That(berserker.PrintedPoliticalSkill, Is.Null, "no printed political skill - a dash");
        Assert.That(game.EffectiveGlory(berserker), Is.EqualTo(1));
        Assert.That(berserker.Traits, Is.EquivalentTo(new[] { "bushi", "berserker" }));
    }
}
