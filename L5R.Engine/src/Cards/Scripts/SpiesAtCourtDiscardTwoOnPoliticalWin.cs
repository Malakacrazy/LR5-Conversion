namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for spies-at-court: after winning a political conflict, dishonor a
/// friendly participating character to discard 2 cards at random from the opponent's
/// hand (max 1 per conflict). Needs event.conflict.winner/conflictType field inspection
/// beyond the closed predicate vocabulary. Stubbed until the state model has conflicts.
/// </summary>
public sealed class SpiesAtCourtDiscardTwoOnPoliticalWin : ICardScript
{
}
