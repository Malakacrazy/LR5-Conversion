using System.Text.Json;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class YogoOutcastTests
{
    private static IReadOnlyList<PersistentEffectDefinition> LoadPersistentEffects()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "yogo-outcast.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParsePersistentEffects(document.RootElement);
    }

    [Test]
    public void WhileItsControllerHasLessHonorThanTheOpponent_GetsPlusOnePlusOne()
    {
        var p1 = new Player { Name = "Player1", Honor = 3 };
        var p2 = new Player { Name = "Player2", Honor = 8 };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var outcast = new Card
        {
            Id = "yogo-outcast", Type = CardType.Character, Controller = p1,
            PrintedMilitarySkill = 2, PrintedPoliticalSkill = 2, PersistentEffects = LoadPersistentEffects()
        };
        p1.PlayArea.Add(outcast);

        Assert.That(game.EffectiveMilitarySkill(outcast), Is.EqualTo(3));
        Assert.That(game.EffectivePoliticalSkill(outcast), Is.EqualTo(3));
    }

    [Test]
    public void WhileItsControllerHasMoreHonorThanTheOpponent_NoBonus()
    {
        var p1 = new Player { Name = "Player1", Honor = 9 };
        var p2 = new Player { Name = "Player2", Honor = 2 };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var outcast = new Card
        {
            Id = "yogo-outcast", Type = CardType.Character, Controller = p1,
            PrintedMilitarySkill = 2, PrintedPoliticalSkill = 2, PersistentEffects = LoadPersistentEffects()
        };
        p1.PlayArea.Add(outcast);

        Assert.That(game.EffectiveMilitarySkill(outcast), Is.EqualTo(2));
    }
}
