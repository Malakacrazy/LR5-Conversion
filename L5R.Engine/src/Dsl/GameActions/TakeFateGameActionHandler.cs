using System.Text.Json;
using L5R.Engine.Abilities;

namespace L5R.Engine.Dsl.GameActions;

/// <summary>
/// ringteki TransferFateAction (name "takeFate"): a PlayerAction with no defaultTargets
/// override, so the target defaults to context.player.opponent - moves fate from that
/// target to context.Player. A real transfer (GameAction.moveFateEventHandler clamps the
/// moved amount to what the origin actually has: `Math.min(amount, origin.fate)`), unlike
/// takeHonor's transfer which is NOT balanced this way.
/// </summary>
public sealed class TakeFateGameActionHandler : IGameActionHandler
{
    public void Execute(AbilityContext context, JsonElement? parameters)
    {
        var amount = parameters?.TryGetProperty("amount", out var amountElement) == true ? amountElement.GetInt32() : 1;
        var from = context.Game.Opponent(context.Player);
        var actualAmount = Math.Min(amount, from.Fate);

        from.Fate -= actualAmount;
        context.Player.Fate += actualAmount;
    }
}
