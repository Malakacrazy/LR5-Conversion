namespace L5R.Engine.State;

/// <summary>
/// One active cardLastingEffect stat modifier. Deliberately has no Duration field: every
/// entry ever added is validated at creation time to be "untilEndOfPhase" (the only
/// duration currently supported - see CardLastingEffectGameActionHandler), so
/// GameState.AdvancePhase() can just clear the whole list unconditionally. Would need a
/// real duration/expiry field if untilEndOfConflict or untilEndOfRound is ever added.
/// </summary>
public sealed class LastingEffect
{
    public required Card Target { get; init; }
    public required string Stat { get; init; }
    public required int Value { get; init; }
}
