using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class AdmitDefeatTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "admit-defeat.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseActions(document.RootElement).Single();
    }

    [Test]
    public void WithExactlyOneDefender_BowsIt()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var source = new Card { Id = "admit-defeat", Type = CardType.Event, Controller = p1 };
        var defender = new Card { Id = "the-defender", Type = CardType.Character, Controller = p2 };
        p2.PlayArea.Add(defender);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Defenders.Add(defender);
        game.CurrentConflict = conflict;

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = source };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context, chosenTarget: defender);

        Assert.That(defender.Bowed, Is.True);
    }

    [Test]
    public void CannotBeUsed_WithTwoDefenders()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var source = new Card { Id = "admit-defeat", Type = CardType.Event, Controller = p1 };
        var defender1 = new Card { Id = "defender-1", Type = CardType.Character, Controller = p2 };
        var defender2 = new Card { Id = "defender-2", Type = CardType.Character, Controller = p2 };
        p2.PlayArea.Add(defender1);
        p2.PlayArea.Add(defender2);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Defenders.Add(defender1);
        conflict.Defenders.Add(defender2);
        game.CurrentConflict = conflict;

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = source };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        Assert.Throws<InvalidOperationException>(() => executor.Execute(action, context, chosenTarget: defender1));
    }
}
