using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

/// <summary>A vanilla character - no "abilities" block. No printed political skill (a dash), same convention as matsu-berserker.</summary>
public class MotoHordeTests
{
    [Test]
    public void HasItsPrintedStatsAndTraits()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var horde = new Card
        {
            Id = "moto-horde", Type = CardType.Character, Controller = p1, Faction = "unicorn",
            Traits = new[] { "bushi", "army", "cavalry" }, PrintedCost = 4,
            PrintedMilitarySkill = 6, PrintedGlory = 1
        };
        p1.PlayArea.Add(horde);

        Assert.That(game.EffectiveMilitarySkill(horde), Is.EqualTo(6));
        Assert.That(horde.PrintedPoliticalSkill, Is.Null, "no printed political skill - a dash");
        Assert.That(game.EffectiveGlory(horde), Is.EqualTo(1));
        Assert.That(horde.Traits, Is.EquivalentTo(new[] { "bushi", "army", "cavalry" }));
    }
}
