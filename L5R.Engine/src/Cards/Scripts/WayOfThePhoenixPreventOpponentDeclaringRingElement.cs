namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for way-of-the-phoenix: choose a ring and an opponent; that player
/// cannot declare conflicts of that ring's element this phase (max 1 per phase). Needs a
/// ring-scoped bulk target (no ring equivalent of allCardsMatching exists) and a
/// player-filter-function effect value. Stubbed until the state model has rings.
/// </summary>
public sealed class WayOfThePhoenixPreventOpponentDeclaringRingElement : ICardScript
{
}
