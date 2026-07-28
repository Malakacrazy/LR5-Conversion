namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for cloud-the-mind: ringteki's DrawCard.canPlay override restricting
/// play to when the controller has a 'shugenja' character in play. Stubbed until the
/// state model (task 8) has enough of a play-resolution pipeline for this to hook into -
/// registering it now proves the scriptOverride loading mechanism end to end.
/// </summary>
public sealed class CloudTheMindPlayRestriction : ICardScript
{
}
