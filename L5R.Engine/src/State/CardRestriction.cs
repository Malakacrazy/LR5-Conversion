namespace L5R.Engine.State;

/// <summary>
/// One active cardCannot restriction (ringteki BaseCard.checkRestrictions). Action is the
/// "cannot" value from the card JSON (e.g. "triggerAbilities", "moveToConflict") - a
/// free-form string, not an enum, matching ValueRefResolver's "dynamic" convention: only
/// the values a ported card's executable slice actually needs are consulted by anything.
/// Duration/expiry mirrors LastingEffect exactly (see its own doc comment).
/// </summary>
public sealed class CardRestriction
{
    public required Card Target { get; init; }
    public required string Action { get; init; }
    public required string Duration { get; init; }
}
