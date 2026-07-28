namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for stand-your-ground: when an honored character you control would
/// leave play, instead discard that character's honored status token. Needs event.card
/// field inspection and a cancel-with-replacement gameAction shape, the same gap as
/// ReprieveDiscardInsteadOfParentLeavingPlay. Stubbed until the state model has honor
/// tokens.
/// </summary>
public sealed class StandYourGroundDiscardTokenInsteadOfLeavingPlay : ICardScript
{
}
