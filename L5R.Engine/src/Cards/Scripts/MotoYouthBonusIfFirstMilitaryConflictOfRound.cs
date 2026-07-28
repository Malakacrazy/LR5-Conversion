namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for moto-youth: +1 military skill during a military conflict, but only
/// if no earlier completed conflict this round was declared/switched to military - scans
/// the round's conflict history. Stubbed until the state model tracks conflict history.
/// </summary>
public sealed class MotoYouthBonusIfFirstMilitaryConflictOfRound : ICardScript
{
}
