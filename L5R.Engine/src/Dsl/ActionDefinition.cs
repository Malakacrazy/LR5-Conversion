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
