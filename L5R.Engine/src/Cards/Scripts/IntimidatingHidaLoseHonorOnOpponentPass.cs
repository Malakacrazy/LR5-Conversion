namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for intimidating-hida: after an opponent passes on declaring a
/// conflict, that player loses 1 honor. Needs event.conflict.attackingPlayer field
/// inspection beyond the closed predicate vocabulary. Stubbed until the state model has
/// conflicts.
/// </summary>
public sealed class IntimidatingHidaLoseHonorOnOpponentPass : ICardScript
{
}
