using System.Text.Json;

namespace L5R.Engine.Dsl;

/// <summary>
/// One entry from a loaded card's abilities.actions[]. Deliberately thin: condition and
/// cardCondition are kept as raw JsonElement rather than parsed predicate trees, since no
/// predicate interpreter exists yet - AbilityExecutor throws loudly if it hits one it
/// can't evaluate rather than silently ignoring it. Grows as later card groups need
/// predicate evaluation, targets map, phase/limit checks, etc.
/// </summary>
public sealed record ActionDefinition(
    string Title,
    IReadOnlyList<CostDefinition> Costs,
    TargetDefinition? Target,
    IReadOnlyList<GameActionDefinition> GameActions,
    JsonElement? Condition,
    string? Phase);

public sealed record CostDefinition(string Name, JsonElement? Params);

public sealed record TargetDefinition(
    string? CardType,
    string Controller,
    JsonElement? CardCondition,
    IReadOnlyList<GameActionDefinition> GameActions);

public sealed record GameActionDefinition(string Name, JsonElement? Params);

/// <summary>
/// One entry from a loaded card's abilities.triggeredAbilities[] (reactions/interrupts).
/// Shares Costs/Target/GameActions with ActionDefinition, but is gated by a "when" clause
/// instead of a plain condition - {WhenEvent: WhenCondition} mirrors the JSON's single-key
/// `when: { eventName: predicate }` object. No event bus exists yet, so there's no way to
/// know an event actually happened; the caller (a test, for now) asserts it did by passing
/// the event's subject card directly to AbilityExecutor.ExecuteTriggered, and the predicate
/// is evaluated against that card exactly like a normal cardCondition would be.
/// Trigger ("reaction"/"interrupt"/"wouldInterrupt") is kept for documentation but not
/// enforced - there's no timing-window/priority system yet to make it meaningful.
/// </summary>
public sealed record TriggeredAbilityDefinition(
    string Trigger,
    string Title,
    string WhenEvent,
    JsonElement WhenCondition,
    IReadOnlyList<CostDefinition> Costs,
    TargetDefinition? Target,
    IReadOnlyList<GameActionDefinition> GameActions);
