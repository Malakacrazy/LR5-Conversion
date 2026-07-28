namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for indomitable-will: after a character you control wins a conflict
/// alone, that character does not bow as a result of the conflict's resolution. Needs
/// event.conflict field inspection, a participant count, and a conflict-collection query
/// beyond the closed predicate vocabulary. Stubbed until the state model has conflicts.
/// </summary>
public sealed class IndomitableWillPreventBowOnSoloWin : ICardScript
{
}
