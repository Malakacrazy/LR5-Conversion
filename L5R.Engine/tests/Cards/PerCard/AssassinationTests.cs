using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class AssassinationTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "assassination.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseActions(document.RootElement).Single();
    }

    [Test]
    public void DuringConflict_PayingHonorDiscardsACheapCharacter()
    {
        var p1 = new Player { Name = "Player1", Honor = 5 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var source = new Card { Id = "assassination", Type = CardType.Event, Controller = p1 };
        var cheapCharacter = new Card { Id = "cheap-character", Type = CardType.Character, Controller = p2, PrintedCost = 2 };
        p2.PlayArea.Add(cheapCharacter);

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = source };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context, chosenTarget: cheapCharacter);

        Assert.That(p1.Honor, Is.EqualTo(2), "payHonor(3) cost was paid");
        Assert.That(p2.Discard, Does.Contain(cheapCharacter));
    }

    [Test]
    public void OutsideConflictPhase_CannotBeUsed()
    {
        var p1 = new Player { Name = "Player1", Honor = 5 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Dynasty };
        var source = new Card { Id = "assassination", Type = CardType.Event, Controller = p1 };
        var cheapCharacter = new Card { Id = "cheap-character", Type = CardType.Character, Controller = p2, PrintedCost = 2 };
        p2.PlayArea.Add(cheapCharacter);

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = source };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        Assert.Throws<InvalidOperationException>(() => executor.Execute(action, context, chosenTarget: cheapCharacter));
        Assert.That(p1.Honor, Is.EqualTo(5), "condition failed, so the cost was never paid");
    }
}
