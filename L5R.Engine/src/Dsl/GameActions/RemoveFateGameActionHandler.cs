using System.Text.Json;
using L5R.Engine.Abilities;

namespace L5R.Engine.Dsl.GameActions;

/// <summary>
/// ringteki RemoveFateAction: removes fate from context.Target - amount defaults to 1. No
/// recipient tracking (optional in ringteki, unused by any ported card so far). Checks
/// IsRestrictedFrom "removeFate" (steadfast-samurai's own cardCannot) - previously
/// unchecked here since no ported card needed to block this action before now.
/// </summary>
public sealed class RemoveFateGameActionHandler : IGameActionHandler
{
    public void Execute(AbilityContext context, JsonElement? parameters)
    {
        if (context.Target is null)
            throw new InvalidOperationException("removeFate requires context.Target to be set.");

        if (context.Game.IsRestrictedFrom(context.Target, "removeFate", context.Source))
            throw new InvalidOperationException($"'{context.Target.Id}' cannot have fate removed.");

        var amount = parameters?.TryGetProperty("amount", out var amountElement) == true
            ? amountElement.GetInt32()
            : 1;

        context.Target.Fate = Math.Max(0, context.Target.Fate - amount);
    }
}
