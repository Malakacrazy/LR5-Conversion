using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

/// <summary>A vanilla character - no "abilities" block, printed keywords, or traits beyond courtier; its entire game text is its printed stat line.</summary>
public class DojiWhispererTests
{
    [Test]
    public void HasItsPrintedStatsAndTraits()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var whisperer = new Card
        {
            Id = "doji-whisperer", Type = CardType.Character, Controller = p1, Faction = "crane",
            Traits = new[] { "courtier" }, PrintedCost = 1,
            PrintedMilitarySkill = 0, PrintedPoliticalSkill = 3, PrintedGlory = 1
        };
        p1.PlayArea.Add(whisperer);

        Assert.That(game.EffectiveMilitarySkill(whisperer), Is.EqualTo(0));
        Assert.That(game.EffectivePoliticalSkill(whisperer), Is.EqualTo(3));
        Assert.That(game.EffectiveGlory(whisperer), Is.EqualTo(1));
        Assert.That(whisperer.Traits, Is.EquivalentTo(new[] { "courtier" }));
    }
}
