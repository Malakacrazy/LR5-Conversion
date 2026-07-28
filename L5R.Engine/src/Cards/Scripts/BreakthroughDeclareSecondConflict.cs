namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for breakthrough: after winning your first conflict of the phase as
/// attacker by breaking a province, immediately declare a second conflict. Needs
/// event.conflict field inspection and a conflict-collection query, both beyond the
/// closed predicate vocabulary. Stubbed until the state model has conflicts.
/// </summary>
public sealed class BreakthroughDeclareSecondConflict : ICardScript
{
}
