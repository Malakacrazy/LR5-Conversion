namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for keeper-of-earth: after winning an earth conflict as defender, gain
/// 1 fate. Needs event.conflict.elements/winner/defendingPlayer field inspection beyond
/// the closed predicate vocabulary. Stubbed until the state model has conflicts and roles.
/// </summary>
public sealed class KeeperOfEarthGainFateOnEarthDefenseWin : ICardScript
{
}
