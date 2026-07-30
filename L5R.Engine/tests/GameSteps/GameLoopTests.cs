using System.Linq;
using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Cards;
using L5R.Engine.GameSteps;
using L5R.Engine.Randomness;
using L5R.Engine.Scheduling;
using L5R.Engine.State;

namespace L5R.Engine.Tests.GameSteps;

/// <summary>Only ChooseHonorBid matters - passes on everything else so the Draw-phase bid is the only thing that moves state.</summary>
public sealed class FixedBidBotPolicy : IBotPolicy
{
    private readonly int _bid;

    public FixedBidBotPolicy(int bid) => _bid = bid;

    public CardAction? ChooseAction(GameState game, Player player) => null;
    public Card? ChoosePlay(GameState game, Player player, string location) => null;
    public int ChooseHonorBid(GameState game, Player player) => _bid;
    public ConflictDeclaration? DeclareConflict(GameState game, Player player) => null;
    public IReadOnlyList<Card> DeclareDefenders(GameState game, Conflict conflict, Player defender) => Array.Empty<Card>();
    public (Card Source, IBotScriptAction Action)? ChooseScriptedAction(GameState game, Player player) => null;
    public IBotScriptAction? ResolveEventScript(string cardId) => null;
}

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

    [Test]
    public void HonorThresholdReachedDuringTheDrawPhaseBid_EndsTheGameImmediately()
    {
        var game = NewGame();
        var deck = BuildDeckList();
        GameSetup.SetUpGame(game, deck, deck, new SeededRandom(1));
        game.Player1.Honor = 20;

        var scheduler = new Scheduler();
        var loop = new GameLoop(game, scheduler, new FixedBidBotPolicy(5), new FixedBidBotPolicy(0), roundCap: 50);

        loop.Start();
        scheduler.Pump();

        Assert.That(game.Winner, Is.EqualTo(game.Player1));
        Assert.That(game.RoundNumber, Is.EqualTo(1), "the win is detected mid-round-1, well before the round cap");
    }

    [Test]
    public void RunActionWindow_LetsABotActivateAPlainAbilitiesActionsEntryOnAnInPlayCard()
    {
        // adept-of-shadows.json's "Return to hand" action (abilities.actions[]) was fully
        // wired end to end since M1 (CardFactory bridges it into Card.Actions, LegalActions/
        // ChooseAction can find it) but nothing in GameLoop ever actually consulted it during
        // a simulated game until RunActionWindow was added - this proves that gap is closed.
        var p1 = new Player { Name = "Player1", Honor = 5 };
        var p2 = new Player { Name = "Player2", Honor = 5 };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        p1.Stronghold = new Card { Id = "sh1", Type = CardType.Stronghold, Controller = p1, PrintedFateIncome = 0, PrintedHonor = 5 };
        p2.Stronghold = new Card { Id = "sh2", Type = CardType.Stronghold, Controller = p2, PrintedFateIncome = 0, PrintedHonor = 5 };

        var adept = CardFactory.BuildCard(LoadJson("adept-of-shadows"), p1);
        p1.PlayArea.Add(adept);

        var scheduler = new Scheduler();
        var loop = new GameLoop(game, scheduler, new FirstLegalActionBotPolicy(), new FirstLegalActionBotPolicy(), roundCap: 1);
        loop.Start();
        scheduler.Pump();

        Assert.That(p1.Hand, Contains.Item(adept), "the bot activated adept-of-shadows' own action during the pre-conflict action window");
        Assert.That(p1.Honor, Is.EqualTo(4), "payHonor(1) was paid to activate it");
    }

    [Test]
    public void RunActionWindow_LetsABotPlayACardFromHand()
    {
        // RunPlayWindow only ever ran for Dynasty's "province" location - hand cards (every
        // conflict-deck character/holding/attachment/event) could never be played by a bot at
        // all until RunActionWindow learned to offer ChoosePlay("hand") too. This proves a
        // plain character now makes it from hand into play through a real simulated game.
        var p1 = new Player { Name = "Player1", Honor = 5, Fate = 5 };
        var p2 = new Player { Name = "Player2", Honor = 5 };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        p1.Stronghold = new Card { Id = "sh1", Type = CardType.Stronghold, Controller = p1, PrintedFateIncome = 0, PrintedHonor = 5 };
        p2.Stronghold = new Card { Id = "sh2", Type = CardType.Stronghold, Controller = p2, PrintedFateIncome = 0, PrintedHonor = 5 };

        var scout = CardFactory.BuildCard(LoadJson("eager-scout"), p1);
        scout.Fate = 1; // survives the same round's Fate phase (0-fate characters are discarded there - see FatePhaseStep), so the play itself is what this test observes
        p1.Hand.Add(scout);

        var scheduler = new Scheduler();
        var loop = new GameLoop(game, scheduler, new FirstLegalActionBotPolicy(), new FirstLegalActionBotPolicy(), roundCap: 1);
        loop.Start();
        scheduler.Pump();

        Assert.That(p1.PlayArea, Contains.Item(scout));
        Assert.That(p1.Hand, Does.Not.Contain(scout));
    }

    [Test]
    public void RunActionWindow_PlayingAnEventFromHand_ResolvesItThenDiscardsIt()
    {
        var p1 = new Player { Name = "Player1", Honor = 5, Fate = 5 };
        var p2 = new Player { Name = "Player2", Honor = 5 };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        p1.Stronghold = new Card { Id = "sh1", Type = CardType.Stronghold, Controller = p1, PrintedFateIncome = 0, PrintedHonor = 5 };
        p2.Stronghold = new Card { Id = "sh2", Type = CardType.Stronghold, Controller = p2, PrintedFateIncome = 0, PrintedHonor = 5 };

        var vanillaEvent = new Card { Id = "test-vanilla-event", Type = CardType.Event, Controller = p1, PrintedCost = 0 };
        p1.Hand.Add(vanillaEvent);

        var scheduler = new Scheduler();
        var loop = new GameLoop(game, scheduler, new FirstLegalActionBotPolicy(), new FirstLegalActionBotPolicy(), roundCap: 1);
        loop.Start();
        scheduler.Pump();

        Assert.That(p1.Discard, Contains.Item(vanillaEvent), "an event never lingers in play - it resolves once, then discards");
        Assert.That(p1.PlayArea, Does.Not.Contain(vanillaEvent));
        Assert.That(p1.Hand, Does.Not.Contain(vanillaEvent));
    }
}
