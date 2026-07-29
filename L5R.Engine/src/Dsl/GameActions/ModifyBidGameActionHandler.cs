using System.Text.Json;
using L5R.Engine.Abilities;

namespace L5R.Engine.Dsl.GameActions;

/// <summary>
/// ringteki ModifyBidAction (a PlayerAction - context.Player is the implicit default
/// target, not context.Target): adjusts context.Player.HonorBidModifier by amount
/// (default 1), in the given direction (default "increase"). "prompt" (let the player
/// choose) has no choice-prompt UI, so it reads context.ChosenBidDirection instead - same
/// caller-supplies-the-choice convention as ChosenRingElement.
/// </summary>
public sealed class ModifyBidGameActionHandler : IGameActionHandler
{
    public void Execute(AbilityContext context, JsonElement? parameters)
    {
        var direction = parameters?.TryGetProperty("direction", out var directionElement) == true ? directionElement.GetString()! : "increase";
        if (direction == "prompt")
            direction = context.ChosenBidDirection
                ?? throw new InvalidOperationException("modifyBid's 'prompt' direction requires a chosen direction but none was supplied.");

        var amount = parameters?.TryGetProperty("amount", out var amountElement) == true ? amountElement.GetInt32() : 1;

        context.Player.HonorBidModifier += direction switch
        {
            "increase" => amount,
            "decrease" => -amount,
            _ => throw new NotSupportedException($"ModifyBidGameActionHandler does not yet support direction '{direction}'.")
        };
    }
}
