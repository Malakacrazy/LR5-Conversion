namespace L5R.Engine.State;

/// <summary>
/// One of the 5 rings' persistent claimed/unclaimed state across the whole game -
/// distinct from Conflict.ConflictType/Elements, which model the ring currently
/// contested *in a conflict*, not this. Plain mutable state (ringteki ring.js's
/// claimed/claimedBy/contested/fate), same convention as Card/Conflict - no
/// computed-effects layer, since nothing yet needs one.
/// </summary>
public sealed class Ring
{
    public required string Element { get; init; }

    /// <summary>"military" or "political" - flippable, like Conflict.ConflictType (ringteki's flipConflictType).</summary>
    public required string ConflictType { get; set; }

    public bool Claimed { get; set; }
    public Player? ClaimedBy { get; set; }

    /// <summary>True only while this is the ring of the currently-declared conflict - distinct from Claimed.</summary>
    public bool Contested { get; set; }

    public int Fate { get; set; }

    /// <summary>ringteki ring.js isUnclaimed(): !contested && !claimed.</summary>
    public bool IsUnclaimed => !Contested && !Claimed;
}
