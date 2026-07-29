using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class ICanSwimTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "i-can-swim.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseActions(document.RootElement).Single();
    }

    [Test]
    public void WithAHigherBid_DiscardsAParticipatingDishonoredEnemy()
    {
        var p1 = new Player { Name = "Player1", ShowBid = 5 };
        var p2 = new Player { Name = "Player2", ShowBid = 2 };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var source = new Card { Id = "i-can-swim", Type = CardType.Event, Controller = p1 };
        var target = new Card { Id = "dishonored-enemy", Type = CardType.Character, Controller = p2, IsDishonored = true };
        p2.PlayArea.Add(target);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Defenders.Add(target);
        game.CurrentConflict = conflict;

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = source };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context, chosenTarget: target);

        Assert.That(p2.Discard, Does.Contain(target));
    }

    [Test]
    public void WithALowerBid_ConditionIsNotMet()
    {
        var p1 = new Player { Name = "Player1", ShowBid = 1 };
        var p2 = new Player { Name = "Player2", ShowBid = 4 };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var source = new Card { Id = "i-can-swim", Type = CardType.Event, Controller = p1 };

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = source };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        Assert.That(executor.IsConditionMet(action, context), Is.False);
    }
}
