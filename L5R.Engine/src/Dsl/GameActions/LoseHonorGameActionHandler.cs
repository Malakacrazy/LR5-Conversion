using System.Text.Json;
using L5R.Engine.Abilities;

namespace L5R.Engine.Dsl.GameActions;

/// <summary>ringteki LoseHonorAction: a PlayerAction with no defaultTargets override, so it inherits the base PlayerAction default of context.player.opponent. ringteki player.js's modifyHonor floors at 0.</summary>
public sealed class LoseHonorGameActionHandler : IGameActionHandler
{
    public void Execute(AbilityContext context, JsonElement? parameters)
    {
        var amount = parameters?.TryGetProperty("amount", out var amountElement) == true ? amountElement.GetInt32() : 1;
        var target = context.Game.Opponent(context.Player);
        target.Honor = Math.Max(0, target.Honor - amount);
    }
}
