using System.Linq;
using System.Text.Json;
using L5R.Engine.GameSteps;
using L5R.Engine.Randomness;
using L5R.Engine.Scheduling;
using L5R.Engine.State;

namespace L5R.Engine.Tests.GameSteps;

public class GameLoopTests
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

    private static GameState NewGame()
    {
        var p1 = new Player { Name = "Player1" };
        var p2 = new Player { Name = "Player2" };
        return new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
    }

    [Test]
    public void WithBothPlayersAlwaysPassing_CyclesRoundsAndStopsAtTheRoundCap()
    {
        var game = NewGame();
        var deck = BuildDeckList();
        GameSetup.SetUpGame(game, deck, deck, new SeededRandom(1));

        var scheduler = new Scheduler();
        var loop = new GameLoop(game, scheduler, new AlwaysPassBotPolicy(), new AlwaysPassBotPolicy(), roundCap: 3);

        loop.Start();
        var state = scheduler.Pump();

        Assert.That(state, Is.EqualTo(StepState.Idle));
        Assert.That(game.RoundNumber, Is.EqualTo(4), "stops the round after RoundNumber exceeds the cap of 3");
        Assert.That(game.CurrentPhase, Is.EqualTo(Phase.Dynasty));
        Assert.That(game.Winner, Is.Null, "always-pass bots never attack, so no win condition triggers");
    }

    [Test]
    public void WithBothPlayersPlayingFirstLegalAction_PlaysACompleteGameToTheRoundCapWithoutThrowing()
    {
        var game = NewGame();
        var deck = BuildDeckList();
        GameSetup.SetUpGame(game, deck, deck, new SeededRandom(7));

        var scheduler = new Scheduler();
        var loop = new GameLoop(game, scheduler, new FirstLegalActionBotPolicy(), new FirstLegalActionBotPolicy(), roundCap: 10);

        loop.Start();
        StepState state = default;
        Assert.DoesNotThrow(() => state = scheduler.Pump());

        Assert.That(state, Is.EqualTo(StepState.Idle));
        Assert.That(game.RoundNumber, Is.GreaterThanOrEqualTo(1));
        Assert.That(game.RoundNumber, Is.LessThanOrEqualTo(11), "either a real win (conquest) ended it early, or it ran out the round cap");
        // Draw phase always runs regardless of what later happens to drawn cards (played,
        // discarded for lack of fate, etc.), so the conflict deck shrinking is a persistent,
        // timing-independent signal that the loop actually executed real rounds rather than
        // just proving it didn't throw.
        Assert.That(game.Player1.Deck.Count < 6 || game.Player2.Deck.Count < 6, Is.True, "at least one full Draw phase should have run");
    }
}
