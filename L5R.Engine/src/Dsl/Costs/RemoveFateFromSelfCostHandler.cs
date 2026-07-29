using System.Text.Json;
using L5R.Engine.Abilities;

namespace L5R.Engine.Dsl.Costs;

/// <summary>
/// ringteki costs.js removeFateFromSelf: `new GameActionCost(GameActions.removeFate())` with
/// no target override, so its default target is the source card. RemoveFateAction.canAffect
/// requires fate !== 0 (and play-area location, already guaranteed for a source paying its
/// own cost) - can't pay if the source has no fate to remove.
/// </summary>
public sealed class RemoveFateFromSelfCostHandler : ICostHandler
{
    public bool CanPay(AbilityContext context, JsonElement? parameters) =>
        context.Source.Fate > 0;

    public void Pay(AbilityContext context, JsonElement? parameters) =>
        context.Source.Fate--;
}
