namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for honored-blade: after the attached character wins a conflict, gain
/// 1 honor. Needs event.conflict.winner field inspection beyond the closed predicate
/// vocabulary. Stubbed until the state model has conflicts.
/// </summary>
public sealed class HonoredBladeGainHonorWhenParentWins : ICardScript
{
}
