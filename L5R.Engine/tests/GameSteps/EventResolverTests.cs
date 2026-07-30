using System.Text.Json;
using L5R.Engine.Cards;
using L5R.Engine.GameSteps;
using L5R.Engine.State;

namespace L5R.Engine.Tests.GameSteps;

public class EventResolverTests
{
    private static readonly string CardsDir = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core");

    private static JsonElement LoadJson(string cardId)
    {
        var path = Path.Combine(CardsDir, $"{cardId}.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return document.RootElement.Clone();
    }

    [Test]
    public void ResolvesItsBridgedActionAgainstALegalTarget_ThenDiscardsItself()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };

        var goodOmen = CardFactory.BuildCard(LoadJson("good-omen"), p1);
        p1.PlayArea.Add(goodOmen); // simulates having just been moved here by PlayCardGameActionHandler

        var target = new Card { Id = "expensive", Type = CardType.Character, Controller = p1, PrintedCost = 4, Fate = 0 };
        p1.PlayArea.Add(target);

        EventResolver.ResolveAndDiscard(game, goodOmen, p1);

        Assert.That(target.Fate, Is.EqualTo(1));
        Assert.That(p1.Discard, Contains.Item(goodOmen));
        Assert.That(p1.PlayArea, Does.Not.Contain(goodOmen));
        Assert.That(goodOmen.Location, Is.EqualTo("discard"));
    }

    [Test]
    public void WithNoLegalTarget_StillDiscardsItselfWithoutThrowing()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };

        var goodOmen = CardFactory.BuildCard(LoadJson("good-omen"), p1);
        p1.PlayArea.Add(goodOmen);

        var cheapCharacter = new Card { Id = "cheap", Type = CardType.Character, Controller = p1, PrintedCost = 1, Fate = 0 };
        p1.PlayArea.Add(cheapCharacter);

        Assert.DoesNotThrow(() => EventResolver.ResolveAndDiscard(game, goodOmen, p1));

        Assert.That(cheapCharacter.Fate, Is.EqualTo(0), "no legal target existed (printedCost 1 is not > 2)");
        Assert.That(p1.Discard, Contains.Item(goodOmen));
    }

    [Test]
    public void WithNoBridgedActionAtAll_JustDiscardsItself()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var vanillaEvent = new Card { Id = "some-vanilla-event", Type = CardType.Event, Controller = p1 };
        p1.PlayArea.Add(vanillaEvent);

        Assert.DoesNotThrow(() => EventResolver.ResolveAndDiscard(game, vanillaEvent, p1));

        Assert.That(p1.Discard, Contains.Item(vanillaEvent));
    }
}
