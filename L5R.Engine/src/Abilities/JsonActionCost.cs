using L5R.Engine.Dsl;

namespace L5R.Engine.Abilities;

/// <summary>
/// Bridges a JSON-parsed CostDefinition (AbilityExecutor's own cost representation) into
/// the Abilities.ICost shape CardAction.MeetsRequirements checks - the "can this even be
/// attempted right now" pre-check, distinct from ICostHandler.Pay which actually mutates
/// state once an action is chosen. Delegates to the same CostRegistry/ICostHandler.CanPay
/// AbilityExecutor.Prepare itself uses, so the two never disagree about affordability.
/// </summary>
public sealed class JsonActionCost : ICost
{
    private readonly CostDefinition _definition;
    private readonly CostRegistry _costRegistry;

    public JsonActionCost(CostDefinition definition, CostRegistry costRegistry)
    {
        _definition = definition;
        _costRegistry = costRegistry;
    }

    public bool CanPay(AbilityContext context) =>
        _costRegistry.Resolve(_definition.Name).CanPay(context, _definition.Params);
}
