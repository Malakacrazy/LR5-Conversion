using System.Text.Json;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class FearsomeMysticTests
{
    private static IReadOnlyList<PersistentEffectDefinition> LoadPersistentEffects()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "fearsome-mystic.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParsePersistentEffects(document.RootElement);
    }

    [Test]
    public void DuringAnAirConflict_GetsPlusTwoGlory()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var mystic = new Card
        {
            Id = "fearsome-mystic", Type = CardType.Character, Controller = p1,
            PrintedGlory = 1, PersistentEffects = LoadPersistentEffects()
        };
        p1.PlayArea.Add(mystic);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, Elements = new List<string> { "air" } };
        game.CurrentConflict = conflict;

        Assert.That(game.EffectiveGlory(mystic), Is.EqualTo(3));
    }

    [Test]
    public void DuringANonAirConflict_NoBonus()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var mystic = new Card
        {
            Id = "fearsome-mystic", Type = CardType.Character, Controller = p1,
            PrintedGlory = 1, PersistentEffects = LoadPersistentEffects()
        };
        p1.PlayArea.Add(mystic);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, ConflictType = "military" };
        game.CurrentConflict = conflict;

        Assert.That(game.EffectiveGlory(mystic), Is.EqualTo(1));
    }
}
