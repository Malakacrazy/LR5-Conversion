using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class BlackmailTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "blackmail.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseActions(document.RootElement).Single();
    }

    [Test]
    public void DuringConflict_TakesControlOfACheapOpponentCharacterUntilTheConflictEnds()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var source = new Card { Id = "blackmail", Type = CardType.Event, Controller = p1 };
        var stolenCharacter = new Card { Id = "cheap-enemy", Type = CardType.Character, Controller = p2, PrintedCost = 2 };
        p2.PlayArea.Add(stolenCharacter);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        game.CurrentConflict = conflict;

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = source };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context, chosenTarget: stolenCharacter);

        Assert.That(stolenCharacter.Controller, Is.EqualTo(p1));
        Assert.That(p1.PlayArea, Does.Contain(stolenCharacter));
        Assert.That(p2.PlayArea, Does.Not.Contain(stolenCharacter));

        game.EndConflict();

        Assert.That(stolenCharacter.Controller, Is.EqualTo(p2), "control reverts once the conflict that granted it ends");
        Assert.That(p2.PlayArea, Does.Contain(stolenCharacter));
        Assert.That(p1.PlayArea, Does.Not.Contain(stolenCharacter));
    }

    [Test]
    public void CannotTargetAnExpensiveCharacter()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var expensive = new Card { Id = "expensive-enemy", Type = CardType.Character, Controller = p2, PrintedCost = 4 };

        var action = LoadFirstAction();
        var legalTargets = TargetResolver.ResolveLegalTargets(
            action.Target!,
            new AbilityContext { Game = game, Player = p1, Source = new Card { Id = "blackmail", Type = CardType.Event, Controller = p1 } });

        Assert.That(legalTargets, Does.Not.Contain(expensive));
    }
}
