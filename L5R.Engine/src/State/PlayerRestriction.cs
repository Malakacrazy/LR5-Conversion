namespace L5R.Engine.State;

/// <summary>
/// One active playerCannot restriction - the player-scoped sibling of CardRestriction.
/// Source is the card that granted this restriction, needed for the "source" Qualifier
/// (doomed-shugenja's "you cannot place fate when playing [a copy of] this character" -
/// checked against whichever card is being considered, not this restriction's target
/// player). Duration/expiry mirrors CardRestriction/LastingEffect exactly.
/// </summary>
public sealed class PlayerRestriction
{
    public required Player Target { get; init; }
    public required string Action { get; init; }
    public required Card Source { get; init; }
    public string? Qualifier { get; init; }
    public required string Duration { get; init; }
}
