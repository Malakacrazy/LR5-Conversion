using System.Text.Json;

namespace L5R.Engine.Cards;

/// <summary>
/// Scans a card document's `abilities` subtree for every effect/gameAction/cost
/// reference, regardless of nesting depth - including inside a game action's free-form
/// `params` (e.g. cardLastingEffect nests an `effect` inside its params). Scoped to the
/// `abilities` element specifically so the card's own top-level `name` field (its display
/// name, not a DSL reference) never gets swept in.
/// </summary>
public static class CardReferenceCollector
{
    private static readonly string[] ReferenceKeys = { "effect", "gameAction", "cost" };

    public static IReadOnlyList<CollectedReference> Collect(JsonElement abilities)
    {
        var found = new List<CollectedReference>();
        Walk(abilities, found);
        return found;
    }

    private static void Walk(JsonElement element, List<CollectedReference> found)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    if (Array.IndexOf(ReferenceKeys, property.Name) >= 0)
                        CollectNamesFrom(property.Name, property.Value, found);

                    Walk(property.Value, found);
                }
                break;

            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                    Walk(item, found);
                break;
        }
    }

    private static void CollectNamesFrom(string kind, JsonElement value, List<CollectedReference> found)
    {
        switch (value.ValueKind)
        {
            case JsonValueKind.Object:
                if (value.TryGetProperty("name", out var nameProperty) && nameProperty.ValueKind == JsonValueKind.String)
                    found.Add(new CollectedReference(kind, nameProperty.GetString()!));
                break;

            case JsonValueKind.Array:
                foreach (var item in value.EnumerateArray())
                    CollectNamesFrom(kind, item, found);
                break;
        }
    }
}
