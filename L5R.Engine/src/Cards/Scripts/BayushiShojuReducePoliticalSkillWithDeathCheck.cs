namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for bayushi-shoju: while participating in a political conflict, reduce
/// an opponent's participating character's political skill by 1 until the end of the
/// conflict, discarding it if it reaches 0 (limit twice per round). Needs a nested
/// delayedEffect as one of the lasting effect's array entries, the same gap as
/// ObstinateRecruitDiscardWhenOpponentMoreHonorable. Stubbed until the state model has
/// conflicts.
/// </summary>
public sealed class BayushiShojuReducePoliticalSkillWithDeathCheck : ICardScript
{
}
