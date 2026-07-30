using System.Text.Json;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class MotoYouthTests
{
    private static IReadOnlyList<PersistentEffectDefinition> LoadPersistentEffects()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "moto-youth.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParsePersistentEffects(document.RootElement);
    }

    [Test]
    public void DuringTheFirstMilitaryConflictOfTheRound_GetsPlusOneMilitarySkill()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var youth = new Card { Id = "moto-youth", Type = CardType.Character, Controller = p1, PrintedMilitarySkill = 2, PersistentEffects = LoadPersistentEffects() };
        p1.PlayArea.Add(youth);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, ConflictType = "military" };
        game.CurrentConflict = conflict;

        Assert.That(game.EffectiveMilitarySkill(youth), Is.EqualTo(3));
    }

    [Test]
    public void WhenAMilitaryConflictAlreadyCompletedThisRound_NoBonus()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var youth = new Card { Id = "moto-youth", Type = CardType.Character, Controller = p1, PrintedMilitarySkill = 2, PersistentEffects = LoadPersistentEffects() };
        p1.PlayArea.Add(youth);

        game.ConflictRecord.Add(new Conflict { AttackingPlayer = p2, DefendingPlayer = p1, ConflictType = "military", Winner = p2 });

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, ConflictType = "military" };
        game.CurrentConflict = conflict;

        Assert.That(game.EffectiveMilitarySkill(youth), Is.EqualTo(2));
    }

    [Test]
    public void DuringAPoliticalConflict_NoBonus()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var youth = new Card { Id = "moto-youth", Type = CardType.Character, Controller = p1, PrintedMilitarySkill = 2, PersistentEffects = LoadPersistentEffects() };
        p1.PlayArea.Add(youth);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, ConflictType = "political" };
        game.CurrentConflict = conflict;

        Assert.That(game.EffectiveMilitarySkill(youth), Is.EqualTo(2));
    }

    [Test]
    public void APastPoliticalConflictThisRound_DoesNotBlockTheBonus()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var youth = new Card { Id = "moto-youth", Type = CardType.Character, Controller = p1, PrintedMilitarySkill = 2, PersistentEffects = LoadPersistentEffects() };
        p1.PlayArea.Add(youth);

        game.ConflictRecord.Add(new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, ConflictType = "political", Winner = p1 });

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, ConflictType = "military" };
        game.CurrentConflict = conflict;

        Assert.That(game.EffectiveMilitarySkill(youth), Is.EqualTo(3));
    }
}
