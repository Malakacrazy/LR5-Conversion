namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for blackmail: can only be played while the controller is less
/// honorable than their opponent. Overrides DrawCard.canPlay, same play-eligibility gap
/// as CloudTheMindPlayRestriction/HeightOfFashionCannotPlayDuringConflict. Stubbed until
/// the state model has a play-resolution pipeline with eligibility checks.
/// </summary>
public sealed class BlackmailCannotPlayUnlessLessHonorable : ICardScript
{
}
