using System.Collections;
using System.Linq;
using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.Logging;
using L5R.Engine.Scheduling;
using L5R.Engine.State;

namespace L5R.Engine.GameSteps;

/// <summary>
/// Ports ringteki's beginRound() (game.js): Dynasty -&gt; Draw -&gt; Conflict -&gt; Fate, looping
/// forever - except ringteki's own loop never actually stops once a winner is recorded (a
/// real client just stops reading prompts); this harness checks GameState.Winner after every
/// phase and stops explicitly, plus a hard round-count safety cap ringteki has none of (a bot
/// pairing that never reaches a win threshold would otherwise loop forever).
///
/// Deliberately out of scope for v1 (none of it is needed to prove a real, complete game
/// plays end to end on the generic-DSL card set - see ConflictResolver's own doc comment for
/// the conflict-specific list): the Draw/Fate phases' action windows (no ported card's bot-
/// relevant behavior needs one yet), and imperial favor's glory count at the end of the
/// Conflict phase.
///
/// One real gap this surfaced once actual usable actions started mattering (Phase B):
/// LegalActions.GetLegalActions/IBotPolicy.ChooseAction existed and were tested in isolation
/// since M1, but nothing here ever actually consulted them during a simulated game - a card
/// like adept-of-shadows' "pay 1 honor, return to hand" action was fully wired end to end in
/// its own test but unreachable through a real game. RunActionWindow (ringteki's own
/// "preConflict" ActionWindow placement) closes that: one generic window, right before
/// conflicts are declared, where either player may activate any currently-legal CardAction
/// (plain JSON abilities.actions[] via ChooseAction, or - once adopted - a Phase B scripted
/// action via ChooseScriptedAction) or pass.
/// </summary>
public sealed class GameLoop
{
    private const int ProvinceCount = 4;
    private const int ConflictOpportunitiesPerPlayer = 2;

    private readonly GameState _game;
    private readonly Scheduler _scheduler;
    private readonly IReadOnlyDictionary<Player, IBotPolicy> _policies;
    private readonly int _roundCap;
    private readonly EventLog? _eventLog;

    /// <summary>eventLog is optional (defaults to null, logging nothing) so M3/M4's own tests, written before EventLog was wired in, don't need updating for a milestone that isn't theirs.</summary>
    public GameLoop(GameState game, Scheduler scheduler, IBotPolicy player1Policy, IBotPolicy player2Policy, int roundCap = 50, EventLog? eventLog = null)
    {
        _game = game;
        _scheduler = scheduler;
        _policies = new Dictionary<Player, IBotPolicy> { [game.Player1] = player1Policy, [game.Player2] = player2Policy };
        _roundCap = roundCap;
        _eventLog = eventLog;
    }

    public void Start() => _scheduler.QueueStep(BeginRound());

    private IEnumerable<Player> Players()
    {
        yield return _game.Player1;
        yield return _game.Player2;
    }

    private bool ShouldStop() => _game.Winner is not null || _game.RoundNumber > _roundCap;

    private IEnumerator BeginRound()
    {
        if (ShouldStop()) yield break;

        _scheduler.QueueStep(DynastyPhaseStep());
        yield return null;
        if (ShouldStop()) yield break;

        _scheduler.QueueStep(DrawPhaseStep());
        yield return null;
        if (ShouldStop()) yield break;

        _scheduler.QueueStep(ConflictPhaseStep());
        yield return null;
        if (ShouldStop()) yield break;

        _scheduler.QueueStep(FatePhaseStep());
        yield return null;
        if (ShouldStop()) yield break;

        _scheduler.QueueStep(BeginRound());
        yield return null;
    }

    private IEnumerator DynastyPhaseStep()
    {
        foreach (var player in Players())
        {
            while (player.Provinces.Count < ProvinceCount && player.DynastyDeck.Count > 0)
            {
                var card = player.DynastyDeck[0];
                player.DynastyDeck.RemoveAt(0);
                card.Facedown = true;
                card.Location = "province";
                card.ProvinceSlot = NextFreeProvinceSlot(player);
                player.Provinces.Add(card);
            }

            foreach (var province in player.Provinces)
                province.Facedown = false;

            player.Fate += _game.FateIncomeFor(player);
        }

        RunPlayWindow("province");

        _game.AdvancePhase();
        LogPhaseChanged();
        yield break;
    }

