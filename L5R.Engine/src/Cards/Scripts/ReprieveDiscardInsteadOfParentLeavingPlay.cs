namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for reprieve: when the attached character would leave play, discard
/// this attachment instead. Needs event.card compared against source.parent (not
/// source), a legality check function, and a cancel-with-replacement gameAction shape,
/// none modeled by the closed vocabulary. Stubbed until the state model has attachments.
/// </summary>
public sealed class ReprieveDiscardInsteadOfParentLeavingPlay : ICardScript
{
}
