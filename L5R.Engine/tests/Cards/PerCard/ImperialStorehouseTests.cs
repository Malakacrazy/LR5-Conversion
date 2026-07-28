using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class ImperialStorehouseTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "imperial-storehouse.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseActions(document.RootElement).Single();
    }

    [Test]
    public void SacrificesItselfToDrawACard()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var imperialStorehouse = new Card { Id = "imperial-storehouse", Type = CardType.Holding, Controller = p1 };
        var deckCard = new Card { Id = "deck-card", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(imperialStorehouse);
        p1.Deck.Add(deckCard);

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = imperialStorehouse };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context);

        Assert.That(p1.PlayArea, Does.Not.Contain(imperialStorehouse), "sacrificed as the cost");
        Assert.That(p1.Discard, Does.Contain(imperialStorehouse));
        Assert.That(p1.Deck, Does.Not.Contain(deckCard));
        Assert.That(p1.Hand, Does.Contain(deckCard), "draw moves the top card of the deck to hand");
    }

    [Test]
    public void DrawingWithAnEmptyDeckIsAHarmlessNoOp()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var imperialStorehouse = new Card { Id = "imperial-storehouse", Type = CardType.Holding, Controller = p1 };
        p1.PlayArea.Add(imperialStorehouse);

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = imperialStorehouse };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        Assert.DoesNotThrow(() => executor.Execute(action, context));
        Assert.That(p1.Hand, Is.Empty);
    }
}
