using L5R.Engine.Dsl;

namespace L5R.Engine.Abilities;

/// <summary>
/// Bridges a JSON-parsed TargetDefinition into the Abilities.ITargetRequirement shape
/// CardAction.MeetsRequirements checks. Delegates directly to TargetResolver.ResolveLegalTargets,
/// which already does exactly "does at least one legal candidate exist right now" - existing,
/// reused as-is.
/// </summary>
public sealed class JsonActionTarget : ITargetRequirement
{
    private readonly TargetDefinition _definition;

    public JsonActionTarget(TargetDefinition definition) => _definition = definition;

    public bool HasLegalTarget(AbilityContext context) =>
        TargetResolver.ResolveLegalTargets(_definition, context).Count > 0;
}
