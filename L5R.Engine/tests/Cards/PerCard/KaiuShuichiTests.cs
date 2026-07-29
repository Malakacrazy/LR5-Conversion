using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class KaiuShuichiTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "kaiu-shuichi.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseActions(document.RootElement).Single();
    }

    [Test]
    public void WhileParticipating_WithAnOwnHolding_GainsOneFate()
    {
        var p1 = new Player { Name = "Player1", Fate = 0 };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var shuichi = new Card { Id = "kaiu-shuichi", Type = CardType.Character, Controller = p1 };
        var holding = new Card { Id = "some-holding", Type = CardType.Holding, Controller = p1 };
        p1.PlayArea.Add(shuichi);
        p1.PlayArea.Add(holding);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(shuichi);
        game.CurrentConflict = conflict;

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = shuichi };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context);

        Assert.That(p1.Fate, Is.EqualTo(1));
    }

    [Test]
    public void CannotBeUsed_WhenNeitherPlayerControlsAHolding()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var shuichi = new Card { Id = "kaiu-shuichi", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(shuichi);

        var conflict = new Conflict { AttackingPlayer = p1, DefendingPlayer = p2 };
        conflict.Attackers.Add(shuichi);
        game.CurrentConflict = conflict;

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = shuichi };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        Assert.Throws<InvalidOperationException>(() => executor.Execute(action, context));
    }
}
