using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Dsl;

/// <summary>
/// ikoma-prodigy: gain 1 honor after fate is placed on this character. Its own script
/// (IkomaProdigyGainHonorWhenFateAddedOrMoved) checks "has fate right now" rather than "fate
/// was just added" - a state fact that doesn't reset on its own, over-firing badly if polled
/// every action window. Invoked here as a one-shot hook at the single place fate actually
/// gets placed on a card (PlaceFateGameActionHandler.Execute, also the JSON DSL's registered
/// "placeFate" gameAction), it fires exactly once per placement instead. Doesn't cover every
/// conceivable way a card's Fate could increase (a few scriptOverride scripts add fate via a
/// direct += rather than this shared handler) - matches this session's general "hook the
/// canonical shared primitive, not every call site" precedent (same reasoning as
/// Honor/DishonorGameActionHandler for young-rumormonger).
/// </summary>
public static class IkomaProdigyFirer
{
    public static void FireIfLegal(GameState game, Card card)
    {
        if (card.Id != "ikoma-prodigy" || card.Fate <= 0)
            return;

        var context = new AbilityContext { Game = game, Player = card.Controller, Source = card };
        new IkomaProdigyGainHonorWhenFateAddedOrMoved().Execute(context);
    }
}
