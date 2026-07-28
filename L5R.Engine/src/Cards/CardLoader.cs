using System.Text.Json;

namespace L5R.Engine.Cards;

/// <summary>
/// Card-load-time validation: every effect/gameAction/cost name referenced in the
/// document must be registered, and a scriptOverride's handler must resolve to a real
/// ICardScript implementation. This is deliberately just validation-and-cataloging for
/// now, not full runtime instantiation into the task-8 state model - that needs the
/// state model to grow (skills, zones, attachments) before it means anything.
/// </summary>
public sealed class CardLoader
{
    private readonly NameRegistry _effects;
    private readonly NameRegistry _gameActions;
    private readonly NameRegistry _costs;

    public CardLoader(NameRegistry effects, NameRegistry gameActions, NameRegistry costs)
    {
        _effects = effects;
        _gameActions = gameActions;
        _costs = costs;
    }

    public LoadedCard Load(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        var id = root.GetProperty("id").GetString()!;
        var name = root.GetProperty("name").GetString()!;
        var type = root.GetProperty("type").GetString()!;

        var references = root.TryGetProperty("abilities", out var abilities)
            ? CardReferenceCollector.Collect(abilities)
            : Array.Empty<CollectedReference>();

        foreach (var reference in references)
            Validate(id, reference);

        var scriptOverride = root.TryGetProperty("scriptOverride", out var overrideElement)
            ? LoadScriptOverride(id, overrideElement)
            : null;

        return new LoadedCard(id, name, type, references, scriptOverride);
    }

    private void Validate(string cardId, CollectedReference reference)
    {
        var registry = reference.Kind switch
        {
            "effect" => _effects,
            "gameAction" => _gameActions,
            "cost" => _costs,
            _ => throw new CardLoadException($"Card '{cardId}': unrecognized reference kind '{reference.Kind}'.")
        };

        if (!registry.IsRegistered(reference.Name))
            throw new CardLoadException($"Card '{cardId}' references unknown {reference.Kind} '{reference.Name}'.");
    }

    private static ScriptOverrideInfo LoadScriptOverride(string cardId, JsonElement overrideElement)
    {
        var reason = overrideElement.GetProperty("reason").GetString()!;
        var handlerName = overrideElement.GetProperty("handler").GetString()!;

        var handlerType = Type.GetType(handlerName)
            ?? throw new CardLoadException($"Card '{cardId}': scriptOverride handler '{handlerName}' could not be resolved.");

        if (!typeof(ICardScript).IsAssignableFrom(handlerType))
            throw new CardLoadException($"Card '{cardId}': scriptOverride handler '{handlerName}' does not implement ICardScript.");

        return new ScriptOverrideInfo(reason, handlerType);
    }
}
