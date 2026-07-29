using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class CityOfTheOpenHandTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "city-of-the-open-hand.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseActions(document.RootElement).Single();
    }

    [Test]
    public void WithLessHonorThanTheOpponent_CanChooseToGainOneHonor()
    {
        var p1 = new Player { Name = "Player1", Honor = 3 };
        var p2 = new Player { Name = "Player2", Honor = 8 };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var stronghold = new Card { Id = "city-of-the-open-hand", Type = CardType.Stronghold, Controller = p1 };

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = stronghold };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context, chosenChoice: "Gain 1 honor");

        Assert.That(stronghold.Bowed, Is.True, "bowSelf cost was paid");
        Assert.That(p1.Honor, Is.EqualTo(4));
        Assert.That(p2.Honor, Is.EqualTo(8));
    }

    [Test]
    public void WithLessHonorThanTheOpponent_CanChooseToMakeTheOpponentLoseOneHonor()
    {
        var p1 = new Player { Name = "Player1", Honor = 3 };
        var p2 = new Player { Name = "Player2", Honor = 8 };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var stronghold = new Card { Id = "city-of-the-open-hand", Type = CardType.Stronghold, Controller = p1 };

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = stronghold };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context, chosenChoice: "Make opponent lose 1 honor");

        Assert.That(p1.Honor, Is.EqualTo(3));
        Assert.That(p2.Honor, Is.EqualTo(7));
    }

    [Test]
    public void WithMoreHonorThanTheOpponent_ConditionIsNotMet()
    {
        var p1 = new Player { Name = "Player1", Honor = 9 };
        var p2 = new Player { Name = "Player2", Honor = 2 };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var stronghold = new Card { Id = "city-of-the-open-hand", Type = CardType.Stronghold, Controller = p1 };

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = stronghold };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        Assert.That(executor.IsConditionMet(action, context), Is.False);
    }
}
