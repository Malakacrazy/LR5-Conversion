using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class FertileFieldsTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "fertile-fields.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseActions(document.RootElement).Single();
    }

    [Test]
    public void DrawsACard_WithNoCostOrTarget()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var province = new Card { Id = "fertile-fields", Type = CardType.Province, Controller = p1 };
        var deckCard = new Card { Id = "top-of-deck", Type = CardType.Character, Controller = p1 };
        p1.Deck.Add(deckCard);

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = province };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context);

        Assert.That(p1.Hand, Does.Contain(deckCard));
    }
}
