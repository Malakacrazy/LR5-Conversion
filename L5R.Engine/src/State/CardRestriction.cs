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

    /// <summary>
    /// Optional conflict-type/element scoping (pacifism's "cannot participate as attacker/
    /// defender... in a military conflict" - value: "military"), null for an unconditional
    /// restriction. Checked against GameState.CurrentConflict the same way isDuringConflict's
    /// "type" filter is (ConflictType or Elements).
    /// </summary>
    public string? Qualifier { get; init; }
}
