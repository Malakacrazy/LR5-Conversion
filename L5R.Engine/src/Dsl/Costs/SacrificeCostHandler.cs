using System.Text.Json;
using L5R.Engine.Abilities;

namespace L5R.Engine.Dsl.Costs;

/// <summary>
/// ringteki costs.js sacrifice({cardType, cardCondition}): a parameterized MetaActionCost
/// built on GameActions.selectCard. Unlike a regular target (BaseCardSelector.js defaults
/// controller to Players.Any), MetaActionCost.ts hardcodes controller: Players.Self in both
/// canPay and addEventsToArray regardless of the properties passed in - so this always
/// restricts to the acting player's own cards, matching printed "(friendly)" cost text like
/// funeral-pyre's, even though its params carry no controller field at all.
///
/// GameActions.sacrifice() is literally `new DiscardFromPlayAction(propertyFactory, true)` -
/// the same action class as the "discardFromPlay" gameAction - and its canAffect requires
/// card.location === Locations.PlayArea (for non-Holding cards; Holdings check a province
/// location instead, not modeled here since no ported sacrifice cost targets a Holding yet).
/// So "sacrifice" can only ever select a card already in play, regardless of what the
/// selector's own location default would otherwise be.
/// </summary>
public sealed class SacrificeCostHandler : ICostHandler
{
    public bool CanPay(AbilityContext context, JsonElement? parameters) =>
        ResolveLegalCandidates(context, parameters).Any();

    public void Pay(AbilityContext context, JsonElement? parameters)
    {
        var target = context.CostTarget
            ?? throw new InvalidOperationException("sacrifice cost requires a chosen cost target but none was supplied.");

        ZoneMover.MoveTo(target, target.Controller.Discard, "discard");
    }

    private static IReadOnlyList<Engine.State.Card> ResolveLegalCandidates(AbilityContext context, JsonElement? parameters)
    {
        if (parameters is null)
            throw new InvalidOperationException("sacrifice cost requires params (at least cardType).");

        var props = parameters.Value;
        var cardType = props.TryGetProperty("cardType", out var cardTypeElement) ? cardTypeElement.GetString() : null;
        JsonElement? cardCondition = props.TryGetProperty("cardCondition", out var cc) ? cc.Clone() : null;

        var target = new TargetDefinition(cardType, "self", cardCondition, Array.Empty<GameActionDefinition>());
        return TargetResolver.ResolveLegalTargets(target, context)
            .Where(card => card.Location == "play area")
            .ToList();
    }
}
