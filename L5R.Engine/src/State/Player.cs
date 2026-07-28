namespace L5R.Engine.State;

/// <summary>
/// Minimal player state - just enough for GetLegalActions to have real zones and
/// resources to check. Grows when task 9 needs more (decks, provinces, etc).
/// </summary>
public sealed class Player
{
    public required string Name { get; init; }
    public int Fate { get; set; }
    public int Honor { get; set; }
    public List<Card> Hand { get; } = new();
    public List<Card> PlayArea { get; } = new();
}
