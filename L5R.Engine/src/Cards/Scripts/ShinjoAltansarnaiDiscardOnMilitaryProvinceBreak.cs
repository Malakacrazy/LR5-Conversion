namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for shinjo-altansarnai: after breaking a province during a military
/// conflict this character is participating in, the opponent discards a character they
/// control. Needs event.conflict.conflictType field inspection beyond the closed
/// predicate vocabulary. Stubbed until the state model has conflicts.
/// </summary>
public sealed class ShinjoAltansarnaiDiscardOnMilitaryProvinceBreak : ICardScript
{
}
