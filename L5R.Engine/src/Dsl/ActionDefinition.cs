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

/// <summary>
/// Choices is non-null only for "mode": "select" targets (card-schema.json target.choices) -
/// a menu of named alternatives, each its own gameActionEntry, rather than a card to pick.
/// No selection UI exists, so the caller supplies the chosen label directly (same
/// ChosenX-parameter convention as ChosenTarget/ChosenCostTarget/ChosenRingElement).
/// CardType/Controller/CardCondition/GameActions are meaningless when Choices is set.
/// </summary>
public sealed record TargetDefinition(
    string? CardType,
    string Controller,
    JsonElement? CardCondition,
    IReadOnlyList<GameActionDefinition> GameActions,
    IReadOnlyDictionary<string, GameActionDefinition>? Choices = null,
    MaxStatSelection? MaxStat = null);

/// <summary>
/// "mode": "maxStat" - pick up to NumCards cards (0 means unlimited) matching CardType/
/// Controller/CardCondition, whose CardStat totals at most StatBudget (e.g. ambush's "up to
/// 2 Scorpion characters totaling printedCost 6 or less"). No selection UI exists to search
/// for a legal combination itself, so the caller supplies the chosen set directly (like
/// ChosenTarget) and AbilityExecutor just validates it against the count/condition/budget
/// constraints rather than solving the knapsack itself.
/// </summary>
public sealed record MaxStatSelection(int NumCards, string CardStat, JsonElement StatBudget);

/// <summary>
/// Target is card-schema.json gameActionEntry's own optional override (a sibling of
/// "name", not nested in "params") - "Overrides the default target (source card for card
/// actions, ...)". Only the "allCardsMatching" valueRef shape is understood so far (a bulk
/// target applied to every matching card independently, e.g. the-art-of-peace's "honor all
/// defenders"); other valueRef shapes (contextPath, a single dynamic, ...) throw.
/// </summary>
public sealed record GameActionDefinition(string Name, JsonElement? Params, JsonElement? Target = null);

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

/// <summary>
/// One entry from a loaded card's abilities.persistentEffects[] (ringteki
/// this.persistentEffect({...}), always Durations.Persistent - unlike cardLastingEffect,
/// there's no apply-once moment; GameState re-evaluates every entry from every in-play card
/// on demand each time a stat/restriction is queried, rather than tracking a materialized
/// list like LastingEffects/Restrictions. Match is the raw JsonElement for either shape the
/// schema allows: the string "self", or a predicate object evaluated against each
/// candidate card. Player-scoped effects (schema: "omit match entirely") aren't supported -
/// AbilityDefinitionParser throws rather than silently treating a missing match as an
/// always-active card effect.
/// </summary>
public sealed record PersistentEffectDefinition(
    JsonElement Match,
    JsonElement? Condition,
    string TargetController,
    string SourceLocation,
    IReadOnlyList<JsonElement> Effects);

/// <summary>
/// One entry from a loaded card's abilities.whileAttached[] - active only while the source
/// (an attachment) is attached to a character, and always applies to the parent it's
/// attached to (unlike PersistentEffectDefinition's Match/TargetController, there's no
/// separate scope to choose - "the card this is attached to" is the only recipient). Condition
/// is evaluated against the attachment itself; Match is an extra, optional condition on the
/// parent beyond "is the card this is attached to" (card-schema.json) - no ported card in the
/// executable set uses it yet.
/// </summary>
public sealed record WhileAttachedDefinition(
    JsonElement? Match,
    JsonElement? Condition,
    IReadOnlyList<JsonElement> Effects);
