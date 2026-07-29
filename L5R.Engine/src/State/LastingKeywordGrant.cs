using System.Text.Json;

namespace L5R.Engine.State;

/// <summary>
/// One active cardLastingEffect "addKeyword" grant (adept-of-the-waves' "give a character
/// Covert while it's a water conflict") - the one-shot-application counterpart to
/// PersistentEffectDefinition/WhileAttachedDefinition's own addKeyword scans
/// (GameState.HasAddEffect), which only see grants still attached to a card's own
/// persistentEffects/whileAttached list, not something an ability applied once and walked
/// away from. Duration/expiry mirrors LastingEffect exactly (see its own doc comment).
/// </summary>
public sealed class LastingKeywordGrant
{
    public required Card Target { get; init; }
    public required string Keyword { get; init; }
    public required string Duration { get; init; }

    /// <summary>Re-checked live on every GameState.HasKeyword query, same convention as CardRestriction.Condition - adept-of-the-waves' grant only applies "during a water conflict", not just up until Duration expires.</summary>
    public JsonElement? Condition { get; init; }
}
