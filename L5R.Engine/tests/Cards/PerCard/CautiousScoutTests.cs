using System.Text.Json;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class CautiousScoutTests
{
    private static IReadOnlyList<PersistentEffectDefinition> LoadPersistentEffects()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "cautious-scout.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParsePersistentEffects(document.RootElement);
    }

    [Test]
    public void WhileAttackingAlone_BlanksTheConflictProvince()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var scout = new Card { Id = "cautious-scout", Type = CardType.Character, Controller = p1, PersistentEffects = LoadPersistentEffects() };
        var province = new Card { Id = "opponent-province", Type = CardType.Province, Controller = p2 };
        p1.PlayArea.Add(scout);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, DeclaredProvince = province };
        conflict.Attackers.Add(scout);
        game.CurrentConflict = conflict;

        Assert.That(game.IsBlanked(province), Is.True);
    }

    [Test]
    public void WhileAttackingAlongsideAnotherCharacter_DoesNotBlankTheProvince()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var scout = new Card { Id = "cautious-scout", Type = CardType.Character, Controller = p1, PersistentEffects = LoadPersistentEffects() };
        var ally = new Card { Id = "ally", Type = CardType.Character, Controller = p1 };
        var province = new Card { Id = "opponent-province", Type = CardType.Province, Controller = p2 };
        p1.PlayArea.Add(scout);
        p1.PlayArea.Add(ally);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, DeclaredProvince = province };
        conflict.Attackers.Add(scout);
        conflict.Attackers.Add(ally);
        game.CurrentConflict = conflict;

        Assert.That(game.IsBlanked(province), Is.False, "not the sole attacker, so the condition is not met");
    }

    [Test]
    public void ADifferentProvince_IsNotBlanked()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var scout = new Card { Id = "cautious-scout", Type = CardType.Character, Controller = p1, PersistentEffects = LoadPersistentEffects() };
        var declaredProvince = new Card { Id = "opponent-province", Type = CardType.Province, Controller = p2 };
        var otherProvince = new Card { Id = "other-province", Type = CardType.Province, Controller = p2 };
        p1.PlayArea.Add(scout);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, DeclaredProvince = declaredProvince };
        conflict.Attackers.Add(scout);
        game.CurrentConflict = conflict;

        Assert.That(game.IsBlanked(otherProvince), Is.False);
    }
}
