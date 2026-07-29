using System.Linq;
using System.Text.Json;

namespace L5R.Engine.Dsl;

/// <summary>
/// Parses a loaded card's abilities.actions[] into executable ActionDefinitions. Separate
/// from CardLoader (which only validates shape/names and resolves scriptOverride) -
/// CardLoader answers "is this card well-formed", this answers "what does running its
/// first action actually mean". Only understands the subset of card-schema.json exercised
/// by cards ported into the executable set so far; unsupported shapes throw rather than
/// silently drop behavior.
/// </summary>
public static class AbilityDefinitionParser
{
    public static IReadOnlyList<ActionDefinition> ParseActions(JsonElement cardRoot)
    {
        if (!cardRoot.TryGetProperty("abilities", out var abilities))
            return Array.Empty<ActionDefinition>();

        if (!abilities.TryGetProperty("actions", out var actions))
            return Array.Empty<ActionDefinition>();

        var result = new List<ActionDefinition>();
        foreach (var action in actions.EnumerateArray())
            result.Add(ParseAction(action));

        return result;
    }

    public static IReadOnlyList<TriggeredAbilityDefinition> ParseTriggeredAbilities(JsonElement cardRoot)
    {
        if (!cardRoot.TryGetProperty("abilities", out var abilities))
            return Array.Empty<TriggeredAbilityDefinition>();

        if (!abilities.TryGetProperty("triggeredAbilities", out var triggeredAbilities))
            return Array.Empty<TriggeredAbilityDefinition>();

        var result = new List<TriggeredAbilityDefinition>();
        foreach (var ability in triggeredAbilities.EnumerateArray())
            result.Add(ParseTriggeredAbility(ability));

        return result;
    }

    public static IReadOnlyList<PersistentEffectDefinition> ParsePersistentEffects(JsonElement cardRoot)
    {
        if (!cardRoot.TryGetProperty("abilities", out var abilities))
            return Array.Empty<PersistentEffectDefinition>();

        if (!abilities.TryGetProperty("persistentEffects", out var persistentEffects))
            return Array.Empty<PersistentEffectDefinition>();

        var result = new List<PersistentEffectDefinition>();
        foreach (var entry in persistentEffects.EnumerateArray())
            result.Add(ParsePersistentEffect(entry));

        return result;
    }

    private static PersistentEffectDefinition ParsePersistentEffect(JsonElement entry)
    {
        // "match" is "self", a predicate, or omitted entirely - card-schema.json's own
        // documented convention for a player-scoped effect. A null Match here is that third
        // case, resolved via TargetController alone by ActivePersistentEffectsAffectingPlayer.
        JsonElement? match = entry.TryGetProperty("match", out var matchElement) ? matchElement.Clone() : null;

        var condition = entry.TryGetProperty("condition", out var conditionElement) ? conditionElement.Clone() : (JsonElement?)null;
        var targetController = entry.TryGetProperty("targetController", out var tc) ? tc.GetString()! : "self";
        var sourceLocation = entry.TryGetProperty("sourceLocation", out var sl) ? sl.GetString()! : "play area";

        var effectElement = entry.GetProperty("effect");
        var effects = effectElement.ValueKind == JsonValueKind.Array
            ? effectElement.EnumerateArray().Select(e => e.Clone()).ToList()
            : new List<JsonElement> { effectElement.Clone() };

        return new PersistentEffectDefinition(match, condition, targetController, sourceLocation, effects);
    }

    public static IReadOnlyList<WhileAttachedDefinition> ParseWhileAttached(JsonElement cardRoot)
    {
        if (!cardRoot.TryGetProperty("abilities", out var abilities))
            return Array.Empty<WhileAttachedDefinition>();

        if (!abilities.TryGetProperty("whileAttached", out var whileAttached))
            return Array.Empty<WhileAttachedDefinition>();

        var result = new List<WhileAttachedDefinition>();
        foreach (var entry in whileAttached.EnumerateArray())
            result.Add(ParseWhileAttachedEntry(entry));

        return result;
    }

