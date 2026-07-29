using System.Text.Json;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class PoliticalRivalTests
{
    private static IReadOnlyList<PersistentEffectDefinition> LoadPersistentEffects()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "political-rival.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParsePersistentEffects(document.RootElement);
    }

    [Test]
    public void WhileDefending_GetsPlusThreePoliticalSkill()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var rival = new Card
        {
            Id = "political-rival", Type = CardType.Character, Controller = p1,
            PrintedPoliticalSkill = 3, PersistentEffects = LoadPersistentEffects()
        };
        p1.PlayArea.Add(rival);

        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1 };
        conflict.Defenders.Add(rival);
        game.CurrentConflict = conflict;

        Assert.That(game.EffectivePoliticalSkill(rival), Is.EqualTo(6));
    }

    [Test]
    public void WhileAttacking_NoBonus()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var rival = new Card
        {
            Id = "political-rival", Type = CardType.Character, Controller = p1,
            PrintedPoliticalSkill = 3, PersistentEffects = LoadPersistentEffects()
        };
        p1.PlayArea.Add(rival);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(rival);
        game.CurrentConflict = conflict;

        Assert.That(game.EffectivePoliticalSkill(rival), Is.EqualTo(3));
    }
}
