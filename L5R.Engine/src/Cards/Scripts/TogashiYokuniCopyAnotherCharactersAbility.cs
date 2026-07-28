namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for togashi-yokuni: choose a triggered ability printed on another
/// character and gain that ability until the end of the phase (max 1 per round). Needs
/// target.mode 'ability' and copying an entire ability object, both explicitly outside
/// the closed vocabulary. Stubbed until the state model has ability introspection.
/// </summary>
public sealed class TogashiYokuniCopyAnotherCharactersAbility : ICardScript
{
}
