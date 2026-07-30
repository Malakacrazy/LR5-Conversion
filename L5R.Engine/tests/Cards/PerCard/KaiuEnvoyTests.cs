using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

/// <summary>
/// A vanilla character - no "abilities" block; its entire game text is its printed stats,
/// traits, and the "courtesy"/"sincerity" keywords (their actual rules effects aren't
/// implemented anywhere yet; no ported card's tested behavior needs them).
/// </summary>
public class KaiuEnvoyTests
{
    [Test]
    public void HasItsPrintedStatsTraitsAndKeywords()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var envoy = new Card
        {
            Id = "kaiu-envoy", Type = CardType.Character, Controller = p1, Faction = "crab",
            Traits = new[] { "bushi" }, PrintedKeywords = new[] { "courtesy", "sincerity" },
            PrintedCost = 1, PrintedMilitarySkill = 1, PrintedPoliticalSkill = 0, PrintedGlory = 1
        };
        p1.PlayArea.Add(envoy);

        Assert.That(game.EffectiveMilitarySkill(envoy), Is.EqualTo(1));
        Assert.That(game.EffectivePoliticalSkill(envoy), Is.EqualTo(0));
        Assert.That(game.EffectiveGlory(envoy), Is.EqualTo(1));
        Assert.That(game.HasKeyword(envoy, "courtesy"), Is.True);
        Assert.That(game.HasKeyword(envoy, "sincerity"), Is.True);
    }
}
