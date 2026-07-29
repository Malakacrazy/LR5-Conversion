using System.Text.Json;
using L5R.Engine.Abilities;

namespace L5R.Engine.Dsl.Costs;

/// <summary>
/// ringteki costs.js payFateToRing: `MetaActionCost(GameActions.selectRing({ringCondition:
/// ring => ring.isUnclaimed(), gameAction: placeFateOnRing({amount, origin: context.player})}))` -
/// select an unclaimed ring and move `amount` fate from the player's own pool onto it. No
/// ring-selection UI exists, so the caller supplies the chosen ring via
/// context.CostRingTarget, same convention as CostTarget for card-based parameterized costs.
/// </summary>
public sealed class PayFateToRingCostHandler : ICostHandler
{
    public bool CanPay(AbilityContext context, JsonElement? parameters) =>
        context.Player.Fate >= Amount(parameters) && context.Game.Rings.Any(r => r.IsUnclaimed);

    public void Pay(AbilityContext context, JsonElement? parameters)
    {
        var ring = context.CostRingTarget
            ?? throw new InvalidOperationException("payFateToRing cost requires a chosen ring but none was supplied.");

        if (!ring.IsUnclaimed)
            throw new InvalidOperationException($"'{ring.Element}' ring is not unclaimed - payFateToRing requires an unclaimed ring.");

        var amount = Amount(parameters);
        context.Player.Fate -= amount;
        ring.Fate += amount;
    }

    private static int Amount(JsonElement? parameters) =>
        parameters?.TryGetProperty("amount", out var amountElement) == true ? amountElement.GetInt32() : 1;
}
