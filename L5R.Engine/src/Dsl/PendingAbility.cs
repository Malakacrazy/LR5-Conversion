using L5R.Engine.Abilities;
using L5R.Engine.State;

namespace L5R.Engine.Dsl;

/// <summary>
/// An ability whose costs have been paid but whose effects haven't run yet - the "event"
/// half of ringteki's Event/EventWindow split (see Event.js's `cancelled` flag), scoped
/// down to exactly what AbilityExecutor.Resolve needs to finish the job. No interrupt-
/// window scheduler exists to create and cancel these automatically; a test (or future
/// caller) creates one via AbilityExecutor.Prepare/PrepareTriggered, optionally cancels it
/// by running a "cancel" gameAction against it (see CancelGameActionHandler and
/// AbilityContext.InterruptedAbility), then calls Resolve.
/// </summary>
public sealed class PendingAbility
{
    public required string Title { get; init; }
    public required TargetDefinition? Target { get; init; }
    public required IReadOnlyList<GameActionDefinition> GameActions { get; init; }
    public required AbilityContext Context { get; init; }

    /// <summary>ActionDefinition.Targets (know-the-world's multi-ring "targets" shape) - null for every other card, which uses the singular Target instead.</summary>
    public IReadOnlyDictionary<string, RingTargetEntry>? Targets { get; init; }

    // Carried forward from Prepare's caller so Resolve can run the same target-resolution
    // branches (single target / "mode": "select" / "mode": "maxStat") Execute already
    // supported before this Prepare/Resolve split existed.
    public Card? ChosenTarget { get; init; }
    public string? ChosenChoice { get; init; }
    public IReadOnlyList<Card>? ChosenTargets { get; init; }

    /// <summary>Which Ring fills each named slot in Targets - no ring-selection UI exists, so the caller supplies these directly, same convention as ChosenTarget.</summary>
    public IReadOnlyDictionary<string, Ring>? ChosenRingTargets { get; init; }

    public bool Cancelled { get; private set; }

    public void Cancel() => Cancelled = true;
}
