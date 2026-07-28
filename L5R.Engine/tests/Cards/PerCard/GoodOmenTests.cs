using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class GoodOmenTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "good-omen.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseActions(document.RootElement).Single();
    }

    private static GameState NewGame(out Player p1, out Player p2)
    {
        p1 = new Player { Name = "Player1" };
        p2 = new Player { Name = "Player2" };
        return new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
    }

    [Test]
    public void OnlyTargetsCharactersWithPrintedCostAboveTwo()
    {
        var game = NewGame(out var p1, out var p2);
        var goodOmen = new Card { Id = "good-omen", Type = CardType.Event, Controller = p1 };
        var cheapCharacter = new Card { Id = "cheap", Type = CardType.Character, Controller = p1, PrintedCost = 2 };
        var expensiveCharacter = new Card { Id = "expensive", Type = CardType.Character, Controller = p2, PrintedCost = 3 };
        p1.PlayArea.Add(cheapCharacter);
        p2.PlayArea.Add(expensiveCharacter);

        var action = LoadFirstAction();
        var legalTargets = TargetResolver.ResolveLegalTargets(
            action.Target!,
            new AbilityContext { Game = game, Player = p1, Source = goodOmen });

        Assert.That(legalTargets, Does.Not.Contain(cheapCharacter), "printedCost 2 is not > 2");
        Assert.That(legalTargets, Does.Contain(expensiveCharacter), "controller defaults to any, so either player's character qualifies");
    }

    [Test]
    public void PlacesOneFateOnTheChosenCharacter()
    {
        var game = NewGame(out var p1, out _);
        var goodOmen = new Card { Id = "good-omen", Type = CardType.Event, Controller = p1 };
        var target = new Card { Id = "expensive", Type = CardType.Character, Controller = p1, PrintedCost = 4, Fate = 0 };
        p1.PlayArea.Add(target);

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = goodOmen };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context, chosenTarget: target);

        Assert.That(target.Fate, Is.EqualTo(1));
    }
}
