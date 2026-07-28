using L5R.Engine.State;

namespace L5R.Engine.Dsl;

/// <summary>
/// Shared plumbing for cost/gameAction handlers that move a card between its
/// controller's zones (sacrificeSelf, returnToHand, discardFromPlay, ...). Removes from
/// whichever zone currently holds the card rather than trusting Card.Location, since
/// that's just a display/query field, not the source of truth.
/// </summary>
public static class ZoneMover
{
    public static void MoveTo(Card card, List<Card> destination, string newLocation)
    {
        var controller = card.Controller;
        controller.Hand.Remove(card);
        controller.PlayArea.Remove(card);
        controller.Discard.Remove(card);
        controller.Deck.Remove(card);

        destination.Add(card);
        card.Location = newLocation;
    }
}
