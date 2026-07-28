namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for fearsome-mystic's action only (the +2 glory persistent effect is
/// generic): while participating, remove 1 fate from each participating opponent
/// character with lower glory than this character. Needs a candidate-vs-source stat
/// comparison the closed predicate vocabulary can't express. Stubbed until the state
/// model has conflicts.
/// </summary>
public sealed class FearsomeMysticRemoveFateFromLowerGloryOpponents : ICardScript
{
}
