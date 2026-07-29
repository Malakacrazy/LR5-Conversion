using System.Text.Json;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class OtomoCourtierTests
{
    private static IReadOnlyList<PersistentEffectDefinition> LoadPersistentEffects()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "otomo-courtier.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParsePersistentEffects(document.RootElement);
    }

    [Test]
    public void WhileTheOpponentHoldsImperialFavor_CannotBeDeclaredAsAnAttacker()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2", ImperialFavor = "military" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var courtier = new Card
        {
            Id = "otomo-courtier", Type = CardType.Character, Controller = p1, Location = "hand",
            PersistentEffects = LoadPersistentEffects()
        };
        p1.Hand.Add(courtier);

        Assert.That(game.IsRestrictedFrom(courtier, "declareAsAttacker"), Is.True,
            "sourceLocation 'any' means the restriction stays active even outside play area");
    }

    [Test]
    public void WhileNeitherPlayerHoldsImperialFavor_NoRestriction()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var courtier = new Card
        {
            Id = "otomo-courtier", Type = CardType.Character, Controller = p1,
            PersistentEffects = LoadPersistentEffects()
        };
        p1.PlayArea.Add(courtier);

        Assert.That(game.IsRestrictedFrom(courtier, "declareAsAttacker"), Is.False);
    }

    [Test]
    public void WhileItsOwnControllerHoldsImperialFavor_NoRestriction()
    {
        var p1 = new Player { Name = "Player1", ImperialFavor = "political" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var courtier = new Card
        {
            Id = "otomo-courtier", Type = CardType.Character, Controller = p1,
            PersistentEffects = LoadPersistentEffects()
        };
        p1.PlayArea.Add(courtier);

        Assert.That(game.IsRestrictedFrom(courtier, "declareAsAttacker"), Is.False,
            "the condition checks the opponent's favor, not this character's own controller's");
    }
}
