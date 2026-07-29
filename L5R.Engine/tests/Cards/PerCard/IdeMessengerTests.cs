using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class IdeMessengerTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "ide-messenger.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseActions(document.RootElement).Single();
    }

    [Test]
    public void PayingOneFate_MovesAnAllyIntoTheConflict()
    {
        var p1 = new Player { Name = "Player1", Fate = 1 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var messenger = new Card { Id = "ide-messenger", Type = CardType.Character, Controller = p1 };
        var ally = new Card { Id = "ally-1", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(messenger);
        p1.PlayArea.Add(ally);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        game.CurrentConflict = conflict;

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = messenger };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context, chosenTarget: ally);

        Assert.That(p1.Fate, Is.EqualTo(0), "payFate(1) cost was paid");
        Assert.That(conflict.Attackers, Does.Contain(ally));
    }

    [Test]
    public void WithNoFate_CannotPayTheCost()
    {
        var p1 = new Player { Name = "Player1", Fate = 0 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var messenger = new Card { Id = "ide-messenger", Type = CardType.Character, Controller = p1 };
        var ally = new Card { Id = "ally-1", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(messenger);
        p1.PlayArea.Add(ally);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        game.CurrentConflict = conflict;

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = messenger };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        Assert.Throws<InvalidOperationException>(() => executor.Execute(action, context, chosenTarget: ally));
    }
}
