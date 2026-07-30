using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

/// <summary>A vanilla character - no "abilities" block beyond the printed "sincerity" keyword (see BayushiLiarTests' own doc comment on why its rules effect isn't implemented). No printed military skill (a dash).</summary>
public class NaiveStudentTests
{
    [Test]
    public void HasItsPrintedStatsTraitsAndKeyword()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var student = new Card
        {
            Id = "naive-student", Type = CardType.Character, Controller = p1, Faction = "phoenix",
            Traits = new[] { "courtier", "scholar" }, PrintedKeywords = new[] { "sincerity" },
            PrintedCost = 1, PrintedPoliticalSkill = 2, PrintedGlory = 2
        };
        p1.PlayArea.Add(student);

        Assert.That(student.PrintedMilitarySkill, Is.Null, "no printed military skill - a dash");
        Assert.That(game.EffectivePoliticalSkill(student), Is.EqualTo(2));
        Assert.That(game.EffectiveGlory(student), Is.EqualTo(2));
        Assert.That(student.Traits, Is.EquivalentTo(new[] { "courtier", "scholar" }));
        Assert.That(game.HasKeyword(student, "sincerity"), Is.True);
    }
}
