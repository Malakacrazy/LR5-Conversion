namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for keeper-of-air: reaction trigger needs to inspect the conflict
/// event's elements/winner/defendingPlayer fields, which the closed predicate vocabulary
/// deliberately doesn't generalize (event shapes vary too much per event name). Stubbed
/// until the state model has conflicts to inspect.
/// </summary>
public sealed class KeeperOfAirGainFateOnDefendedWin : ICardScript
{
}
