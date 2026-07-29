using System.Text.Json;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class IshikenInitiateTests
{
    private static IReadOnlyList<PersistentEffectDefinition> LoadPersistentEffects()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "ishiken-initiate.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParsePersistentEffects(document.RootElement);
    }

    [Test]
    public void GetsBothSkillsIncreasedByTheNumberOfRingsItsControllerHasClaimed()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        game.Rings[0].Claimed = true;
        game.Rings[0].ClaimedBy = p1;
        game.Rings[1].Claimed = true;
        game.Rings[1].ClaimedBy = p1;
        var initiate = new Card
        {
            Id = "ishiken-initiate", Type = CardType.Character, Controller = p1,
            PrintedMilitarySkill = 1, PrintedPoliticalSkill = 1, PersistentEffects = LoadPersistentEffects()
        };
        p1.PlayArea.Add(initiate);

        Assert.That(game.EffectiveMilitarySkill(initiate), Is.EqualTo(3));
        Assert.That(game.EffectivePoliticalSkill(initiate), Is.EqualTo(3));
    }

    [Test]
    public void WithNoClaimedRings_NoBonus()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var initiate = new Card
        {
            Id = "ishiken-initiate", Type = CardType.Character, Controller = p1,
            PrintedMilitarySkill = 1, PrintedPoliticalSkill = 1, PersistentEffects = LoadPersistentEffects()
        };
        p1.PlayArea.Add(initiate);

        Assert.That(game.EffectiveMilitarySkill(initiate), Is.EqualTo(1));
    }
}
