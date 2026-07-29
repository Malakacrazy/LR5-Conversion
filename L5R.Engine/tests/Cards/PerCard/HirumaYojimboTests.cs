using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class HirumaYojimboTests
{
    private static IReadOnlyList<PersistentEffectDefinition> LoadPersistentEffects()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "hiruma-yojimbo.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParsePersistentEffects(document.RootElement);
    }

    [Test]
    public void CannotBeMovedIntoAConflictAsAnAttacker()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var yojimbo = new Card
        {
            Id = "hiruma-yojimbo", Type = CardType.Character, Controller = p1, PersistentEffects = LoadPersistentEffects()
        };
        p1.PlayArea.Add(yojimbo);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = yojimbo, Target = yojimbo };
        var handler = new MoveToConflictGameActionHandler();

        Assert.Throws<InvalidOperationException>(() => handler.Execute(context, parameters: null));
        Assert.That(conflict.Attackers, Does.Not.Contain(yojimbo));
    }

    [Test]
    public void CanStillBeMovedIntoAConflictAsADefender()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var yojimbo = new Card
        {
            Id = "hiruma-yojimbo", Type = CardType.Character, Controller = p1, PersistentEffects = LoadPersistentEffects()
        };
        p1.PlayArea.Add(yojimbo);

        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1 };
        game.CurrentConflict = conflict;

        var context = new AbilityContext { Game = game, Player = p1, Source = yojimbo, Target = yojimbo };
        new MoveToConflictGameActionHandler().Execute(context, parameters: null);

        Assert.That(conflict.Defenders, Does.Contain(yojimbo));
    }
}
