using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class CavalryReservesTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "cavalry-reserves.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseActions(document.RootElement).Single();
    }

    [Test]
    public void DuringAMilitaryConflict_PutsCavalryFromDiscardIntoPlayWithinBudget()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var source = new Card { Id = "cavalry-reserves", Type = CardType.Event, Controller = p1 };
        var cavalry1 = new Card { Id = "cavalry-1", Type = CardType.Character, Controller = p1, Traits = new List<string> { "cavalry" }, PrintedCost = 3, Location = "dynasty discard pile" };
        var cavalry2 = new Card { Id = "cavalry-2", Type = CardType.Character, Controller = p1, Traits = new List<string> { "cavalry" }, PrintedCost = 3, Location = "dynasty discard pile" };
        var cavalry3 = new Card { Id = "cavalry-3", Type = CardType.Character, Controller = p1, Traits = new List<string> { "cavalry" }, PrintedCost = 2, Location = "dynasty discard pile" };
        p1.Discard.Add(cavalry1);
        p1.Discard.Add(cavalry2);
        p1.Discard.Add(cavalry3);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, ConflictType = "military" };
        game.CurrentConflict = conflict;

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = source };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        // numCards is 0 (unlimited) - all three fit under the budget of 6? No: 3+3+2=8 > 6,
        // so only pick two that fit (3+3=6).
        executor.Execute(action, context, chosenTargets: new[] { cavalry1, cavalry2 });

        Assert.That(p1.PlayArea, Does.Contain(cavalry1));
        Assert.That(p1.PlayArea, Does.Contain(cavalry2));
        Assert.That(conflict.Attackers, Has.Count.EqualTo(2));
    }

    [Test]
    public void DuringAPoliticalConflict_ConditionIsNotMet()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var source = new Card { Id = "cavalry-reserves", Type = CardType.Event, Controller = p1 };

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2, ConflictType = "political" };
        game.CurrentConflict = conflict;

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = source };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        Assert.That(executor.IsConditionMet(action, context), Is.False);
    }
}
