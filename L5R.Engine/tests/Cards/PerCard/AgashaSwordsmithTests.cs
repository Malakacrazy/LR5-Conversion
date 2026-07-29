using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Tests.Cards.PerCard;

public class AgashaSwordsmithTests
{
    private static ActionDefinition LoadFirstAction()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core", "agasha-swordsmith.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return AbilityDefinitionParser.ParseActions(document.RootElement).Single();
    }

    private static Player NewPlayerWithDeck(out Card attachment)
    {
        var p1 = new Player { Name = "Player1" };
        attachment = new Card { Id = "found-attachment", Type = CardType.Attachment, Controller = p1 };
        p1.Deck.Add(new Card { Id = "top-character", Type = CardType.Character, Controller = p1 });
        p1.Deck.Add(attachment);
        for (var i = 0; i < 3; i++)
            p1.Deck.Add(new Card { Id = $"filler-{i}", Type = CardType.Character, Controller = p1 });
        return p1;
    }

    [Test]
    public void TakingAMatchingAttachmentFromTheTopFive_MovesItToHand()
    {
        var p1 = NewPlayerWithDeck(out var attachment);
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Dynasty };
        var swordsmith = new Card { Id = "agasha-swordsmith", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(swordsmith);

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = swordsmith, ChosenDeckSearchCard = attachment };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context);

        Assert.That(p1.Hand, Does.Contain(attachment));
        Assert.That(p1.Deck, Does.Not.Contain(attachment));
    }

    [Test]
    public void CannotTakeACardOutsideTheTopFive()
    {
        var p1 = NewPlayerWithDeck(out _);
        var beyondTop5 = new Card { Id = "beyond-top-5", Type = CardType.Attachment, Controller = p1 };
        p1.Deck.Add(beyondTop5);
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Dynasty };
        var swordsmith = new Card { Id = "agasha-swordsmith", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(swordsmith);

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = swordsmith, ChosenDeckSearchCard = beyondTop5 };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        Assert.Throws<InvalidOperationException>(() => executor.Execute(action, context));
    }

    [Test]
    public void CannotTakeANonAttachment()
    {
        var p1 = NewPlayerWithDeck(out _);
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Dynasty };
        var swordsmith = new Card { Id = "agasha-swordsmith", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(swordsmith);

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = swordsmith, ChosenDeckSearchCard = p1.Deck[0] };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        Assert.Throws<InvalidOperationException>(() => executor.Execute(action, context));
    }

    [Test]
    public void TakingNothingIsLegal()
    {
        var p1 = NewPlayerWithDeck(out _);
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1, CurrentPhase = Phase.Dynasty };
        var swordsmith = new Card { Id = "agasha-swordsmith", Type = CardType.Character, Controller = p1 };
        p1.PlayArea.Add(swordsmith);
        var deckBefore = p1.Deck.Count;

        var action = LoadFirstAction();
        var context = new AbilityContext { Game = game, Player = p1, Source = swordsmith };
        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());

        executor.Execute(action, context);

        Assert.That(p1.Deck.Count, Is.EqualTo(deckBefore));
        Assert.That(p1.Hand, Is.Empty);
    }
}
