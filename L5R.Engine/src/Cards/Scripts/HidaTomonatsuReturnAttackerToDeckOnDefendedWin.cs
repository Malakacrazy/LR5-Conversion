namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for hida-tomonatsu: after winning a conflict as defender, sacrifice
/// this character and return a non-unique attacking character to the top of its owner's
/// deck. Needs event.conflict.winner field inspection beyond the closed predicate
/// vocabulary. Stubbed until the state model has conflicts.
/// </summary>
public sealed class HidaTomonatsuReturnAttackerToDeckOnDefendedWin : ICardScript
{
}
