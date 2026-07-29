using System.Text.Json;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class SeekerOfEnlightenmentTests
{
    private static IReadOnlyList<PersistentEffectDefinition> LoadPersistentEffects()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "seeker-of-enlightenment.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParsePersistentEffects(document.RootElement);
    }

    [Test]
    public void BothSkillsAreBoostedByTheFateOnUnclaimedRings()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var seeker = new Card
        {
            Id = "seeker-of-enlightenment", Type = CardType.Character, Controller = p1,
            PrintedMilitarySkill = 1, PrintedPoliticalSkill = 1, PersistentEffects = LoadPersistentEffects()
        };
        p1.PlayArea.Add(seeker);

        game.Rings.Add(new Ring { Element = "air", ConflictType = "military", Fate = 2 });
        game.Rings.Add(new Ring { Element = "fire", ConflictType = "military", Fate = 1, Claimed = true, ClaimedBy = p1 });

        Assert.That(game.EffectiveMilitarySkill(seeker), Is.EqualTo(3), "only the 2 fate on the unclaimed air ring counts");
        Assert.That(game.EffectivePoliticalSkill(seeker), Is.EqualTo(3));
    }

    [Test]
    public void WithNoFateOnUnclaimedRings_NoBonus()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var seeker = new Card
        {
            Id = "seeker-of-enlightenment", Type = CardType.Character, Controller = p1,
            PrintedMilitarySkill = 1, PrintedPoliticalSkill = 1, PersistentEffects = LoadPersistentEffects()
        };
        p1.PlayArea.Add(seeker);

        Assert.That(game.EffectiveMilitarySkill(seeker), Is.EqualTo(1));
    }
}
