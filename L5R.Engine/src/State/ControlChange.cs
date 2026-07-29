namespace L5R.Engine.State;

/// <summary>
/// One active takeControl effect (ringteki effects.js's `takeControl` static card effect -
/// card.controller resolves through it while active). Unlike LastingEffect/CardRestriction
/// (both summed/checked on demand, so "reverting" is just letting the entry expire),
/// Controller is a plain mutable field read directly by dozens of call sites across the
/// engine (TargetResolver, MoveToConflictGameActionHandler, ...) - a computed "effective
/// controller" overlay nothing else would consult would make takeControl hollow. So this
/// records what to restore, and GameState.EndConflict()/AdvancePhase() actively mutate
/// Controller (and move the card between PlayArea lists) back on expiry, the same direct-
/// mutation style already used for Bowed/IsHonored/IsDishonored.
/// </summary>
public sealed class ControlChange
{
    public required Card Target { get; init; }
    public required Player OriginalController { get; init; }
    public required string Duration { get; init; }
}
