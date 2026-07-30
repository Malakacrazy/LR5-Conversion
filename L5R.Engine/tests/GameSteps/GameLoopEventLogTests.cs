using System.Linq;
using System.Text.Json;
using L5R.Engine.GameSteps;
using L5R.Engine.Logging;
using L5R.Engine.Randomness;
using L5R.Engine.Scheduling;
using L5R.Engine.State;

namespace L5R.Engine.Tests.GameSteps;

/// <summary>
/// Extends Replay/ReplayDeterminismTests' own pattern (which stood in for match setup with
/// fake string card ids "until real Game/Player/Deck types exist") to an actual full game:
/// GameSetup+GameLoop, driven by the same seed, must produce byte-identical EventLog output -
/// the literal Phase 1 exit criterion this whole plan exists to satisfy.
/// </summary>
public class GameLoopEventLogTests
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
        DynastyCards: new[] { "moto-horde", "naive-student", "serene-warrior", "unassuming-yojimbo", "tattooed-wanderer", "kaiu-envoy" }.Select(LoadJson).ToArray(),
        ConflictCards: new[] { "bayushi-liar", "doji-whisperer", "eager-scout", "kaiu-envoy", "matsu-berserker", "serene-warrior" }.Select(LoadJson).ToArray());

    private static byte[] RunGame(ulong seed)
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var deck = BuildDeckList();
        var eventLog = new EventLog();

        GameSetup.SetUpGame(game, deck, deck, new SeededRandom(seed), eventLog);

        var scheduler = new Scheduler();
        var loop = new GameLoop(game, scheduler, new FirstLegalActionBotPolicy(), new FirstLegalActionBotPolicy(), roundCap: 10, eventLog: eventLog);
        loop.Start();
        scheduler.Pump();

        return eventLog.ToCanonicalBytes();
    }

    [Test]
    public void SameSeed_ProducesAByteIdenticalEventLogAcrossTwoFullGames()
    {
        var first = RunGame(99);
        var second = RunGame(99);

        Assert.That(second, Is.EqualTo(first));
    }

    [Test]
    public void DifferentSeeds_CanProduceADifferentEventLog()
    {
        var first = RunGame(1);
        var second = RunGame(2);

        Assert.That(second, Is.Not.EqualTo(first));
    }

    [Test]
    public void TheEventLogRecordsSetupAndGameplayEvents()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        var deck = BuildDeckList();
        var eventLog = new EventLog();

        GameSetup.SetUpGame(game, deck, deck, new SeededRandom(42), eventLog);

        var scheduler = new Scheduler();
        var loop = new GameLoop(game, scheduler, new FirstLegalActionBotPolicy(), new FirstLegalActionBotPolicy(), roundCap: 5, eventLog: eventLog);
        loop.Start();
        scheduler.Pump();

        var eventNames = eventLog.Entries.Select(e => e.EventName).ToList();
        Assert.That(eventNames, Does.Contain("deckShuffled"));
        Assert.That(eventNames, Does.Contain("phaseChanged"));
    }
}
