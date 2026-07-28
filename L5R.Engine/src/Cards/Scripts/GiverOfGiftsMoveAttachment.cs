namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// scriptOverride for giver-of-gifts: choose an attachment you control, then move it to
/// an eligible character you control other than its current parent. Needs a two-level
/// select (choose the attachment, then choose the destination character) the schema's
/// target.gameAction can't express. Stubbed until the state model supports nested/
/// dependent selection.
/// </summary>
public sealed class GiverOfGiftsMoveAttachment : ICardScript
{
}
