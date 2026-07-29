using System.Text.Json;
using L5R.Engine.Abilities;

namespace L5R.Engine.Dsl.Costs;

/// <summary>ringteki costs.js payFate: `new GameActionCost(GameActions.loseFate({target: context.player, amount}))` - pay fate from your own total as a cost, mirroring PayHonorCostHandler.</summary>
public sealed class PayFateCostHandler : ICostHandler
{
    public bool CanPay(AbilityContext context, JsonElement? parameters) =>
        context.Player.Fate >= Amount(parameters);

    public void Pay(AbilityContext context, JsonElement? parameters) =>
        context.Player.Fate -= Amount(parameters);

    private static int Amount(JsonElement? parameters) =>
        parameters?.TryGetProperty("amount", out var amountElement) == true ? amountElement.GetInt32() : 1;
}
