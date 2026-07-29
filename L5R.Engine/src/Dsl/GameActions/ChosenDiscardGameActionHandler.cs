using System.Text.Json;
using L5R.Engine.Abilities;

namespace L5R.Engine.Dsl.GameActions;

/// <summary>
/// ringteki ChosenDiscardAction (a PlayerAction - defaultTargets is [context.player.opponent],
/// no explicit target on any ported card overrides that): makes the target player discard
/// exactly min(hand size, amount) cards of their own choosing (or the ability's controller's,
/// for restoration-of-balance-style "look at their hand" effects - not modeled, no ported
/// card needs it). No selection UI exists, so the caller supplies which cards were chosen via
/// context.ChosenDiscardCards, same convention as ChosenRingElement.
/// </summary>
public sealed class ChosenDiscardGameActionHandler : IGameActionHandler
{
    public void Execute(AbilityContext context, JsonElement? parameters)
    {
        if (parameters is null || !parameters.Value.TryGetProperty("amount", out var amountRef))
            throw new InvalidOperationException("chosenDiscard requires params.amount.");

        var amount = ValueRefResolver.ResolveInt(amountRef, context);
        var targetPlayer = context.Game.Opponent(context.Player);
        var actualAmount = Math.Max(0, Math.Min(targetPlayer.Hand.Count, amount));

        if (actualAmount == 0)
            return;

        var chosen = context.ChosenDiscardCards
            ?? throw new InvalidOperationException("chosenDiscard requires context.ChosenDiscardCards to be set.");

        if (chosen.Count != actualAmount)
            throw new InvalidOperationException($"chosenDiscard requires exactly {actualAmount} chosen card(s), got {chosen.Count}.");

        foreach (var card in chosen)
        {
            if (card.Controller != targetPlayer || card.Location != "hand")
                throw new InvalidOperationException($"'{card.Id}' is not a legal chosenDiscard candidate (must be in {targetPlayer.Name}'s hand).");

            ZoneMover.MoveTo(card, targetPlayer.Discard, "discard");
        }
    }
}
