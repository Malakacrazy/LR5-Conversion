namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for the-perfect-gift: reveal the top 4 cards of each player's conflict
/// deck, choose 1 revealed card owned by each player to add to their hand, then shuffle.
/// Needs a sequential multi-step combinator with two independent nested selections,
/// neither modeled by the closed vocabulary. Stubbed until the state model has decks.
/// </summary>
public sealed class ThePerfectGiftRevealAndGiveEachPlayerACard : ICardScript
{
}
