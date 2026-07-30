using System.Text.Json;
using L5R.Engine.Abilities;

namespace L5R.Engine.Dsl.GameActions;

/// <summary>
/// Backs tattooed-wanderer/togashi-kazue's "may be played as an attachment instead of as a
/// character" alternate play mode (ringteki: a custom PlayAttachmentAction subclass that
/// temporarily overrides the card's printedType). Rather than making Card.Type mutable or
/// adding a parallel "effective type" field - both would ripple into every Type-keyed switch
/// in the engine - this duplicates PlayCardGameActionHandler's attachment branch verbatim,
/// just without its `card.Type == CardType.Attachment` guard, and leaves Card.Type untouched.
/// Nothing elsewhere keys off Type to decide whether a card is "acting as" an attachment -
/// attach-specific state (AttachedTo, WhileAttachedEffects, IsAttachRestricted) is already
/// keyed off Card.AttachedTo, not Type, so a Character card attached this way behaves exactly
/// like a real attachment everywhere that matters.
/// </summary>
public sealed class PlayAsAttachmentGameActionHandler : IGameActionHandler
{
    public void Execute(AbilityContext context, JsonElement? parameters)
    {
        var card = context.Target ?? context.Source;

        if (context.Game.IsPlayerRestrictedFrom(context.Player, "play", card))
            throw new InvalidOperationException($"'{context.Player.Name}' cannot play '{card.Id}' right now.");

        if (card.PlayScript?.CanPlay(context) == false)
            throw new InvalidOperationException($"'{context.Player.Name}' cannot play '{card.Id}' right now.");

        var cost = context.Game.EffectiveCost(card, context.Player);
        if (context.Player.Fate < cost)
            throw new InvalidOperationException($"'{context.Player.Name}' cannot afford to play '{card.Id}' (cost {cost}, has {context.Player.Fate} fate).");

        context.Player.Fate -= cost;

        var attachTarget = context.PlayAttachTarget
            ?? throw new InvalidOperationException($"Playing '{card.Id}' as an attachment requires context.PlayAttachTarget to be set.");

        if (context.Game.IsAttachRestricted(card, attachTarget, context.Player))
            throw new InvalidOperationException($"'{card.Id}' cannot be attached to '{attachTarget.Id}' (attachmentMyControlOnly).");

        ZoneMover.MoveTo(card, card.Controller.PlayArea, "play area");
        card.AttachedTo = attachTarget;
    }
}
