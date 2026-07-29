namespace L5R.Engine.State;

/// <summary>
/// Minimal conflict state - just enough to answer isParticipating/isAttacking/isDefending,
/// isDuringConflict's "type" filter, and to let moveToConflict/sendHome move characters in
/// and out. Still deliberately has no real Ring or province reference: ringteki's
/// conflictType/elements are actually derived from the declared ring
/// (this.ring.conflictType, this.ring.elements), but no ported card's executable slice
/// needs the ring object itself yet, only the two values it would produce - so they're
/// plain fields set directly by the caller (like GameState.CurrentPhase), not created
/// through a declaration/ring pipeline.
/// </summary>
public sealed class Conflict
{
    public required Player AttackingPlayer { get; init; }
    public required Player DefendingPlayer { get; init; }
    public List<Card> Attackers { get; } = new();
    public List<Card> Defenders { get; } = new();

    /// <summary>"military" or "political". Optional since not every existing test/card cares about it. Settable - see SwitchConflictTypeGameActionHandler.</summary>
    public string? ConflictType { get; set; }

    /// <summary>Ring element(s) in play for this conflict, e.g. ["fire"]. ringteki's isDuringConflict checks conflictType.concat(elements) against the same list.</summary>
    public List<string> Elements { get; init; } = new();

    /// <summary>
    /// event.conflict.winner/loser (honored-blade and several other afterConflict reactions'
    /// own scriptOverride reason) - no skill-comparison/breach resolution pipeline exists to
    /// compute these automatically (matches this engine's general lack of one), so they're
    /// caller-set facts, same convention as ConflictType/Elements' own "no declaration
    /// pipeline" doc comment above. Null until a caller sets them (e.g. before invoking a
    /// script that reacts to the conflict's own outcome).
    /// </summary>
    public Player? Winner { get; set; }
    public Player? Loser { get; set; }

    /// <summary>event.conflict.skillDifference (fallen-in-battle's "won by 5+ skill") - same caller-set-fact convention as Winner/Loser, no skill-comparison pipeline computes this automatically.</summary>
    public int SkillDifference { get; set; }

    /// <summary>this.game.currentConflict.attackerSkill/defenderSkill (kakita-asami's own comparison) - same caller-set-fact convention as Winner/Loser/SkillDifference; no skill-totaling pipeline sums participants' effective skill automatically.</summary>
    public int AttackerSkill { get; set; }
    public int DefenderSkill { get; set; }
}
