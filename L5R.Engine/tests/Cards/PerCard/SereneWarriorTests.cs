using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

/// <summary>A vanilla character - no "abilities" block; its entire game text is its printed stat line.</summary>
public class SereneWarriorTests
{
    [Test]
    public void HasItsPrintedStatsAndTraits()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var warrior = new Card
        {
            Id = "serene-warrior", Type = CardType.Character, Controller = p1, Faction = "phoenix",
            Traits = new[] { "bushi" }, PrintedCost = 3,
            PrintedMilitarySkill = 3, PrintedPoliticalSkill = 2, PrintedGlory = 4
        };
        p1.PlayArea.Add(warrior);

        Assert.That(game.EffectiveMilitarySkill(warrior), Is.EqualTo(3));
        Assert.That(game.EffectivePoliticalSkill(warrior), Is.EqualTo(2));
        Assert.That(game.EffectiveGlory(warrior), Is.EqualTo(4));
        Assert.That(warrior.Traits, Is.EquivalentTo(new[] { "bushi" }));
    }
}
