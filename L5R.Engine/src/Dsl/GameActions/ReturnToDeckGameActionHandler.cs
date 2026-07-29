using System.Text.Json;
using L5R.Engine.Abilities;

namespace L5R.Engine.Dsl.GameActions;

/// <summary>
/// ringteki ReturnToDeckAction: returns context.Target to the top of its controller's deck
/// (ringteki's own default: bottom: false, shuffle: false - only the default case is
/// modeled, since no ported card needs bottom/shuffle yet). Duplicates ZoneMover's zone-
/// clearing lines rather than reusing it, since ZoneMover.MoveTo only appends to the
/// destination list - this needs to insert at index 0 (the top), a different enough
/// operation not to force into that shared helper's append-only shape.
/// </summary>
public sealed class ReturnToDeckGameActionHandler : IGameActionHandler
{
    public void Execute(AbilityContext context, JsonElement? parameters)
    {
        if (context.Target is null)
            throw new InvalidOperationException("returnToDeck requires context.Target to be set.");

        var card = context.Target;
        var controller = card.Controller;
        controller.Hand.Remove(card);
        controller.PlayArea.Remove(card);
        controller.Discard.Remove(card);
        controller.Deck.Remove(card);

        controller.Deck.Insert(0, card);
        card.Location = "deck";
    }
}
