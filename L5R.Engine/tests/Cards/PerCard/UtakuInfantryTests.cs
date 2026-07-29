using System.Text.Json;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class UtakuInfantryTests
{
    private static IReadOnlyList<PersistentEffectDefinition> LoadPersistentEffects()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "utaku-infantry.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParsePersistentEffects(document.RootElement);
    }

    [Test]
    public void WhileParticipatingAlone_GetsPlusOnePlusOne_CountingItself()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var infantry = new Card
        {
            Id = "utaku-infantry", Type = CardType.Character, Controller = p1, Faction = "unicorn",
            PrintedMilitarySkill = 0, PrintedPoliticalSkill = 0, PersistentEffects = LoadPersistentEffects()
        };
        p1.PlayArea.Add(infantry);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(infantry);
        game.CurrentConflict = conflict;

        Assert.That(game.EffectiveMilitarySkill(infantry), Is.EqualTo(1));
        Assert.That(game.EffectivePoliticalSkill(infantry), Is.EqualTo(1));
    }

    [Test]
    public void WithAnotherParticipatingUnicorn_ScalesUp()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var infantry = new Card
        {
            Id = "utaku-infantry", Type = CardType.Character, Controller = p1, Faction = "unicorn",
            PrintedMilitarySkill = 0, PrintedPoliticalSkill = 0, PersistentEffects = LoadPersistentEffects()
        };
        var ally = new Card { Id = "unicorn-ally", Type = CardType.Character, Controller = p1, Faction = "unicorn" };
        p1.PlayArea.Add(infantry);
        p1.PlayArea.Add(ally);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(infantry);
        conflict.Attackers.Add(ally);
        game.CurrentConflict = conflict;

        Assert.That(game.EffectiveMilitarySkill(infantry), Is.EqualTo(2));
    }

    [Test]
    public void WhileNotParticipating_NoBonus()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var infantry = new Card
        {
            Id = "utaku-infantry", Type = CardType.Character, Controller = p1, Faction = "unicorn",
            PrintedMilitarySkill = 0, PrintedPoliticalSkill = 0, PersistentEffects = LoadPersistentEffects()
        };
        p1.PlayArea.Add(infantry);

        Assert.That(game.EffectiveMilitarySkill(infantry), Is.EqualTo(0));
    }
}
