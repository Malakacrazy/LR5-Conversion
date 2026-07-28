using System.Text.Json;
using L5R.Engine.Abilities;

namespace L5R.Engine.Dsl.Costs;

/// <summary>ringteki costs.js payHonor: pay honor from your own total as a cost.</summary>
public sealed class PayHonorCostHandler : ICostHandler
{
    public bool CanPay(AbilityContext context, JsonElement? parameters) =>
        context.Player.Honor >= Amount(parameters);

    public void Pay(AbilityContext context, JsonElement? parameters) =>
        context.Player.Honor -= Amount(parameters);

    private static int Amount(JsonElement? parameters) =>
        parameters?.TryGetProperty("amount", out var amountElement) == true ? amountElement.GetInt32() : 1;
}
