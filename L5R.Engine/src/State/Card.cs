namespace L5R.Engine.State;

/// <summary>
/// Minimal card state - location/controller/bowed/traits, enough for CardAction's
/// requirement checks (location, phase, player, condition), plus Fate for the first
/// executable gameActions (placeFate/removeFate). Grows further as later card groups
/// need more (skill values, attachments, persistent effects, etc).
/// </summary>
public sealed class Card
{
    public required string Id { get; init; }
    public required CardType Type { get; init; }
    public required Player Controller { get; set; }
    public string Location { get; set; } = "play area";
    public bool Bowed { get; set; }
    public int Fate { get; set; }
    public IReadOnlyList<string> Traits { get; init; } = Array.Empty<string>();
    public List<Abilities.CardAction> Actions { get; } = new();
}
