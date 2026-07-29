using System.Text.Json;
using L5R.Engine.Abilities;

namespace L5R.Engine.Dsl.GameActions;

/// <summary>
/// ringteki ModifyBidAction (a PlayerAction - context.Player is the implicit default
/// target, not context.Target): adjusts context.Player.HonorBidModifier by amount
/// (default 1), in the given direction (default "increase"). "prompt" (let the player
/// choose increase or decrease) needs a player-choice prompt this engine doesn't have yet,
/// so only the two fixed directions are supported.
/// </summary>
public sealed class ModifyBidGameActionHandler : IGameActionHandler
{
    public void Execute(AbilityContext context, JsonElement? parameters)
    {
        var direction = parameters?.TryGetProperty("direction", out var directionElement) == true ? directionElement.GetString()! : "increase";
        var amount = parameters?.TryGetProperty("amount", out var amountElement) == true ? amountElement.GetInt32() : 1;

        context.Player.HonorBidModifier += direction switch
        {
            "increase" => amount,
            "decrease" => -amount,
            _ => throw new NotSupportedException($"ModifyBidGameActionHandler does not yet support direction '{direction}' (needs a player-choice prompt).")
        };
    }
}
