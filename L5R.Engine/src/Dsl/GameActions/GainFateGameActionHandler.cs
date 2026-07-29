using System.Text.Json;
using L5R.Engine.Abilities;

namespace L5R.Engine.Dsl.GameActions;

/// <summary>ringteki GainFateAction: the acting player gains fate - context.Player, not a card target.</summary>
public sealed class GainFateGameActionHandler : IGameActionHandler
{
    public void Execute(AbilityContext context, JsonElement? parameters)
    {
        var amount = parameters?.TryGetProperty("amount", out var amountElement) == true
            ? amountElement.GetInt32()
            : 1;

        context.Player.Fate += amount;
    }
}
