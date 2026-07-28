namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for fallen-in-battle: after winning a military conflict by 5+ skill,
/// discard a participating character (max 1 per conflict). Needs
/// event.conflict.winner/conflictType/skillDifference field inspection beyond the closed
/// predicate vocabulary. Stubbed until the state model has conflicts.
/// </summary>
public sealed class FallenInBattleDiscardOnDecisiveMilitaryWin : ICardScript
{
}
