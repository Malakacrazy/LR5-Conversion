namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for watch-commander: after an opponent plays a card during a conflict
/// the attached character is participating in, that player loses 1 honor (unlimited).
/// Needs event.player field inspection, a per-character attachment copy limit, and an
/// unlimitedPerConflict reaction limit, none modeled by the closed vocabulary. Stubbed
/// until the state model has attachments and conflicts.
/// </summary>
public sealed class WatchCommanderLoseHonorOnOpponentCardPlayed : ICardScript
{
}
