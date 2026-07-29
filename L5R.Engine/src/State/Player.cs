namespace L5R.Engine.State;

/// <summary>
/// Minimal player state - just enough for GetLegalActions to have real zones and
/// resources to check, plus a single Discard zone for the first executable costs
/// (sacrificeSelf). Not yet split into dynasty/conflict discard piles - grows when a
/// card actually needs that distinction. Grows further as later card groups need more
/// (decks, provinces, etc).
/// </summary>
public sealed class Player
{
    public required string Name { get; init; }
    public int Fate { get; set; }
    public int Honor { get; set; }

    /// <summary>The raw honor dial value revealed for the current conflict (ringteki player.js's showBid). Distinct from HonorBidModifier - honorBid itself (max(0, showBid + honorBidModifier)) isn't modeled since no ported card needs it yet, only the two raw parts.</summary>
    public int ShowBid { get; set; }

    /// <summary>Accumulator mutated by the modifyBid gameAction. See ShowBid's doc comment.</summary>
    public int HonorBidModifier { get; set; }

    /// <summary>Which favor this player currently holds: "", "military", or "political". No claiming mechanic exists yet - set directly by the caller, like ShowBid.</summary>
    public string ImperialFavor { get; set; } = "";
    public List<Card> Hand { get; } = new();
    public List<Card> PlayArea { get; } = new();
    public List<Card> Discard { get; } = new();

    /// <summary>
    /// Single deck for now (not yet split into dynasty/conflict decks) - grows when a
    /// card needs that distinction. Index 0 is the top of the deck.
    /// </summary>
    public List<Card> Deck { get; } = new();
}
