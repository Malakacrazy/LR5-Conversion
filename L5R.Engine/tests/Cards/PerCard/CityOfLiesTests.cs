using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class CityOfLiesTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "city-of-lies.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseActions(document.RootElement).Single();
    }

    [Test]
    public void ReducesTheCostOfTheNextEventByOne()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var cityOfLies = new Card { Id = "city-of-lies", Type = CardType.Holding, Controller = p1 };

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = cityOfLies };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context);

        var someEvent = new Card { Id = "some-event", Type = CardType.Event, Controller = p1, PrintedCost = 2 };
        var someCharacter = new Card { Id = "some-character", Type = CardType.Character, Controller = p1, PrintedCost = 2 };

        Assert.That(game.EffectiveCost(someEvent, p1), Is.EqualTo(1));
        Assert.That(game.EffectiveCost(someCharacter, p1), Is.EqualTo(2), "appliesTo: isType event doesn't cover characters");
        Assert.That(game.EffectiveCost(someEvent, p2), Is.EqualTo(2), "the reduction is scoped to the player who used city-of-lies");
    }
}
