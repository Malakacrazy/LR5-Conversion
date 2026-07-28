namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for height-of-fashion: cannot be played while a conflict is currently
/// in progress. Same play-eligibility gap as CloudTheMindPlayRestriction. Stubbed until
/// the state model has a play-resolution pipeline with eligibility checks.
/// </summary>
public sealed class HeightOfFashionCannotPlayDuringConflict : ICardScript
{
}
