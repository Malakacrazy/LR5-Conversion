using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class AmbushTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "ambush.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseActions(document.RootElement).Single();
    }

    [Test]
    public void DuringConflict_PutsUpToTwoScorpionCharactersIntoPlayWithinBudget()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var source = new Card { Id = "ambush", Type = CardType.Event, Controller = p1 };
        var handCharacter = new Card { Id = "scorpion-hand", Type = CardType.Character, Controller = p1, Faction = "scorpion", PrintedCost = 3, Location = "hand" };
        var provinceCharacter = new Card { Id = "scorpion-province", Type = CardType.Character, Controller = p1, Faction = "scorpion", PrintedCost = 3, Location = "province" };
        p1.Hand.Add(handCharacter);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        game.CurrentConflict = conflict;

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = source };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context, chosenTargets: new[] { handCharacter, provinceCharacter });

        Assert.That(p1.PlayArea, Does.Contain(handCharacter));
        Assert.That(p1.PlayArea, Does.Contain(provinceCharacter));
        Assert.That(conflict.Attackers, Has.Count.EqualTo(2));
    }

    [Test]
    public void ExceedingTheBudget_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var source = new Card { Id = "ambush", Type = CardType.Event, Controller = p1 };
        var expensive1 = new Card { Id = "scorpion-1", Type = CardType.Character, Controller = p1, Faction = "scorpion", PrintedCost = 4, Location = "hand" };
        var expensive2 = new Card { Id = "scorpion-2", Type = CardType.Character, Controller = p1, Faction = "scorpion", PrintedCost = 4, Location = "hand" };
        p1.Hand.Add(expensive1);
        p1.Hand.Add(expensive2);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        game.CurrentConflict = conflict;

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = source };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        Assert.Throws<InvalidOperationException>(
            () => executor.Execute(action, context, chosenTargets: new[] { expensive1, expensive2 }));
    }

    [Test]
    public void ChoosingANonScorpionCharacter_Throws()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var source = new Card { Id = "ambush", Type = CardType.Event, Controller = p1 };
        var crab = new Card { Id = "crab-character", Type = CardType.Character, Controller = p1, Faction = "crab", PrintedCost = 2, Location = "hand" };
        p1.Hand.Add(crab);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        game.CurrentConflict = conflict;

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = source };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        Assert.Throws<InvalidOperationException>(
            () => executor.Execute(action, context, chosenTargets: new[] { crab }));
    }
}
