namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for good-omen: can only be played while the controller has composure
/// (their honor bid is lower than their opponent's). Overrides DrawCard.canPlay, same
/// play-eligibility gap as CloudTheMindPlayRestriction/BlackmailCannotPlayUnlessLessHonorable.
/// Stubbed until the state model has a play-resolution pipeline with eligibility checks.
/// </summary>
public sealed class GoodOmenCannotPlayWithoutComposure : ICardScript
{
}
