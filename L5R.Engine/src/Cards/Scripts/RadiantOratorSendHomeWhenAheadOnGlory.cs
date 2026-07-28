namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for radiant-orator: while participating, if your ready participating
/// characters have more total glory than your opponent's, send an enemy character home.
/// Needs a sum-of-stat aggregate over a set of cards, beyond countCardsMatching's plain
/// count. Stubbed until the state model has conflicts and skill calculation.
/// </summary>
public sealed class RadiantOratorSendHomeWhenAheadOnGlory : ICardScript
{
}
