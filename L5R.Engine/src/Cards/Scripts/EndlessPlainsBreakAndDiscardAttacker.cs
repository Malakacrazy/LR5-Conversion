namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for endless-plains: after an attack is declared here, break this
/// province and force the opponent to discard an attacking character. Needs
/// event.conflict.conflictProvince field inspection beyond the closed predicate
/// vocabulary. Stubbed until the state model has conflicts and provinces.
/// </summary>
public sealed class EndlessPlainsBreakAndDiscardAttacker : ICardScript
{
}