    /// <summary>
    /// Player.Provinces is a plain list, not a fixed-size indexed structure (see
    /// Card.ProvinceSlot's own doc comment for why), so there's no stored record of which of
    /// the ProvinceCount slots is free - derived instead from which slot identities ("0".."3")
    /// are currently occupied. A broken province is removed from Provinces without clearing
    /// its own ProvinceSlot (ConflictResolver.TryBreakProvince), so this naturally reassigns
    /// exactly the vacated slot to a refill, not just the next unused number.
    /// </summary>
    private static string NextFreeProvinceSlot(Player player)
    {
        var occupied = player.Provinces.Select(p => p.ProvinceSlot).Where(slot => slot is not null).ToHashSet();
        for (var i = 0; i < ProvinceCount; i++)
        {
            var candidate = i.ToString();
            if (!occupied.Contains(candidate))
                return candidate;
        }

        throw new InvalidOperationException($"'{player.Name}' has no free province slot to assign a new dynasty card to.");
    }

    private IEnumerator DrawPhaseStep()
    {
        foreach (var player in Players())
            ForgottenLibraryFirer.FireIfLegal(_game, player);

        foreach (var player in Players())
            player.ShowBid = _policies[player].ChooseHonorBid(_game, player);

        var first = _game.ActivePlayer;
        var other = _game.Opponent(first);
        var diff = Math.Abs(first.ShowBid - other.ShowBid);
        if (first.ShowBid > other.ShowBid)
        {
            other.Honor -= diff;
            first.Honor += diff;
        }
        else if (other.ShowBid > first.ShowBid)
        {
            first.Honor -= diff;
            other.Honor += diff;
        }

        _game.CheckWinCondition();
        if (_game.Winner is not null)
        {
            LogGameWon();
            yield break;
        }

        foreach (var player in Players())
        {
            var context = new AbilityContext { Game = _game, Player = player, Source = player.Stronghold! };
            using var parameters = JsonDocument.Parse($"{{\"amount\":{player.ShowBid}}}");
            new DrawGameActionHandler().Execute(context, parameters.RootElement);
        }

        _game.AdvancePhase();
        LogPhaseChanged();
        yield break;
    }

    private IEnumerator ConflictPhaseStep()
    {
        RunActionWindow();

        var current = _game.ActivePlayer;
        var consecutivePasses = 0;

        while (consecutivePasses < 2)
        {
            if (RemainingOpportunities(current) <= 0)
            {
                consecutivePasses++;
                current = _game.Opponent(current);
                continue;
            }

            var declaration = _policies[current].DeclareConflict(_game, current);
            if (declaration is null)
            {
                _game.ConflictDeclarationsThisPhase.Add((current, true));
                IntimidatingHidaFirer.FireIfLegal(_game, current);
                consecutivePasses++;
            }
            else
            {
                _game.ConflictDeclarationsThisPhase.Add((current, false));
                consecutivePasses = 0;
                LogConflictDeclared(current, declaration);

                ConflictResolver.Resolve(_game, current, declaration, _policies[_game.Opponent(current)], _policies[current], _eventLog);
                LogConflictResolved(declaration);

                _game.CheckWinCondition();
                if (_game.Winner is not null)
                {
                    LogGameWon();
                    yield break;
                }

                var bonusDeclaration = BreakthroughOfferer.TryDeclareBonusConflict(_game, current, _policies[current]);
                if (bonusDeclaration is not null)
                {
                    LogConflictDeclared(current, bonusDeclaration);
                    ConflictResolver.Resolve(_game, current, bonusDeclaration, _policies[_game.Opponent(current)], _policies[current], _eventLog);
                    LogConflictResolved(bonusDeclaration);

                    _game.CheckWinCondition();
                    if (_game.Winner is not null)
                    {
                        LogGameWon();
                        yield break;
                    }
                }
            }

            current = _game.Opponent(current);
        }

        foreach (var player in Players())
            ShibaTsukuneFirer.FireIfLegal(_game, player);

        _game.AdvancePhase();
        LogPhaseChanged();
        yield break;
    }

    private int RemainingOpportunities(Player player) =>
        ConflictOpportunitiesPerPlayer - _game.ConflictDeclarationsThisPhase.Count(d => d.Player == player && !d.Passed);

