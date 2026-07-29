using System.Text.Json;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class SeekerOfKnowledgeTests
{
    private static IReadOnlyList<PersistentEffectDefinition> LoadPersistentEffects()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "seeker-of-knowledge.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParsePersistentEffects(document.RootElement);
    }

    [Test]
    public void WhileAttacking_TheConflictCountsAsAnAirConflictToo()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var seeker = new Card { Id = "seeker-of-knowledge", Type = CardType.Character, Controller = p1, PersistentEffects = LoadPersistentEffects() };
        p1.PlayArea.Add(seeker);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, ConflictType = "military", Elements = new List<string> { "fire" } };
        conflict.Attackers.Add(seeker);
        game.CurrentConflict = conflict;

        Assert.That(game.ConflictHasType("air"), Is.True, "seeker-of-knowledge's addElementAsAttacker contributes air even though the ring itself is fire/military");
        Assert.That(game.ConflictHasType("water"), Is.False);
    }

    [Test]
    public void WhileDefending_DoesNotContributeTheElement()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var seeker = new Card { Id = "seeker-of-knowledge", Type = CardType.Character, Controller = p1, PersistentEffects = LoadPersistentEffects() };
        p1.PlayArea.Add(seeker);

        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1, ConflictType = "military", Elements = new List<string> { "fire" } };
        conflict.Defenders.Add(seeker);
        game.CurrentConflict = conflict;

        Assert.That(game.ConflictHasType("air"), Is.False, "addElementAsAttacker only contributes while attacking - ringteki's getElements() only concats currentConflict.attackers");
    }
}
