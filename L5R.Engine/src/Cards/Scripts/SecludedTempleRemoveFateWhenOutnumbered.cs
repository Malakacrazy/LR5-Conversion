namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for secluded-temple: after the conflict phase begins, each opponent
/// who controls more characters than you must remove 1 fate from a character with fate
/// on it. Needs event.phase field inspection and a per-player card count comparison,
/// neither modeled by the closed vocabulary. Stubbed until the state model has phases.
/// </summary>
public sealed class SecludedTempleRemoveFateWhenOutnumbered : ICardScript
{
}
