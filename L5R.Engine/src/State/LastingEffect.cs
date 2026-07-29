namespace L5R.Engine.State;

/// <summary>
/// One active cardLastingEffect stat modifier. Duration is one of "untilEndOfPhase" or
/// "untilEndOfConflict" (the only two CardLastingEffectGameActionHandler accepts).
/// GameState.AdvancePhase() clears every entry unconditionally (ending a phase necessarily
/// ends any conflict within it too), while GameState.EndConflict() only clears the
/// "untilEndOfConflict" ones - a phase can outlive the conflict that created them once the
/// engine models multiple sequential conflicts per phase.
/// </summary>
public sealed class LastingEffect
{
    public required Card Target { get; init; }
    public required string Stat { get; init; }
    public required int Value { get; init; }
    public required string Duration { get; init; }

    /// <summary>
    /// Null for every ordinary additive effect (Value is added to the stat's printed base,
    /// the only shape that existed before way-of-the-lion). Set only by
    /// modifyBaseMilitarySkillMultiplier - the printed base is multiplied by this instead of
    /// Value being added. GameState.EffectiveStat applies at most one of these per stat;
    /// multiple simultaneous multipliers on the same card/stat aren't exercised by any
    /// ported card yet.
    /// </summary>
    public int? Multiplier { get; init; }
}
