namespace L5R.Engine.State;

/// <summary>
/// Minimal conflict state - just enough to answer isParticipating/isAttacking/isDefending
/// and to let moveToConflict/sendHome move characters in and out. Deliberately has no Ring,
/// declared Type, or province reference yet: ringteki's real conflictType is actually
/// derived from the declared ring (this.ring.conflictType), not a plain field, and no
/// ported card's executable slice needs it yet - isDuringConflict's "type" filter and
/// switchConflictType stay unsupported until a card forces that in. Set directly by the
/// caller (like GameState.CurrentPhase), not created through a declaration pipeline -
/// there isn't one yet.
/// </summary>
public sealed class Conflict
{
    public required Player AttackingPlayer { get; init; }
    public required Player DefendingPlayer { get; init; }
    public List<Card> Attackers { get; } = new();
    public List<Card> Defenders { get; } = new();
}
