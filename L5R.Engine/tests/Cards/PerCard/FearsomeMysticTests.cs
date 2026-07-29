using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
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

    [Test]
    public void RemovesFateFromParticipatingOpponentsWithLowerGlory()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var mystic = new Card { Id = "fearsome-mystic", Type = CardType.Character, Controller = p1, PrintedGlory = 3 };
        var lowerGlory = new Card { Id = "lower-glory", Type = CardType.Character, Controller = p2, PrintedGlory = 1, Fate = 2 };
        var higherGlory = new Card { Id = "higher-glory", Type = CardType.Character, Controller = p2, PrintedGlory = 4, Fate = 2 };
        p1.PlayArea.Add(mystic);
        p2.PlayArea.Add(lowerGlory);
        p2.PlayArea.Add(higherGlory);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(mystic);
        conflict.Defenders.Add(lowerGlory);
        conflict.Defenders.Add(higherGlory);
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = mystic };

        new FearsomeMysticRemoveFateFromLowerGloryOpponents().Execute(context);

        Assert.That(lowerGlory.Fate, Is.EqualTo(1));
        Assert.That(higherGlory.Fate, Is.EqualTo(2), "not lower glory than the mystic");
    }

    [Test]
    public void WhenNotParticipating_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var mystic = new Card { Id = "fearsome-mystic", Type = CardType.Character, Controller = p1, PrintedGlory = 3 };
        p1.PlayArea.Add(mystic);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = mystic };

        Assert.Throws<InvalidOperationException>(() => new FearsomeMysticRemoveFateFromLowerGloryOpponents().Execute(context));
    }
}
