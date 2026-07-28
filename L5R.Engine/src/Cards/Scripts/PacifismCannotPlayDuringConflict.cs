namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for pacifism: cannot be played while a conflict is currently in
/// progress. Overrides DrawCard.canPlay, same play-eligibility gap as
/// CloudTheMindPlayRestriction/HeightOfFashionCannotPlayDuringConflict. Stubbed until the
/// state model has a play-resolution pipeline with eligibility checks.
/// </summary>
public sealed class PacifismCannotPlayDuringConflict : ICardScript
{
}
