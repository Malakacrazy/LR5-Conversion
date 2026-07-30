using L5R.Engine.Abilities;
using L5R.Engine.Dsl;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// the-perfect-gift: reveal the top 4 cards of each player's deck, choose 1 revealed card
/// owned by each player to add to their hand, then shuffle. Ringteki's "sequential"
/// combinator here just chains 5 steps that all run immediately in one action resolution -
/// not a delayed/later effect, so it needs no new mechanism, just plain sequential C#
/// statements (matches every other multi-step script this session). "Look at" is a
/// verified no-op (LookAtGameActionHandler's own doc comment) and "shuffle" is a verified
/// no-op (no RNG/shuffle primitive exists - DeckSearchGameActionHandler's own doc comment),
/// so neither needs a call, just reading the top-4 pools to validate against. Reuses
/// context.ChosenCardMenuCard for the controller's own pick and context.ChosenDeckSearchCard
/// for the opponent's pick - a clean semantic fit for "the opponent's own choice from their
/// searched top-N", avoiding a new field.
/// </summary>
public sealed class ThePerfectGiftRevealAndGiveEachPlayerACard : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var perfectGift = context.Source;
        var opponent = context.Game.Opponent(context.Player);

        var myTopFour = context.Player.Deck.Take(4).ToList();
        var opponentTopFour = opponent.Deck.Take(4).ToList();

        var myChoice = context.ChosenCardMenuCard
            ?? throw new InvalidOperationException($"'{perfectGift.Id}' requires context.ChosenCardMenuCard (the controller's own choice) to be set.");
        if (!myTopFour.Contains(myChoice))
            throw new InvalidOperationException($"'{myChoice.Id}' is not among the top 4 cards of '{context.Player.Name}''s deck.");

        var opponentChoice = context.ChosenDeckSearchCard
            ?? throw new InvalidOperationException($"'{perfectGift.Id}' requires context.ChosenDeckSearchCard (the opponent's own choice) to be set.");
        if (!opponentTopFour.Contains(opponentChoice))
            throw new InvalidOperationException($"'{opponentChoice.Id}' is not among the top 4 cards of '{opponent.Name}''s deck.");

        ZoneMover.MoveTo(myChoice, context.Player.Hand, "hand");
        ZoneMover.MoveTo(opponentChoice, opponent.Hand, "hand");
    }
}
