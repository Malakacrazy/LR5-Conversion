using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

/// <summary>
/// A vanilla character - no "abilities" block beyond the printed "covert" keyword. Unlike
/// tattooed-wanderer's *granted* covert (whileAttached "addKeyword"), this is printed
/// directly on the card, exercising GameState.HasKeyword's Card.PrintedKeywords check instead
/// of its HasAddEffect scan.
/// </summary>
public class UnassumingYojimboTests
{
    [Test]
    public void HasItsPrintedStatsTraitsAndKeyword()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var yojimbo = new Card
        {
            Id = "unassuming-yojimbo", Type = CardType.Character, Controller = p1, Faction = "scorpion",
            Traits = new[] { "bushi", "yojimbo" }, PrintedKeywords = new[] { "covert" },
            PrintedCost = 3, PrintedMilitarySkill = 3, PrintedPoliticalSkill = 1, PrintedGlory = 0
        };
        p1.PlayArea.Add(yojimbo);

        Assert.That(game.EffectiveMilitarySkill(yojimbo), Is.EqualTo(3));
        Assert.That(game.EffectivePoliticalSkill(yojimbo), Is.EqualTo(1));
        Assert.That(game.EffectiveGlory(yojimbo), Is.EqualTo(0));
        Assert.That(game.HasKeyword(yojimbo, "covert"), Is.True);
    }
}
