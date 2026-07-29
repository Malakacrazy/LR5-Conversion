using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class ShibaPeacemakerTests
{
    private static IReadOnlyList<PersistentEffectDefinition> LoadPersistentEffects()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "shiba-peacemaker.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParsePersistentEffects(document.RootElement);
    }

    [Test]
    public void CannotBeDeclaredAsAnAttacker_EvenWhileNotInPlay()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var peacemaker = new Card
        {
            Id = "shiba-peacemaker", Type = CardType.Character, Controller = p1, Location = "hand",
            PersistentEffects = LoadPersistentEffects()
        };
        p1.Hand.Add(peacemaker);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        game.CurrentConflict = conflict;

        Assert.That(game.IsRestrictedFrom(peacemaker, "declareAsAttacker"), Is.True,
            "sourceLocation 'any' means the restriction stays active even outside play area");
    }
}
