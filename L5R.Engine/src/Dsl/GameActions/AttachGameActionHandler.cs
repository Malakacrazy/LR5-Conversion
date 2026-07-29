using System.Text.Json;
using L5R.Engine.Abilities;

namespace L5R.Engine.Dsl.GameActions;

/// <summary>
/// ringteki AttachAction (giver-of-gifts' "move an attachment", calling-in-favors'
/// "attach with takeControl: true"): moves context.Target (an already-in-play attachment)
/// to a new parent, context.PlayAttachTarget - the same field PlayCardGameActionHandler
/// already uses for "which character an attachment being played attaches to", reused here
/// for "which character an already-in-play attachment is being moved to" since both are
/// just "the character this attachment attaches to" from the attachment's own perspective.
/// params.takeControl (default false) additionally changes the attachment's controller via
/// GameState.TakeControl, same "untilEndOfConflict" duration convention as blackmail's own
/// cardLastingEffect takeControl - this engine has no separate "permanent until detached"
/// reversion tracking.
/// </summary>
public sealed class AttachGameActionHandler : IGameActionHandler
{
    public void Execute(AbilityContext context, JsonElement? parameters)
    {
        var attachment = context.Target
            ?? throw new InvalidOperationException("attach requires context.Target to be set.");
        var newParent = context.PlayAttachTarget
            ?? throw new InvalidOperationException("attach requires context.PlayAttachTarget to be set.");

        if (context.Game.IsAttachRestricted(attachment, newParent, context.Player))
            throw new InvalidOperationException($"'{attachment.Id}' cannot be attached to '{newParent.Id}' (attachmentMyControlOnly).");

        attachment.AttachedTo = newParent;

        var takeControl = parameters?.TryGetProperty("takeControl", out var tc) == true && tc.GetBoolean();
        if (takeControl && attachment.Controller != newParent.Controller)
            context.Game.TakeControl(attachment, newParent.Controller, "untilEndOfConflict");
    }
}
