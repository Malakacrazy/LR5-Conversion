using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class LevyTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "levy.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseActions(document.RootElement).Single();
    }

    [Test]
    public void ChoosingFate_TakesOneFateFromTheOpponent()
    {
        var p1 = new Player { Name = "Player1", Fate = 2 };
        var p2 = new Player { Name = "Player2", Fate = 3 };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var source = new Card { Id = "levy", Type = CardType.Event, Controller = p1 };

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = source };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context, chosenChoice: "Give your opponent 1 fate");

        Assert.That(p1.Fate, Is.EqualTo(3));
        Assert.That(p2.Fate, Is.EqualTo(2));
    }

    [Test]
    public void ChoosingHonor_TakesOneHonorFromTheOpponent()
    {
        var p1 = new Player { Name = "Player1", Honor = 2 };
        var p2 = new Player { Name = "Player2", Honor = 3 };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var source = new Card { Id = "levy", Type = CardType.Event, Controller = p1 };

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = source };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context, chosenChoice: "Give your opponent 1 honor");

        Assert.That(p1.Honor, Is.EqualTo(3));
        Assert.That(p2.Honor, Is.EqualTo(2));
    }

    [Test]
    public void TakingFateFromAnOpponentWithNone_TransfersNothing()
    {
        var p1 = new Player { Name = "Player1", Fate = 2 };
        var p2 = new Player { Name = "Player2", Fate = 0 };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var source = new Card { Id = "levy", Type = CardType.Event, Controller = p1 };

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = source };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context, chosenChoice: "Give your opponent 1 fate");

        Assert.That(p1.Fate, Is.EqualTo(2), "fate transfer is balanced - can't take what the opponent doesn't have");
        Assert.That(p2.Fate, Is.EqualTo(0));
    }
}