    private static WhileAttachedDefinition ParseWhileAttachedEntry(JsonElement entry)
    {
        var match = entry.TryGetProperty("match", out var matchElement) ? matchElement.Clone() : (JsonElement?)null;
        var condition = entry.TryGetProperty("condition", out var conditionElement) ? conditionElement.Clone() : (JsonElement?)null;

        var effectElement = entry.GetProperty("effect");
        var effects = effectElement.ValueKind == JsonValueKind.Array
            ? effectElement.EnumerateArray().Select(e => e.Clone()).ToList()
            : new List<JsonElement> { effectElement.Clone() };

        return new WhileAttachedDefinition(match, condition, effects);
    }

    private static TriggeredAbilityDefinition ParseTriggeredAbility(JsonElement ability)
    {
        ThrowIfMultiTarget(ability);

        var trigger = ability.GetProperty("trigger").GetString()!;
        var title = ability.GetProperty("title").GetString()!;

        var when = ability.GetProperty("when").EnumerateObject().Single();
        var whenEvent = when.Name;
        var whenCondition = when.Value.Clone();

        var costs = ability.TryGetProperty("cost", out var costElement)
            ? ParseCosts(costElement)
            : Array.Empty<CostDefinition>();

        var target = ability.TryGetProperty("target", out var targetElement)
            ? ParseTarget(targetElement)
            : null;

        var gameActions = ability.TryGetProperty("gameAction", out var gameActionElement)
            ? ParseGameActions(gameActionElement)
            : Array.Empty<GameActionDefinition>();

        return new TriggeredAbilityDefinition(trigger, title, whenEvent, whenCondition, costs, target, gameActions);
    }

    /// <summary>
    /// card-schema.json's "targets" (plural, keyed by name - know-the-world's "returnedRing"/
    /// "takenRing") is a genuinely different shape from "target" (singular) - multi-target
    /// actions and ring-scoped predicates aren't supported yet. Without this guard,
    /// TryGetProperty("target", ...) would just miss it and silently parse Target as null,
    /// quietly dropping the whole ability instead of failing loud.
    /// </summary>
    private static void ThrowIfMultiTarget(JsonElement ability)
    {
        if (ability.TryGetProperty("targets", out _))
            throw new NotSupportedException("AbilityDefinitionParser does not yet support multi-target abilities ('targets', plural).");
    }

    private static ActionDefinition ParseAction(JsonElement action)
    {
        var title = action.GetProperty("title").GetString()!;

        var costs = action.TryGetProperty("cost", out var costElement)
            ? ParseCosts(costElement)
            : Array.Empty<CostDefinition>();

        var target = action.TryGetProperty("target", out var targetElement)
            ? ParseTarget(targetElement)
            : null;

        var gameActions = action.TryGetProperty("gameAction", out var gameActionElement)
            ? ParseGameActions(gameActionElement)
            : Array.Empty<GameActionDefinition>();

        JsonElement? condition = action.TryGetProperty("condition", out var conditionElement)
            ? conditionElement.Clone()
            : null;

        // ringteki CardAction.js: this.phase = properties.phase || 'any' - "any" means
        // unrestricted, so it's represented here as the absence of a restriction (null).
        string? phase = action.TryGetProperty("phase", out var phaseElement) && phaseElement.GetString() != "any"
            ? phaseElement.GetString()
            : null;

        IReadOnlyDictionary<string, RingTargetEntry>? targets = action.TryGetProperty("targets", out var targetsElement)
            ? ParseRingTargets(targetsElement)
            : null;

        return new ActionDefinition(title, costs, target, gameActions, condition, phase, targets);
    }

