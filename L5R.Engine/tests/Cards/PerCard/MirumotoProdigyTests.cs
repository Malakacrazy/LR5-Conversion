using System.Text.Json;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class MirumotoProdigyTests
{
    private static IReadOnlyList<PersistentEffectDefinition> LoadPersistentEffects()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "mirumoto-prodigy.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParsePersistentEffects(document.RootElement);
    }

    [Test]
    public void WhileAttackingAlone_RestrictsTheDefendingPlayerToOneDefender()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var prodigy = new Card { Id = "mirumoto-prodigy", Type = CardType.Character, Controller = p1, PersistentEffects = LoadPersistentEffects() };
        p1.PlayArea.Add(prodigy);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(prodigy);
        game.CurrentConflict = conflict;

        Assert.That(game.MaxDefendersFor(prodigy), Is.EqualTo(1));
    }

    [Test]
    public void WhileAttackingAlongsideAnotherCharacter_DoesNotRestrictDefenders()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var prodigy = new Card { Id = "mirumoto-prodigy", Type = CardType.Character, Controller = p1, PersistentEffects = LoadPersistentEffects() };
        var ally = new Card { Id = "ally", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(prodigy);
        p1.PlayArea.Add(ally);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(prodigy);
        conflict.Attackers.Add(ally);
        game.CurrentConflict = conflict;

        Assert.That(game.MaxDefendersFor(prodigy), Is.EqualTo(int.MaxValue), "not the sole attacker, so the condition is not met");
    }

    [Test]
    public void WhileDefending_DoesNotRestrictDefenders()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var prodigy = new Card { Id = "mirumoto-prodigy", Type = CardType.Character, Controller = p1, PersistentEffects = LoadPersistentEffects() };
        p1.PlayArea.Add(prodigy);

        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1 };
        conflict.Defenders.Add(prodigy);
        game.CurrentConflict = conflict;

        Assert.That(game.MaxDefendersFor(prodigy), Is.EqualTo(int.MaxValue), "isAttacking is false while defending");
    }
}