    private IEnumerator FatePhaseStep()
    {
        foreach (var player in Players())
            SteadfastSamuraiFirer.FireIfLegal(_game, player);

        foreach (var player in Players())
        {
            foreach (var character in player.PlayArea.Where(c => c.Type == CardType.Character).ToList())
            {
                if (character.Fate <= 0)
                {
                    if (_game.IsRestrictedFrom(character, "discardFromPlay"))
                        continue;

                    player.PlayArea.Remove(character);
                    character.Location = "discard";
                    player.Discard.Add(character);
                }
                else if (!_game.IsRestrictedFrom(character, "removeFate"))
                {
                    character.Fate--;
                }
            }
        }

        foreach (var ring in _game.Rings.Where(r => r.IsUnclaimed))
            ring.Fate++;

        foreach (var player in Players())
        {
            foreach (var card in player.PlayArea)
                card.Bowed = false;

            if (player.Stronghold is not null)
                player.Stronghold.Bowed = false;
        }

        foreach (var ring in _game.Rings.Where(r => r.Claimed))
        {
            ring.Claimed = false;
            ring.ClaimedBy = null;
        }

        WayOfTheUnicornOfferer.TryPlay(_game, _game.ActivePlayer);

        _game.AdvancePhase();
        LogPhaseChanged();
        yield break;
    }

    private void RunPlayWindow(string location)
    {
        var current = _game.ActivePlayer;
        var consecutivePasses = 0;

        while (consecutivePasses < 2)
        {
            var card = _policies[current].ChoosePlay(_game, current, location);
            if (card is null)
            {
                consecutivePasses++;
            }
            else
            {
                consecutivePasses = 0;
                string? provinceSlot = null;
                if (location == "province")
                {
                    current.Provinces.Remove(card);
                    provinceSlot = card.ProvinceSlot;
                    card.ProvinceSlot = null;
                }

                var context = new AbilityContext { Game = _game, Player = current, Source = card };
                new PlayCardGameActionHandler().Execute(context, null);
                _eventLog?.Record("cardPlayed", new Dictionary<string, string> { ["player"] = current.Name, ["card"] = card.Id, ["from"] = location });

                if (card.Type == CardType.Event)
                {
                    EventResolver.ResolveAndDiscard(_game, card, current, _policies[current].ResolveEventScript(card.Id));
                    _eventLog?.Record("eventResolved", new Dictionary<string, string> { ["player"] = current.Name, ["card"] = card.Id });
                }
                else if (card.Type == CardType.Character && provinceSlot is not null)
                {
                    AkodoGunsoFirer.FireIfLegal(_game, card, provinceSlot);
                }
            }

            current = _game.Opponent(current);
        }
    }

    /// <summary>
    /// The pre-conflict action window (ringteki's own "preConflict" ActionWindow placement),
    /// starting with the current first player - see ActionWindowRunner for the actual shared
    /// loop, also used mid-conflict by ConflictResolver.
    /// </summary>
    private void RunActionWindow() => ActionWindowRunner.Run(_game, _game.ActivePlayer, _policies, _eventLog);

    private void LogPhaseChanged() =>
        _eventLog?.Record("phaseChanged", new Dictionary<string, string> { ["phase"] = _game.CurrentPhase.ToString(), ["round"] = _game.RoundNumber.ToString() });

    private void LogConflictDeclared(Player attacker, ConflictDeclaration declaration) =>
        _eventLog?.Record("conflictDeclared", new Dictionary<string, string>
        {
            ["attacker"] = attacker.Name,
            ["ring"] = declaration.Ring,
            ["province"] = declaration.Province.Id,
            ["attackers"] = string.Join(",", declaration.Attackers.Select(c => c.Id))
        });

    /// <summary>
    /// Folds "province broken"/"ring claimed" (their own listed event categories in the plan)
    /// into this single event's data rather than firing three separate event types - both are
    /// simple observable facts about the conflict that just resolved, not additional decisions
    /// or state transitions worth their own event name.
    /// </summary>
    private void LogConflictResolved(ConflictDeclaration declaration)
    {
        var conflict = _game.ConflictRecord[^1];
        var ring = _game.Rings.First(r => r.Element == declaration.Ring);

        _eventLog?.Record("conflictResolved", new Dictionary<string, string>
        {
            ["winner"] = conflict.Winner?.Name ?? "none",
            ["skillDifference"] = conflict.SkillDifference.ToString(),
            ["unopposed"] = conflict.Unopposed.ToString(),
            ["provinceBroken"] = declaration.Province.Broken.ToString(),
            ["ringClaimed"] = ring.Claimed.ToString(),
            ["ringClaimedBy"] = ring.ClaimedBy?.Name ?? "none"
        });
    }

    private void LogGameWon() =>
        _eventLog?.Record("gameWon", new Dictionary<string, string> { ["winner"] = _game.Winner!.Name });
}
