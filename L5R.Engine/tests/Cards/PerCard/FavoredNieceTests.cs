using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class FavoredNieceTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "favored-niece.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseActions(document.RootElement).Single();
    }

    [Test]
    public void DiscardingAHandCardDrawsAReplacement()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var niece = new Card { Id = "favored-niece", Type = CardType.Character, Controller = p1 };
        var handCard = new Card { Id = "some-hand-card", Type = CardType.Character, Controller = p1, Location = "hand" };
        var deckCard = new Card { Id = "top-of-deck", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(niece);
        p1.Hand.Add(handCard);
        p1.Deck.Add(deckCard);

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = niece };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context, chosenCostTarget: handCard);

        Assert.That(p1.Discard, Does.Contain(handCard));
        Assert.That(p1.Hand, Does.Contain(deckCard));
    }

    [Test]
    public void CannotPayCost_WhenHandIsEmpty()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var niece = new Card { Id = "favored-niece", Type = CardType.Character, Controller = p1 };
        var characterInPlay = new Card { Id = "not-a-hand-card", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(niece);
        p1.PlayArea.Add(characterInPlay);

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = niece };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        // discardCard's location default is "hand" - a card already in play must not count,
        // even though it would satisfy the (absent) cardType/cardCondition filters.
        Assert.Throws<InvalidOperationException>(() => executor.Execute(action, context));
    }
}
