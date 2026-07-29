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
        if (!entry.TryGetProperty("match", out var matchElement))
            throw new NotSupportedException("PersistentEffectDefinition requires 'match' - player-scoped effects (schema: omit match entirely) aren't supported yet.");

        var condition = entry.TryGetProperty("condition", out var conditionElement) ? conditionElement.Clone() : (JsonElement?)null;
        var targetController = entry.TryGetProperty("targetController", out var tc) ? tc.GetString()! : "self";
        var sourceLocation = entry.TryGetProperty("sourceLocation", out var sl) ? sl.GetString()! : "play area";

        var effectElement = entry.GetProperty("effect");
        var effects = effectElement.ValueKind == JsonValueKind.Array
            ? effectElement.EnumerateArray().Select(e => e.Clone()).ToList()
            : new List<JsonElement> { effectElement.Clone() };

        return new PersistentEffectDefinition(matchElement.Clone(), condition, targetController, sourceLocation, effects);
    }

    private static TriggeredAbilityDefinition ParseTriggeredAbility(JsonElement ability)
    {
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

        return new ActionDefinition(title, costs, target, gameActions, condition, phase);
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

        return new TargetDefinition(cardType, controller, cardCondition, gameActions);
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
