namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for seeker-of-air: after an air province you control is revealed, gain
/// 1 fate. Needs event.card cross-referenced against source's own trait, beyond the
/// isSelf/isType/hasTrait-against-event.card convention. Stubbed until the state model
/// has provinces and roles.
/// </summary>
public sealed class SeekerOfAirGainFateOnMatchingProvinceReveal : ICardScript
{
}
