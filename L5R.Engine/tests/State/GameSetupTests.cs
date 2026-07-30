using System.Text.Json;
using L5R.Engine.Randomness;
using L5R.Engine.State;

namespace L5R.Engine.Tests.State;

public class GameSetupTests
{
    private static readonly string CardsDir = Path.Combine(AppContext.BaseDirectory, "Cards", "01-Core");

    private static JsonElement LoadJson(string cardId)
    {
        var path = Path.Combine(CardsDir, $"{cardId}.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return document.RootElement.Clone();
    }

    private static DeckList BuildDeckList() => new(
        Stronghold: LoadJson("city-of-the-open-hand"),
        Role: null,
        DynastyCards: new[] { "moto-horde", "naive-student", "serene-warrior", "unassuming-yojimbo", "tattooed-wanderer" }.Select(LoadJson).ToArray(),
        ConflictCards: new[] { "bayushi-liar", "doji-whisperer", "eager-scout", "kaiu-envoy", "matsu-berserker" }.Select(LoadJson).ToArray());

    private static GameState NewGame()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        return new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
    }

    [Test]
    public void BuildsStrongholdRoleDecksProvincesAndStartingHand()
    {
        var game = NewGame();
        var deck = BuildDeckList();

        GameSetup.SetUpGame(game, deck, deck, new SeededRandom(42));

        Assert.That(game.Player1.Stronghold, Is.Not.Null);
        Assert.That(game.Player1.Stronghold!.Id, Is.EqualTo("city-of-the-open-hand"));
        Assert.That(game.Player1.Honor, Is.EqualTo(10), "set from the stronghold's printed honor");
        Assert.That(game.Player1.Provinces, Has.Count.EqualTo(4));
        Assert.That(game.Player1.Provinces, Has.All.Matches<Card>(c => c.Facedown && c.Location == "province"));
        Assert.That(game.Player1.Provinces.Select(c => c.ProvinceSlot), Is.EquivalentTo(new[] { "0", "1", "2", "3" }));
        Assert.That(game.Player1.DynastyDeck, Has.Count.EqualTo(1), "5 dynasty cards - 4 dealt to provinces");
        Assert.That(game.Player1.Hand, Has.Count.EqualTo(4));
        Assert.That(game.Player1.Deck, Has.Count.EqualTo(1), "5 conflict cards - 4 drawn to hand");
    }

    [Test]
    public void SameSeed_ProducesIdenticalDeckAndHandOrder()
    {
        var gameA = NewGame();
        var gameB = NewGame();
        var deck = BuildDeckList();

        GameSetup.SetUpGame(gameA, deck, deck, new SeededRandom(1234));
        GameSetup.SetUpGame(gameB, deck, deck, new SeededRandom(1234));

        Assert.That(gameA.Player1.Hand.Select(c => c.Id), Is.EqualTo(gameB.Player1.Hand.Select(c => c.Id)));
        Assert.That(gameA.Player1.Provinces.Select(c => c.Id), Is.EqualTo(gameB.Player1.Provinces.Select(c => c.Id)));
        Assert.That(gameA.Player1.Deck.Select(c => c.Id), Is.EqualTo(gameB.Player1.Deck.Select(c => c.Id)));
    }

    [Test]
    public void DifferentSeeds_CanProduceDifferentOrder()
    {
        var gameA = NewGame();
        var gameB = NewGame();
        var deck = BuildDeckList();

        GameSetup.SetUpGame(gameA, deck, deck, new SeededRandom(1));
        GameSetup.SetUpGame(gameB, deck, deck, new SeededRandom(2));

        var handsMatch = gameA.Player1.Hand.Select(c => c.Id).SequenceEqual(gameB.Player1.Hand.Select(c => c.Id));
        var deckOrderMatches = gameA.Player1.Deck.Select(c => c.Id).SequenceEqual(gameB.Player1.Deck.Select(c => c.Id));
        var provincesMatch = gameA.Player1.Provinces.Select(c => c.Id).SequenceEqual(gameB.Player1.Provinces.Select(c => c.Id));

        Assert.That(handsMatch && deckOrderMatches && provincesMatch, Is.False, "different seeds should diverge somewhere across hand/deck/provinces");
    }
}
