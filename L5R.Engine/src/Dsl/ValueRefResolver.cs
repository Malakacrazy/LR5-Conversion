using System.Text.Json;
using L5R.Engine.Abilities;

namespace L5R.Engine.Dsl;

/// <summary>
/// Resolves one card-schema.json valueRef to a literal int. Deliberately covers only
/// plain number literals so far - contextPath/dynamic/allCardsMatching all throw until a
/// card in the executable set actually needs one, per the same fail-loud policy as
/// PredicateEvaluator.
/// </summary>
public static class ValueRefResolver
{
    public static int ResolveInt(JsonElement valueRef, AbilityContext context)
    {
        if (valueRef.ValueKind == JsonValueKind.Number)
            return valueRef.GetInt32();

        throw new NotSupportedException("ValueRefResolver only supports plain number literals so far.");
    }
}
