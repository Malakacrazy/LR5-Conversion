using System.Text.Json;
using L5R.Engine.Abilities;

namespace L5R.Engine.Dsl.Costs;

/// <summary>ringteki costs.js sacrificeSelf: discard the source card as a cost.</summary>
public sealed class SacrificeSelfCostHandler : ICostHandler
{
    public bool CanPay(AbilityContext context, JsonElement? parameters) =>
        context.Source.Location == "play area";

    public void Pay(AbilityContext context, JsonElement? parameters) =>
        ZoneMover.MoveTo(context.Source, context.Source.Controller.Discard, "discard");
}
