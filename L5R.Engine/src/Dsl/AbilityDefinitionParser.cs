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

        return new ActionDefinition(title, costs, target, gameActions, condition);
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
        return new GameActionDefinition(name, paramsElement);
    }
}
