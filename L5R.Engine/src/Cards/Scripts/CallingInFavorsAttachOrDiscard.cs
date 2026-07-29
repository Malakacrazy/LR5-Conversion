using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.State;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// calling-in-favors: dishonor a friendly character (context.CostTarget, the cost), then
/// attach a chosen opponent's attachment (context.Target) to it and take control. Needs a
/// contextPath reference to the dishonor cost's chosen target, which the closed valueRef
/// vocabulary doesn't support - matches this card's own scriptOverride reason. Reuses
/// AttachGameActionHandler (with takeControl) and DishonorGameActionHandler directly.
/// Ringteki's own "otherwise discard the attachment" ifAble fallback (for when attaching
/// isn't possible) is not modeled: GameState.IsAttachRestricted's one restriction check
/// (attachmentMyControlOnly) is keyed on the acting player differing from the new parent's
/// controller, which can never be true here - the acting player and the dishonored
/// cost-target's controller are already required to match by the check above. No other
/// attach-legality check exists in this engine yet to make the fallback reachable.
/// </summary>
public sealed class CallingInFavorsAttachOrDiscard : ICardScript
{
    private static readonly JsonElement TakeControlTrue = JsonDocument.Parse("{\"takeControl\":true}").RootElement;

    public void Execute(AbilityContext context)
    {
        var callingInFavors = context.Source;

        var attachment = context.Target
            ?? throw new InvalidOperationException($"'{callingInFavors.Id}' requires context.Target (the opponent's attachment) to be set.");

        if (attachment.Type != CardType.Attachment)
            throw new InvalidOperationException($"'{attachment.Id}' must be an attachment.");

        if (attachment.Controller != context.Game.Opponent(context.Player))
            throw new InvalidOperationException($"'{attachment.Id}' must be controlled by the opponent.");

        var costTarget = context.CostTarget
            ?? throw new InvalidOperationException($"'{callingInFavors.Id}' requires context.CostTarget (the character to dishonor as a cost) to be set.");

        if (costTarget.Controller != context.Player)
            throw new InvalidOperationException($"'{costTarget.Id}' must be controlled by '{callingInFavors.Id}''s controller.");

        if (costTarget.IsDishonored)
            throw new InvalidOperationException($"'{costTarget.Id}' is already dishonored.");

        context.Target = costTarget;
        new DishonorGameActionHandler().Execute(context, null);

        context.Target = attachment;
        context.PlayAttachTarget = costTarget;
        new AttachGameActionHandler().Execute(context, TakeControlTrue);
    }
}
