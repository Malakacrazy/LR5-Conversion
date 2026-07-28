namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for keeper-initiate: after claiming a ring matching your role's
/// element, put this character into play from your dynasty discard pile or provinces,
/// then place 1 fate on it. Needs event.player/event.conflict/event.ring inspection, a
/// role-trait cross-reference, and a chained follow-up ability, none modeled by the
/// closed vocabulary. Stubbed until the state model has roles and rings.
/// </summary>
public sealed class KeeperInitiatePutIntoPlayOnMatchingRingClaim : ICardScript
{
}
