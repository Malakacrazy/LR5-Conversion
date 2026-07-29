namespace L5R.Engine.State;

public sealed class GameState
{
    public required Player Player1 { get; init; }
    public required Player Player2 { get; init; }
    public Phase CurrentPhase { get; set; }
    public required Player ActivePlayer { get; set; }

    /// <summary>
    /// ringteki game.js: roundNumber starts at 0 and is bumped to 1 by DynastyPhase.
    /// createPhase() the first time the game enters Dynasty. Our GameState is always
    /// constructed already "mid-game" (tests set CurrentPhase directly, with no separate
    /// game-start step), so 1 is the sensible resting default rather than a pre-game 0
    /// nothing in this engine represents yet.
    /// </summary>
    public int RoundNumber { get; set; } = 1;

    /// <summary>All cards controlled by either player, regardless of zone.</summary>
    public IEnumerable<Card> AllCards() => Player1.Hand.Concat(Player1.PlayArea).Concat(Player2.Hand).Concat(Player2.PlayArea);

    public Player Opponent(Player player) => player == Player1 ? Player2 : Player1;

    /// <summary>Null outside of a conflict - see Conflict's own doc comment for what's deliberately not modeled yet.</summary>
    public Conflict? CurrentConflict { get; set; }

    /// <summary>
    /// Active cardLastingEffect modifiers - see CardLastingEffectGameActionHandler and
    /// LastingEffect's own doc comment for why every entry here is always "untilEndOfPhase".
    /// </summary>
    public List<LastingEffect> LastingEffects { get; } = new();

    public int EffectiveGlory(Card card) =>
        (card.PrintedGlory ?? 0) + LastingEffects.Where(e => e.Target == card && e.Stat == "glory").Sum(e => e.Value);

    /// <summary>
    /// ringteki game.js beginRound(): queues DynastyPhase, DrawPhase, ConflictPhase,
    /// FatePhase, then loops back into a new DynastyPhase - Regroup is a real Phases enum
    /// value in Constants.ts, but this ringteki version's round loop never actually queues
    /// a separate Regroup phase (FatePhase's own steps cover readying cards/returning rings
    /// instead), so it's unreachable here too. This method only moves CurrentPhase/
    /// RoundNumber forward and expires untilEndOfPhase lasting effects; no other side
    /// effects yet (no fate collection, no card flipping) - those are added only once a
    /// card actually needs them.
    /// </summary>
    public void AdvancePhase()
    {
        CurrentPhase = CurrentPhase switch
        {
            Phase.Dynasty => Phase.Draw,
            Phase.Draw => Phase.Conflict,
            Phase.Conflict => Phase.Fate,
            Phase.Fate => Phase.Dynasty,
            _ => throw new NotSupportedException($"AdvancePhase does not support starting from '{CurrentPhase}'.")
        };

        if (CurrentPhase == Phase.Dynasty)
            RoundNumber++;

        LastingEffects.Clear();
    }
}
