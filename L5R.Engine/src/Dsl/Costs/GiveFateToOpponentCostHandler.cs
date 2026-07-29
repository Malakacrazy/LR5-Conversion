using System.Text.Json;
using L5R.Engine.Abilities;

namespace L5R.Engine.Dsl.Costs;

/// <summary>ringteki costs.js giveFateToOpponent: `new GameActionCost(GameActions.takeFate({target: context.player, amount}))` - TransferFateAction with target=self, so the payer's fate moves to their opponent (a real transfer, not a payFate-style loss into nowhere).</summary>
public sealed class GiveFateToOpponentCostHandler : ICostHandler
{
    public bool CanPay(AbilityContext context, JsonElement? parameters) =>
        context.Player.Fate >= Amount(parameters);

    public void Pay(AbilityContext context, JsonElement? parameters)
    {
        var amount = Amount(parameters);
        context.Player.Fate -= amount;
        context.Game.Opponent(context.Player).Fate += amount;
    }

    private static int Amount(JsonElement? parameters) =>
        parameters?.TryGetProperty("amount", out var amountElement) == true ? amountElement.GetInt32() : 1;
}
