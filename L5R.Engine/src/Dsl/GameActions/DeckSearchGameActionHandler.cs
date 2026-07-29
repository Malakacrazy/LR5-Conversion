using System.Text.Json;
using L5R.Engine.Abilities;

namespace L5R.Engine.Dsl.GameActions;

/// <summary>
/// ringteki DeckSearchAction (agasha-swordsmith's "look at the top 5, take a matching
/// attachment"): a PlayerAction - operates on context.Player's own deck, not context.Target
/// (same convention as DrawGameActionHandler). No search/selection-menu UI exists, so the
/// caller supplies which card (if any - "Take nothing" is a legal choice) via
/// context.ChosenDeckSearchCard directly, and this only validates it: must be among the top
/// "amount" cards (amount &lt; 0 means the whole deck, ringteki's own default) and satisfy
/// "cardCondition" if present. Moves the chosen card to hand (ringteki's own default
/// destination - no ported card uses a different one yet). Ringteki always reshuffles the
/// deck afterward; this engine has no RNG/shuffle primitive, and no observable behavior
/// depends on deck order beyond "the top card" (DrawGameActionHandler), so there is nothing
/// further to do - the remaining cards are simply left in place.
/// </summary>
public sealed class DeckSearchGameActionHandler : IGameActionHandler
{
    public void Execute(AbilityContext context, JsonElement? parameters)
    {
        var amount = parameters?.TryGetProperty("amount", out var amountElement) == true
            ? amountElement.GetInt32()
            : -1;

        var chosen = context.ChosenDeckSearchCard;
        if (chosen is null)
            return;

        var player = context.Player;
        var pool = amount < 0 ? player.Deck : player.Deck.Take(amount);
        if (!pool.Contains(chosen))
            throw new InvalidOperationException($"'{chosen.Id}' is not among the searched cards.");

        if (parameters?.TryGetProperty("cardCondition", out var cardCondition) == true
            && !PredicateEvaluator.Evaluate(cardCondition, chosen, context))
            throw new InvalidOperationException($"'{chosen.Id}' does not satisfy the deckSearch's cardCondition.");

        ZoneMover.MoveTo(chosen, player.Hand, "hand");
    }
}
