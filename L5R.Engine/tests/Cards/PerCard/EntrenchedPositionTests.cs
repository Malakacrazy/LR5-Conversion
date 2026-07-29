using System.Text.Json;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class EntrenchedPositionTests
{
    private static IReadOnlyList<PersistentEffectDefinition> LoadPersistentEffects()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "entrenched-position.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParsePersistentEffects(document.RootElement);
    }

    [Test]
    public void DuringAMilitaryConflict_GetsPlusFiveStrength()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var province = new Card
        {
            Id = "entrenched-position", Type = CardType.Province, Controller = p1,
            PrintedProvinceStrength = 5, PersistentEffects = LoadPersistentEffects()
        };
        p1.PlayArea.Add(province);

        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1, ConflictType = "military" };
        game.CurrentConflict = conflict;

        Assert.That(game.EffectiveProvinceStrength(province), Is.EqualTo(10));
    }

    [Test]
    public void DuringAPoliticalConflict_NoBonus()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var province = new Card
        {
            Id = "entrenched-position", Type = CardType.Province, Controller = p1,
            PrintedProvinceStrength = 5, PersistentEffects = LoadPersistentEffects()
        };
        p1.PlayArea.Add(province);

        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1, ConflictType = "political" };
        game.CurrentConflict = conflict;

        Assert.That(game.EffectiveProvinceStrength(province), Is.EqualTo(5));
    }
}
