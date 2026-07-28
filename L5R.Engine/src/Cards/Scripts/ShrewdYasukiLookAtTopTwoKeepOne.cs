namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for shrewd-yasuki: while participating and a holding is in play, look
/// at the top 2 cards of your conflict deck, add one to hand, and put the other on the
/// bottom. Needs a bespoke 'look at N, keep one, bottom the rest' handler, no equivalent
/// gameAction exists. Stubbed until the state model has decks.
/// </summary>
public sealed class ShrewdYasukiLookAtTopTwoKeepOne : ICardScript
{
}
