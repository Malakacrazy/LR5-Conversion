using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

/// <summary>
/// A vanilla character - no "abilities" block, so its entire game text is its printed stats,
/// traits, and the "sincerity" keyword (its actual rules effect - draw 1 card when this
/// leaves play - isn't implemented anywhere yet; no ported card's tested behavior needs it).
/// No printed military skill (a "dash" - this card can't be committed to a military conflict,
/// a rule this engine doesn't enforce anywhere), unlike doji-whisperer/eager-scout's real 0.
/// </summary>
public class BayushiLiarTests
{
    [Test]
    public void HasItsPrintedStatsTraitsAndKeyword()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var liar = new Card
        {
            Id = "bayushi-liar", Type = CardType.Character, Controller = p1, Faction = "scorpion",
            Traits = new[] { "courtier" }, PrintedKeywords = new[] { "sincerity" },
            PrintedCost = 1, PrintedPoliticalSkill = 3, PrintedGlory = 0
        };
        p1.PlayArea.Add(liar);

        Assert.That(liar.PrintedMilitarySkill, Is.Null, "no printed military skill - a dash");
        Assert.That(game.EffectivePoliticalSkill(liar), Is.EqualTo(3));
        Assert.That(game.EffectiveGlory(liar), Is.EqualTo(0));
        Assert.That(liar.Traits, Is.EquivalentTo(new[] { "courtier" }));
        Assert.That(game.HasKeyword(liar, "sincerity"), Is.True);
    }
}
