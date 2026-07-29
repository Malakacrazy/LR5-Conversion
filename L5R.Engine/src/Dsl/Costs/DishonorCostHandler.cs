using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.State;

namespace L5R.Engine.Dsl.Costs;

/// <summary>
/// ringteki costs.js dishonor({cardType, cardCondition}): `getSelectCost(GameActions.dishonor(), ...)` -
/// the same MetaActionCost-wrapping-a-GameAction shape as sacrifice/discardCard, so
/// controller is hardcoded to "self" here too (see SacrificeCostHandler's doc comment).
/// DishonorAction.canAffect requires play-area location, character type, and not already
/// dishonored - forged-edict's own cardCondition only adds "hasTrait courtier" on top of
/// that, so those three base requirements are filtered here rather than duplicated per card.
/// Pay reuses DishonorGameActionHandler.Execute (including its
/// IsRestrictedFrom(..., "receiveDishonorToken") check) rather than re-implementing dishonor.
/// </summary>
public sealed class DishonorCostHandler : ICostHandler
{
    public bool CanPay(AbilityContext context, JsonElement? parameters) =>
        ResolveLegalCandidates(context, parameters).Any();

    public void Pay(AbilityContext context, JsonElement? parameters)
    {
        var target = context.CostTarget
            ?? throw new InvalidOperationException("dishonor cost requires a chosen cost target but none was supplied.");

        context.Target = target;
        new DishonorGameActionHandler().Execute(context, parameters: null);
    }

    private static IReadOnlyList<Card> ResolveLegalCandidates(AbilityContext context, JsonElement? parameters)
    {
        if (parameters is null)
            throw new InvalidOperationException("dishonor cost requires params (at least cardCondition or cardType).");

        var props = parameters.Value;
        var cardType = props.TryGetProperty("cardType", out var cardTypeElement) ? cardTypeElement.GetString() : "character";
        JsonElement? cardCondition = props.TryGetProperty("cardCondition", out var cc) ? cc.Clone() : null;

        var target = new TargetDefinition(cardType, "self", cardCondition, Array.Empty<GameActionDefinition>());
        return TargetResolver.ResolveLegalTargets(target, context)
            .Where(card => card.Location == "play area" && !card.IsDishonored)
            .ToList();
    }
}
