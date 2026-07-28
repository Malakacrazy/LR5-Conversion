using System.Text.Json;
using L5R.Engine.Abilities;

namespace L5R.Engine.Dsl.GameActions;

/// <summary>
/// ringteki GameActions.ts draw: the acting player draws from their deck, not a card
/// target - context.Player, not context.Target (ringteki's own default target for draw).
/// </summary>
public sealed class DrawGameActionHandler : IGameActionHandler
{
    public void Execute(AbilityContext context, JsonElement? parameters)
    {
        var amount = parameters?.TryGetProperty("amount", out var amountElement) == true
            ? amountElement.GetInt32()
            : 1;

        var player = context.Player;
        for (var i = 0; i < amount && player.Deck.Count > 0; i++)
        {
            var top = player.Deck[0];
            ZoneMover.MoveTo(top, player.Hand, "hand");
        }
    }
}
