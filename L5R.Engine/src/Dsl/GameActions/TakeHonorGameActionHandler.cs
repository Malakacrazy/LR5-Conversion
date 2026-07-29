using System.Text.Json;
using L5R.Engine.Abilities;

namespace L5R.Engine.Dsl.GameActions;

/// <summary>
/// ringteki TransferHonorAction (name "takeHonor"): a PlayerAction with no defaultTargets
/// override, so the target defaults to context.player.opponent. Unlike takeFate, this
/// transfer is NOT balanced - eventHandler independently does
/// `event.player.modifyHonor(-amount)` (floored at 0) and
/// `event.player.opponent.modifyHonor(amount)`, so context.Player always gains the full
/// amount even if the target didn't have that much honor to lose.
/// </summary>
public sealed class TakeHonorGameActionHandler : IGameActionHandler
{
    public void Execute(AbilityContext context, JsonElement? parameters)
    {
        var amount = parameters?.TryGetProperty("amount", out var amountElement) == true ? amountElement.GetInt32() : 1;
        var from = context.Game.Opponent(context.Player);

        from.Honor = Math.Max(0, from.Honor - amount);
        context.Player.Honor += amount;
    }
}
