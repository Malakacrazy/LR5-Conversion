using L5R.Engine.Abilities;
using L5R.Engine.State;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// shrewd-yasuki: while participating and a revealed (non-facedown) holding sits in either
/// player's provinces, look at the top 2 cards of the deck, take one to hand, and put the
/// other on the bottom. No "look at N, keep one, bottom the rest" gameAction exists (every
/// other deckSearch-shaped card leaves the unchosen cards in place) - matches this card's
/// own scriptOverride reason. context.ChosenDeckSearchCard carries the kept card, same
/// convention as DeckSearchGameActionHandler.
/// </summary>
public sealed class ShrewdYasukiLookAtTopTwoKeepOne : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var yasuki = context.Source;

        if (!IsParticipating(context.Game, yasuki))
            throw new InvalidOperationException($"'{yasuki.Id}' can only be used while participating.");

        var revealedHoldingInPlay = context.Game.Player1.Provinces.Concat(context.Game.Player2.Provinces)
            .Any(c => c.Type == CardType.Holding && !c.Facedown);
        if (!revealedHoldingInPlay)
            throw new InvalidOperationException($"'{yasuki.Id}' requires a revealed holding in either player's provinces.");

        var player = context.Player;
        if (player.Deck.Count == 0)
            throw new InvalidOperationException($"'{player.Name}' has no cards left in their deck.");

        var pool = player.Deck.Take(2).ToList();
        var chosen = context.ChosenDeckSearchCard
            ?? throw new InvalidOperationException($"'{yasuki.Id}' requires context.ChosenDeckSearchCard to be set.");

        if (!pool.Contains(chosen))
            throw new InvalidOperationException($"'{chosen.Id}' is not among the top 2 cards.");

        player.Deck.Remove(chosen);
        player.Hand.Add(chosen);
        chosen.Location = "hand";

        foreach (var other in pool.Where(c => c != chosen))
        {
            player.Deck.Remove(other);
            player.Deck.Add(other);
        }
    }

    private static bool IsParticipating(GameState game, Card card) =>
        game.CurrentConflict is { } conflict && (conflict.Attackers.Contains(card) || conflict.Defenders.Contains(card));
}
