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
    /// The conflict deck - drawn to hand (DrawGameActionHandler), same deck know-the-world/
    /// shrewd-yasuki's own "conflict deck" text refers to. Named plainly "Deck" from when
    /// this was the only deck; kept as-is rather than renamed to avoid touching every
    /// existing reference across the whole ported card set for a milestone that doesn't need
    /// to. Index 0 is the top of the deck. See DynastyDeck's own doc comment for a caveat
    /// this split exposes in a handful of pre-existing scripts.
    /// </summary>
    public List<Card> Deck { get; } = new();

    /// <summary>
    /// The dynasty deck - fills provinces (GameSetup, and refills them after a dynasty card
    /// is played/discarded from one). Added for GameSetup's real game-start flow; a handful
    /// of pre-existing scripts written back when only one deck existed (akodo-gunso's own
    /// province refill, rebuild, shrewd-yasuki) reference the plain Deck field above for
    /// what should really be *this* deck - left as-is (their own tests still pass, since
    /// those tests populate whichever field the script reads) rather than fixed as a
    /// tangent to this milestone; worth reconciling whenever one of those cards is revisited.
    /// Index 0 is the top of the deck.
    /// </summary>
    public List<Card> DynastyDeck { get; } = new();

    /// <summary>
    /// A flat list rather than ringteki's 5 fixed, individually-named slots (stronghold +
    /// 4 provinces) - most ported cards only need to know a card is a province
    /// (Card.Location = "province") and possibly facedown (Card.Facedown), not which
    /// specific slot. borderlands-fortifications/rebuild/akodo-gunso are the first three
    /// that need positional identity across a swap/refill - see Card.ProvinceSlot's own
    /// doc comment for why that's tracked per-card instead of reshaping this into an
    /// indexed/fixed-size structure. Not touched by ZoneMover yet - no ported gameAction
    /// moves a card into or out of this zone via that shared helper (flipDynasty only flips
    /// the facedown flag; the three scripts above mutate Provinces/ProvinceSlot directly).
    /// </summary>
    public List<Card> Provinces { get; } = new();

    /// <summary>
    /// This player's stronghold card, null until set - see GameState.SetHonorFromStronghold/
    /// FateIncomeFor/StrongholdStrengthBonusFor for what it provisions. Not moved into
    /// Provinces (ringteki keeps it in its own distinct "stronghold province" zone, not one
    /// of the 4 regular province slots).
    /// </summary>
    public Card? Stronghold { get; set; }

    /// <summary>
    /// This player's role card (seeker-initiate/keeper-initiate's own "context.player.role.
    /// hasTrait(...)" check) - null until set, same convention as Stronghold. Not moved into
    /// any zone list; a role isn't part of the deck/discard/play-area lifecycle any ported
    /// card's executable slice needs yet.
    /// </summary>
    public Card? Role { get; set; }
}
