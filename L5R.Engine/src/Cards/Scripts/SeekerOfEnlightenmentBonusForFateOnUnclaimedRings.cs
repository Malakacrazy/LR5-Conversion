namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for seeker-of-enlightenment: gets +1/+1 for each fate on each unclaimed
/// ring. Needs a sum-of-fate aggregate over rings, beyond countCardsMatching's card-count
/// shape. Stubbed until the state model has rings.
/// </summary>
public sealed class SeekerOfEnlightenmentBonusForFateOnUnclaimedRings : ICardScript
{
}
