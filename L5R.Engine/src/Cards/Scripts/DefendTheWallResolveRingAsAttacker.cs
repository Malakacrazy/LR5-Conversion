namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for defend-the-wall: after winning a conflict at this province, resolve
/// the ring effect as if attacking player. Needs event.conflict field inspection
/// (conflictProvince, winner) beyond the closed predicate vocabulary. Stubbed until the
/// state model has conflicts and provinces.
/// </summary>
public sealed class DefendTheWallResolveRingAsAttacker : ICardScript
{
}
