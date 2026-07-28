using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class FuneralPyreTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "funeral-pyre.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseActions(document.RootElement).Single();
    }

    [Test]
    public void SacrificingOwnCharacterDrawsACard()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var pyre = new Card { Id = "funeral-pyre", Type = CardType.Holding, Controller = p1 };
        var ownCharacter = new Card { Id = "own-character", Type = CardType.Character, Controller = p1 };
        var deckCard = new Card { Id = "top-of-deck", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(pyre);
        p1.PlayArea.Add(ownCharacter);
        p1.Deck.Add(deckCard);

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = pyre };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context, chosenCostTarget: ownCharacter);

        Assert.That(p1.PlayArea, Does.Not.Contain(ownCharacter), "sacrificed character leaves play");
        Assert.That(p1.Discard, Does.Contain(ownCharacter));
        Assert.That(p1.Hand, Does.Contain(deckCard), "sacrifice cost paid, so the draw gameAction runs");
    }

    [Test]
    public void CannotPayCost_WhenNoOwnCharacterInPlay_SincePrintedTextSaysFriendly()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var pyre = new Card { Id = "funeral-pyre", Type = CardType.Holding, Controller = p1 };
        var opponentCharacter = new Card { Id = "opponent-character", Type = CardType.Character, Controller = p2 };
        p1.PlayArea.Add(pyre);
        p2.PlayArea.Add(opponentCharacter);

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = pyre };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        // MetaActionCost.ts hardcodes controller: Players.Self for a sacrifice cost's own
        // target selection regardless of the cost's own params - an opponent's character
        // must never satisfy "sacrifice a character" even though a plain target with no
        // controller would default to Players.Any.
        Assert.Throws<InvalidOperationException>(() => executor.Execute(action, context));
    }

    [Test]
    public void CannotPayCost_WhenOwnCharacterIsOnlyInHand_NotInPlay()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Conflict };
        var pyre = new Card { Id = "funeral-pyre", Type = CardType.Holding, Controller = p1 };
        var characterInHand = new Card { Id = "character-in-hand", Type = CardType.Character, Controller = p1, Location = "hand" };
        p1.PlayArea.Add(pyre);
        p1.Hand.Add(characterInHand);

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = pyre };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        // GameActions.sacrifice() is DiscardFromPlayAction under the hood, whose canAffect
        // requires the card to already be in the play area - a same-type card sitting in
        // hand must not satisfy the cost.
        Assert.Throws<InvalidOperationException>(() => executor.Execute(action, context));
    }
}
