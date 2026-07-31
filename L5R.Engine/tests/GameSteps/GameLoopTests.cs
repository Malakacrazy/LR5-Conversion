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

    public Task<CardAction?> ChooseAction(GameState game, Player player) => Task.FromResult<CardAction?>(null);
    public Task<Card?> ChoosePlay(GameState game, Player player, string location) => Task.FromResult<Card?>(null);
    public Task<int> ChooseHonorBid(GameState game, Player player) => Task.FromResult(_bid);
    public Task<ConflictDeclaration?> DeclareConflict(GameState game, Player player) => Task.FromResult<ConflictDeclaration?>(null);
    public Task<IReadOnlyList<Card>> DeclareDefenders(GameState game, Conflict conflict, Player defender) => Task.FromResult<IReadOnlyList<Card>>(Array.Empty<Card>());
    public Task<(Card Source, IBotScriptAction Action)?> ChooseScriptedAction(GameState game, Player player) => Task.FromResult<(Card Source, IBotScriptAction Action)?>(null);
    public Task<IBotScriptAction?> ResolveEventScript(string cardId) => Task.FromResult<IBotScriptAction?>(null);
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

    [Test]
    public void RunPlayWindow_PlayingAkodoGunsoFromAProvince_RefillsTheVacatedSlotFromTheConflictDeck()
    {
        // DynastyPhaseStep deals gunso into province slot "0" (the only dynasty card, hence
        // the only province) - proves the whole pipeline end to end: slot assignment at deal
        // time, capture-and-clear when played from a province, and akodo-gunso's own refill.
        var p1 = new Player { Name = "Player1", Honor = 5, Fate = 5 };
        var p2 = new Player { Name = "Player2", Honor = 5 };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        p1.Stronghold = new Card { Id = "sh1", Type = CardType.Stronghold, Controller = p1, PrintedFateIncome = 0, PrintedHonor = 5 };
        p2.Stronghold = new Card { Id = "sh2", Type = CardType.Stronghold, Controller = p2, PrintedFateIncome = 0, PrintedHonor = 5 };

        var gunso = new Card { Id = "akodo-gunso", Type = CardType.Character, Controller = p1, PrintedCost = 0, Fate = 1 };
        p1.DynastyDeck.Add(gunso);
        // Unaffordable on purpose: once the refill lands in Provinces it's a legal province
        // play too (Facedown false, per akodo-gunso's own script), and FirstLegalActionBotPolicy
        // would immediately play it in the same Dynasty phase window otherwise - correct bot
        // behavior, but it would leave Provinces empty before this test's own assertions run.
        var refill = new Card { Id = "refill-card", Type = CardType.Character, Controller = p1, PrintedCost = 10 };
        p1.Deck.Add(refill);

        var scheduler = new Scheduler();
        var loop = new GameLoop(game, scheduler, new FirstLegalActionBotPolicy(), new FirstLegalActionBotPolicy(), roundCap: 1);
        loop.Start();
        scheduler.Pump();

        Assert.That(p1.PlayArea, Contains.Item(gunso));
        Assert.That(gunso.ProvinceSlot, Is.Null, "cleared once it entered play");
        Assert.That(p1.Provinces, Contains.Item(refill));
        Assert.That(refill.ProvinceSlot, Is.EqualTo("0"), "refilled the same slot gunso vacated");
        Assert.That(p1.Deck, Does.Not.Contain(refill));
    }

    [Test]
    public void FatePhaseStep_ReadiesBowedStrongholds()
    {
        // Nothing ever bowed a stronghold before mountain-s-anvil-castle's own activatable
        // ability - proves the Fate phase now readies them the same way it already readies
        // every PlayArea card, so the ability isn't a permanent one-per-game use.
        var p1 = new Player { Name = "Player1", Honor = 5 };
        var p2 = new Player { Name = "Player2", Honor = 5 };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        p1.Stronghold = new Card { Id = "sh1", Type = CardType.Stronghold, Controller = p1, PrintedFateIncome = 0, PrintedHonor = 5, Bowed = true };
        p2.Stronghold = new Card { Id = "sh2", Type = CardType.Stronghold, Controller = p2, PrintedFateIncome = 0, PrintedHonor = 5 };

        var scheduler = new Scheduler();
        var loop = new GameLoop(game, scheduler, new AlwaysPassBotPolicy(), new AlwaysPassBotPolicy(), roundCap: 1);
        loop.Start();
        scheduler.Pump();

        Assert.That(p1.Stronghold!.Bowed, Is.False);
    }

    [Test]
    public void FatePhaseStep_ProtectsASteadfastSamuraiAtZeroFateFromTheNoFateDiscardSweep()
    {
        // Before steadfast-samurai, the fate-decrement/no-fate-discard loop mutated
        // Card.Fate/PlayArea directly rather than going through
        // RemoveFateGameActionHandler/DiscardFromPlayGameActionHandler - so a restriction
        // added against those actions would have been silently ignored by this loop. Proves
        // the loop now actually respects it, through a real simulated Fate phase.
        var p1 = new Player { Name = "Player1", Honor = 10 };
        var p2 = new Player { Name = "Player2", Honor = 5 };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        p1.Stronghold = new Card { Id = "sh1", Type = CardType.Stronghold, Controller = p1, PrintedFateIncome = 0, PrintedHonor = 5 };
        p2.Stronghold = new Card { Id = "sh2", Type = CardType.Stronghold, Controller = p2, PrintedFateIncome = 0, PrintedHonor = 5 };
        var samurai = new Card { Id = "steadfast-samurai", Type = CardType.Character, Controller = p1, Fate = 0 };
        p1.PlayArea.Add(samurai);

        var scheduler = new Scheduler();
        var loop = new GameLoop(game, scheduler, new AlwaysPassBotPolicy(), new AlwaysPassBotPolicy(), roundCap: 1);
        loop.Start();
        scheduler.Pump();

        Assert.That(p1.PlayArea, Contains.Item(samurai));
        Assert.That(p1.Discard, Does.Not.Contain(samurai));
    }

    [Test]
    public void FatePhaseStep_PlayingWayOfTheUnicorn_KeepsTheFirstPlayerToken()
    {
        // Without way-of-the-unicorn, AdvancePhase's own Dynasty rollover would flip
        // ActivePlayer to p2 at the end of round 1 - proves the new Fate-phase offerer
        // actually reaches and cancels that pass through a real simulated round.
        var p1 = new Player { Name = "Player1", Honor = 5, Fate = 5 };
        var p2 = new Player { Name = "Player2", Honor = 5 };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        p1.Stronghold = new Card { Id = "sh1", Type = CardType.Stronghold, Controller = p1, PrintedFateIncome = 0, PrintedHonor = 5 };
        p2.Stronghold = new Card { Id = "sh2", Type = CardType.Stronghold, Controller = p2, PrintedFateIncome = 0, PrintedHonor = 5 };

        var card = CardFactory.BuildCard(LoadJson("way-of-the-unicorn"), p1);
        card.Location = "hand";
        p1.Hand.Add(card);

        var scheduler = new Scheduler();
        var loop = new GameLoop(game, scheduler, new FirstLegalActionBotPolicy(), new FirstLegalActionBotPolicy(), roundCap: 1);
        loop.Start();
        scheduler.Pump();

        Assert.That(game.ActivePlayer, Is.EqualTo(p1));
        Assert.That(p1.Discard, Contains.Item(card));
    }

    [Test]
    public void ConflictPhaseStep_WithBreakthroughInHand_DeclaresABonusConflictAfterBreakingAProvince()
    {
        // ConflictOpportunitiesPerPlayer is 2, but p1 only ever gets ONE normal declaration
        // here (p2 has nothing to attack with, so it never contests p1's turn) - proves
        // breakthrough's own bonus declaration is what lets p1 break a *second* province in
        // the same phase, wired through GameLoop.ConflictPhaseStep itself.
        var p1 = new Player { Name = "Player1", Honor = 5, Fate = 5 };
        var p2 = new Player { Name = "Player2", Honor = 5 };
        var game = new GameState { Player1 = p1, Player2 = p2, ActivePlayer = p1 };
        p1.Stronghold = new Card { Id = "sh1", Type = CardType.Stronghold, Controller = p1, PrintedFateIncome = 0, PrintedHonor = 5 };
        p2.Stronghold = new Card { Id = "sh2", Type = CardType.Stronghold, Controller = p2, PrintedFateIncome = 0, PrintedHonor = 5 };

        // Both skills set on each: the bonus conflict claims whichever ring is next
        // unclaimed after the first conflict's own ring, which may be a different
        // conflict type (military vs political) - not something this test controls.
        var attacker1 = new Card { Id = "attacker-1", Type = CardType.Character, Controller = p1, PrintedMilitarySkill = 10, PrintedPoliticalSkill = 10 };
        var attacker2 = new Card { Id = "attacker-2", Type = CardType.Character, Controller = p1, PrintedMilitarySkill = 10, PrintedPoliticalSkill = 10 };
        p1.PlayArea.Add(attacker1);
        p1.PlayArea.Add(attacker2);

        // Built via CardFactory (not hand-built) so its scriptOverride's CanPlay override is
        // actually wired - without it, the generic pre-conflict/mid-conflict hand-play
        // windows would discard it with no effect before it ever gets a chance to fire.
        var breakthrough = CardFactory.BuildCard(LoadJson("breakthrough"), p1);
        breakthrough.Location = "hand";
        p1.Hand.Add(breakthrough);

        var province1 = new Card { Id = "province-1", Type = CardType.Province, Controller = p2, PrintedProvinceStrength = 1 };
        var province2 = new Card { Id = "province-2", Type = CardType.Province, Controller = p2, PrintedProvinceStrength = 1 };
        p2.Provinces.Add(province1);
        p2.Provinces.Add(province2);

        var scheduler = new Scheduler();
        var loop = new GameLoop(game, scheduler, new FirstLegalActionBotPolicy(), new FirstLegalActionBotPolicy(), roundCap: 1);
        loop.Start();
        scheduler.Pump();

        Assert.That(province1.Broken, Is.True);
        Assert.That(province2.Broken, Is.True, "only possible via breakthrough's own bonus conflict");
        Assert.That(p1.Discard, Contains.Item(breakthrough));
    }
}
