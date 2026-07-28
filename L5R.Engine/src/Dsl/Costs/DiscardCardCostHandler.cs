using System.Text.Json;
using L5R.Engine.Abilities;

namespace L5R.Engine.Dsl.Costs;

/// <summary>
/// ringteki costs.js discardCard({cardType, cardCondition, location}): another parameterized
/// MetaActionCost, like sacrifice - controller is hardcoded to Players.Self by
/// MetaActionCost.ts regardless of params, and costs.js's own factory merges in
/// `location: Locations.Hand` as the default before MetaActionCost ever sees it (unlike
/// sacrifice, which has no such default and instead relies on DiscardFromPlayAction's own
/// canAffect to require play area - see SacrificeCostHandler). Every ported card gives
/// "hand" explicitly, so only that location is supported; anything else throws.
/// </summary>
public sealed class DiscardCardCostHandler : ICostHandler
{
    public bool CanPay(AbilityContext context, JsonElement? parameters) =>
        ResolveLegalCandidates(context, parameters).Any();

    public void Pay(AbilityContext context, JsonElement? parameters)
    {
        var target = context.CostTarget
            ?? throw new InvalidOperationException("discardCard cost requires a chosen cost target but none was supplied.");

        ZoneMover.MoveTo(target, target.Controller.Discard, "discard");
    }

    private static IReadOnlyList<Engine.State.Card> ResolveLegalCandidates(AbilityContext context, JsonElement? parameters)
    {
        string? cardType = null;
        JsonElement? cardCondition = null;

        if (parameters is not null)
        {
            var props = parameters.Value;
            var location = props.TryGetProperty("location", out var locationElement) ? locationElement.GetString() : "hand";
            if (location != "hand")
                throw new NotSupportedException($"DiscardCardCostHandler does not yet support location '{location}'.");

            cardType = props.TryGetProperty("cardType", out var cardTypeElement) ? cardTypeElement.GetString() : null;
            cardCondition = props.TryGetProperty("cardCondition", out var cc) ? cc.Clone() : null;
        }

        var target = new TargetDefinition(cardType, "self", cardCondition, Array.Empty<GameActionDefinition>());
        return TargetResolver.ResolveLegalTargets(target, context)
            .Where(card => card.Location == "hand")
            .ToList();
    }
}
