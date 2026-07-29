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
}
