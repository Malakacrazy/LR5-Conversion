using System.Text.Json;
using L5R.Engine.Abilities;

namespace L5R.Engine.Dsl.Costs;

/// <summary>ringteki costs.js bowSelf: bow the source card as a cost. Can't pay if already bowed.</summary>
public sealed class BowSelfCostHandler : ICostHandler
{
    public bool CanPay(AbilityContext context, JsonElement? parameters) =>
        context.Source is { Bowed: false };

    public void Pay(AbilityContext context, JsonElement? parameters) =>
        context.Source.Bowed = true;
}
