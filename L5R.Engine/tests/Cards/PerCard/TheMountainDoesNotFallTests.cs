using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class TheMountainDoesNotFallTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "the-mountain-does-not-fall.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseActions(document.RootElement).Single();
    }

    [Test]
    public void WhileDefending_TheChosenCharacterCannotBeBowed()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var source = new Card { Id = "the-mountain-does-not-fall", Type = CardType.Event, Controller = p1 };
        var defender = new Card { Id = "defender", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(defender);

        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1 };
        conflict.Defenders.Add(defender);
        game.CurrentConflict = conflict;

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = source };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context, chosenTarget: defender);

        Assert.Throws<InvalidOperationException>(() => new BowGameActionHandler().Execute(new AbilityContext { Game = game, Player = p1, Source = source, Target = defender }, null));
        Assert.That(defender.Bowed, Is.False);
    }

    [Test]
    public void OnceItStopsDefending_ItCanBeBowedAgain()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var source = new Card { Id = "the-mountain-does-not-fall", Type = CardType.Event, Controller = p1 };
        var defender = new Card { Id = "defender", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(defender);

        var conflict = new Conflict { AttackingPlayer = p2, DefendingPlayer = p1 };
        conflict.Defenders.Add(defender);
        game.CurrentConflict = conflict;

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = source };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context, chosenTarget: defender);

        // The condition is re-checked live, not just at the moment the effect was applied.
        conflict.Defenders.Remove(defender);

        new BowGameActionHandler().Execute(new AbilityContext { Game = game, Player = p1, Source = source, Target = defender }, null);

        Assert.That(defender.Bowed, Is.True);
    }
}
