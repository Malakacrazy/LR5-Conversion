using L5R.Engine.Abilities;
using L5R.Engine.State;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// artisan-academy: reveal the top card of the conflict deck, then may play it from there
/// for the rest of the conflict phase. This engine has a single, fully-visible-to-the-test
/// Player.Deck (see its own doc comment - not yet split into conflict/dynasty decks), and
/// PlayCardGameActionHandler already moves context.Target to play from wherever it currently
/// sits, with no location check at all - so "playable from the deck" needs no eligibility
/// tracking of its own; the caller can already play Player.Deck[0] (the top card) directly.
/// That leaves nothing here to actually mutate: like LookAtGameActionHandler, this is a
/// verified no-op precondition check, not a stub - ringteki's onCardMoved/onPhaseEnded/
/// onDeckShuffled expiry listeners for the custom-duration lasting effect have nothing to
/// drive here since there's no "eligible to play from deck" flag being granted or revoked.
/// </summary>
public sealed class ArtisanAcademyRevealTopCard : ICardScript
{
    public void Execute(AbilityContext context)
    {
        if (context.Game.CurrentPhase != Phase.Conflict)
            throw new InvalidOperationException($"'{context.Source.Id}' can only be used during the conflict phase.");

        if (context.Player.Deck.Count == 0)
            throw new InvalidOperationException($"'{context.Player.Name}' has no cards left in their deck to reveal.");
    }
}
