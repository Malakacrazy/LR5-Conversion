using System.Text.Json;
using L5R.Engine.Abilities;

namespace L5R.Engine.Dsl.GameActions;

/// <summary>ringteki GainHonorAction: a PlayerAction whose defaultTargets override is [context.player] (unlike the PlayerAction base default of the opponent) - the acting player gains honor.</summary>
public sealed class GainHonorGameActionHandler : IGameActionHandler
{
    public void Execute(AbilityContext context, JsonElement? parameters)
    {
        var amount = parameters?.TryGetProperty("amount", out var amountElement) == true ? amountElement.GetInt32() : 1;
        context.Player.Honor += amount;
    }
}
