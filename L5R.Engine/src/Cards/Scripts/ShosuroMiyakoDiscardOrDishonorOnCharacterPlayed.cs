namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for shosuro-miyako: after you play a character from your hand, your
/// opponent discards at random or dishonors a character they control. Needs
/// event.player/event.playType field inspection and a nested card selection for the
/// dishonor choice, the same two-level select gap as CourtGamesHonorOrDishonorParticipant.
/// Stubbed until the state model has hands and honor.
/// </summary>
public sealed class ShosuroMiyakoDiscardOrDishonorOnCharacterPlayed : ICardScript
{
}
