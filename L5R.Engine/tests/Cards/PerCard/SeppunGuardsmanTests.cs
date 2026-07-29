using System.Text.Json;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class SeppunGuardsmanTests
{
    private static IReadOnlyList<PersistentEffectDefinition> LoadPersistentEffects()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "seppun-guardsman.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParsePersistentEffects(document.RootElement);
    }

    [Test]
    public void WhileTheOpponentHoldsImperialFavor_CannotBeDeclaredAsAnAttacker()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2", ImperialFavor = "political" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var guardsman = new Card
        {
            Id = "seppun-guardsman", Type = CardType.Character, Controller = p1,
            PersistentEffects = LoadPersistentEffects()
        };
        p1.PlayArea.Add(guardsman);

        Assert.That(game.IsRestrictedFrom(guardsman, "declareAsAttacker"), Is.True);
    }

    [Test]
    public void WhileNeitherPlayerHoldsImperialFavor_NoRestriction()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var guardsman = new Card
        {
            Id = "seppun-guardsman", Type = CardType.Character, Controller = p1,
            PersistentEffects = LoadPersistentEffects()
        };
        p1.PlayArea.Add(guardsman);

        Assert.That(game.IsRestrictedFrom(guardsman, "declareAsAttacker"), Is.False);
    }
}
