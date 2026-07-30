using System.Collections;
using System.Linq;
using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;
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
/// </summary>
public sealed class GameLoop
{
    private const int ProvinceCount = 4;
    private const int ConflictOpportunitiesPerPlayer = 2;

    private readonly GameState _game;
    private readonly Scheduler _scheduler;
    private readonly IReadOnlyDictionary<Player, IBotPolicy> _policies;
    private readonly int _roundCap;

    public GameLoop(GameState game, Scheduler scheduler, IBotPolicy player1Policy, IBotPolicy player2Policy, int roundCap = 50)
    {
        _game = game;
        _scheduler = scheduler;
        _policies = new Dictionary<Player, IBotPolicy> { [game.Player1] = player1Policy, [game.Player2] = player2Policy };
        _roundCap = roundCap;
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
                player.Provinces.Add(card);
            }

            foreach (var province in player.Provinces)
                province.Facedown = false;

            player.Fate += _game.FateIncomeFor(player);
        }

        RunPlayWindow("province");

        _game.AdvancePhase();
        yield break;
    }

    private IEnumerator DrawPhaseStep()
    {
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

        foreach (var player in Players())
        {
            var context = new AbilityContext { Game = _game, Player = player, Source = player.Stronghold! };
            using var parameters = JsonDocument.Parse($"{{\"amount\":{player.ShowBid}}}");
            new DrawGameActionHandler().Execute(context, parameters.RootElement);
        }

        _game.AdvancePhase();
        yield break;
    }

    private IEnumerator ConflictPhaseStep()
    {
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
                consecutivePasses++;
            }
            else
            {
                _game.ConflictDeclarationsThisPhase.Add((current, false));
                consecutivePasses = 0;
                ConflictResolver.Resolve(_game, current, declaration, _policies[_game.Opponent(current)]);
                if (_game.Winner is not null) yield break;
            }

            current = _game.Opponent(current);
        }

        _game.AdvancePhase();
        yield break;
    }

    private int RemainingOpportunities(Player player) =>
        ConflictOpportunitiesPerPlayer - _game.ConflictDeclarationsThisPhase.Count(d => d.Player == player && !d.Passed);

    private IEnumerator FatePhaseStep()
    {
        foreach (var player in Players())
        {
            foreach (var character in player.PlayArea.Where(c => c.Type == CardType.Character).ToList())
            {
                if (character.Fate <= 0)
                {
                    player.PlayArea.Remove(character);
                    character.Location = "discard";
                    player.Discard.Add(character);
                }
                else
                {
                    character.Fate--;
                }
            }
        }

        foreach (var ring in _game.Rings.Where(r => r.IsUnclaimed))
            ring.Fate++;

        foreach (var player in Players())
            foreach (var card in player.PlayArea)
                card.Bowed = false;

        foreach (var ring in _game.Rings.Where(r => r.Claimed))
        {
            ring.Claimed = false;
            ring.ClaimedBy = null;
        }

        _game.AdvancePhase();
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
                if (location == "province")
                    current.Provinces.Remove(card);

                var context = new AbilityContext { Game = _game, Player = current, Source = card };
                new PlayCardGameActionHandler().Execute(context, null);
            }

            current = _game.Opponent(current);
        }
    }
}