    /// <summary>
    /// card-schema.json's "targets" (plural, keyed by name - know-the-world's "returnedRing"/
    /// "takenRing") - only the "mode": "ring" shape is understood; anything else throws
    /// rather than silently dropping the ability's effect (see RingTargetEntry's doc comment).
    /// </summary>
    private static IReadOnlyDictionary<string, RingTargetEntry> ParseRingTargets(JsonElement targetsElement)
    {
        var result = new Dictionary<string, RingTargetEntry>();
        foreach (var entry in targetsElement.EnumerateObject())
        {
            var mode = entry.Value.TryGetProperty("mode", out var modeElement) ? modeElement.GetString() : null;
            if (mode != "ring")
                throw new NotSupportedException($"AbilityDefinitionParser only supports 'mode': 'ring' entries within a multi-target 'targets' block, got '{mode}' for '{entry.Name}'.");

            var ringCondition = entry.Value.GetProperty("ringCondition").Clone();
            var gameAction = ParseGameAction(entry.Value.GetProperty("gameAction"));
            result[entry.Name] = new RingTargetEntry(ringCondition, gameAction);
        }

        return result;
    }

    private static IReadOnlyList<CostDefinition> ParseCosts(JsonElement costElement)
    {
        if (costElement.ValueKind == JsonValueKind.Array)
        {
            var result = new List<CostDefinition>();
            foreach (var entry in costElement.EnumerateArray())
                result.Add(ParseCost(entry));
            return result;
        }

        return new[] { ParseCost(costElement) };
    }

    private static CostDefinition ParseCost(JsonElement entry)
    {
        var name = entry.GetProperty("name").GetString()!;
        JsonElement? paramsElement = entry.TryGetProperty("params", out var p) ? p.Clone() : null;
        return new CostDefinition(name, paramsElement);
    }

    private static TargetDefinition ParseTarget(JsonElement targetElement)
    {
        var mode = targetElement.TryGetProperty("mode", out var modeElement) ? modeElement.GetString() : "single";

        if (mode == "select")
        {
            var choices = new Dictionary<string, GameActionDefinition>();
            foreach (var choice in targetElement.GetProperty("choices").EnumerateObject())
                choices[choice.Name] = ParseGameAction(choice.Value);

            return new TargetDefinition(null, "any", null, Array.Empty<GameActionDefinition>(), choices);
        }

        if (mode != "single" && mode != "maxStat")
            throw new NotSupportedException($"AbilityDefinitionParser does not yet support target mode '{mode}'.");

        string? cardType = targetElement.TryGetProperty("cardType", out var cardTypeElement)
            ? cardTypeElement.GetString()
            : null;

        var controller = targetElement.TryGetProperty("controller", out var controllerElement)
            ? controllerElement.GetString()!
            : "any"; // card-schema.json default, matching ringteki's BaseCardSelector.js

        JsonElement? cardCondition = targetElement.TryGetProperty("cardCondition", out var cc)
            ? cc.Clone()
            : null;

        var gameActions = targetElement.TryGetProperty("gameAction", out var gameActionElement)
            ? ParseGameActions(gameActionElement)
            : Array.Empty<GameActionDefinition>();

        MaxStatSelection? maxStat = mode == "maxStat"
            ? new MaxStatSelection(
                targetElement.GetProperty("numCards").GetInt32(),
                targetElement.GetProperty("cardStat").GetString()!,
                targetElement.GetProperty("statBudget").Clone())
            : null;

        return new TargetDefinition(cardType, controller, cardCondition, gameActions, MaxStat: maxStat);
    }

    private static IReadOnlyList<GameActionDefinition> ParseGameActions(JsonElement gameActionElement)
    {
        if (gameActionElement.ValueKind == JsonValueKind.Array)
        {
            var result = new List<GameActionDefinition>();
            foreach (var entry in gameActionElement.EnumerateArray())
                result.Add(ParseGameAction(entry));
            return result;
        }

        return new[] { ParseGameAction(gameActionElement) };
    }

    private static GameActionDefinition ParseGameAction(JsonElement entry)
    {
        var name = entry.GetProperty("name").GetString()!;
        JsonElement? paramsElement = entry.TryGetProperty("params", out var p) ? p.Clone() : null;
        JsonElement? targetElement = entry.TryGetProperty("target", out var t) ? t.Clone() : null;
        return new GameActionDefinition(name, paramsElement, targetElement);
    }
}
