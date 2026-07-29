using System.Text.Json;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class AsahinaStorytellerTests
{
    private static IReadOnlyList<PersistentEffectDefinition> LoadPersistentEffects()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "asahina-storyteller.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParsePersistentEffects(document.RootElement);
    }

    [Test]
    public void GivesEveryHonoredCraneCharacterTheSincerityKeyword()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var storyteller = new Card
        {
            Id = "asahina-storyteller", Type = CardType.Character, Controller = p1, Faction = "crane",
            PersistentEffects = LoadPersistentEffects()
        };
        var honoredCraneAlly = new Card { Id = "honored-crane-ally", Type = CardType.Character, Controller = p1, Faction = "crane", IsHonored = true };
        var unhonoredCraneAlly = new Card { Id = "unhonored-crane-ally", Type = CardType.Character, Controller = p1, Faction = "crane" };
        var honoredOtherFaction = new Card { Id = "honored-other-ally", Type = CardType.Character, Controller = p1, Faction = "crab", IsHonored = true };
        p1.PlayArea.Add(storyteller);
        p1.PlayArea.Add(honoredCraneAlly);
        p1.PlayArea.Add(unhonoredCraneAlly);
        p1.PlayArea.Add(honoredOtherFaction);

        Assert.That(game.HasKeyword(honoredCraneAlly, "sincerity"), Is.True);
        Assert.That(game.HasKeyword(unhonoredCraneAlly, "sincerity"), Is.False, "must be honored");
        Assert.That(game.HasKeyword(honoredOtherFaction, "sincerity"), Is.False, "must be Crane");
    }

    [Test]
    public void ItAppliesToItselfToo_IfItBecomesHonored()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var storyteller = new Card
        {
            Id = "asahina-storyteller", Type = CardType.Character, Controller = p1, Faction = "crane", IsHonored = true,
            PersistentEffects = LoadPersistentEffects()
        };
        p1.PlayArea.Add(storyteller);

        Assert.That(game.HasKeyword(storyteller, "sincerity"), Is.True, "match has no isSelf exclusion, unlike honored-general's military bonus");
    }
}
