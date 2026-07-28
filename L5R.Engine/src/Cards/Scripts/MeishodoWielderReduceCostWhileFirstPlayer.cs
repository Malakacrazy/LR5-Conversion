namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for meishodo-wielder: reduce the cost to play this character by 1
/// while you are the first player. Needs a 'first player' game-state check with no
/// equivalent in the closed predicate vocabulary. Stubbed until the state model tracks
/// first player.
/// </summary>
public sealed class MeishodoWielderReduceCostWhileFirstPlayer : ICardScript
{
}
