using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.State;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// giver-of-gifts: choose an attachment its controller controls, then move it to an
/// eligible character the same player controls, other than its current parent. Needs a
/// two-level select (choose the attachment, then choose the destination character) the
/// schema's target.gameAction can't express - matches this card's own scriptOverride
/// reason. Reuses AttachGameActionHandler directly (no takeControl - the attachment stays
/// under the same controller, only its parent changes).
/// </summary>
public sealed class GiverOfGiftsMoveAttachment : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var giverOfGifts = context.Source;

        var attachment = context.Target
            ?? throw new InvalidOperationException($"'{giverOfGifts.Id}' requires context.Target (the attachment to move) to be set.");

        if (attachment.Type != CardType.Attachment)
            throw new InvalidOperationException($"'{attachment.Id}' must be an attachment.");

        if (attachment.Controller != context.Player)
            throw new InvalidOperationException($"'{attachment.Id}' must be controlled by '{giverOfGifts.Id}''s controller.");

        var newParent = context.PlayAttachTarget
            ?? throw new InvalidOperationException($"'{giverOfGifts.Id}' requires context.PlayAttachTarget (the destination character) to be set.");

        if (newParent.Controller != context.Player)
            throw new InvalidOperationException($"'{newParent.Id}' must be controlled by '{giverOfGifts.Id}''s controller.");

        if (newParent == attachment.AttachedTo)
            throw new InvalidOperationException($"'{attachment.Id}' must move to a different character.");

        new AttachGameActionHandler().Execute(context, null);
    }
}
