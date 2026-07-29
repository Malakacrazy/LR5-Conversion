using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.State;

namespace L5R.Engine.Dsl.GameActions;

/// <summary>
/// ringteki PlayCardAction (GameActions.ts playCard): plays context.Target (defaulting to
/// context.Source, per the shared no-target-block convention) from wherever it currently is,
/// paying its printed cost automatically (ringteki PlayCardProperties.payCosts: true by
/// default - not something the JSON declares per card). Only guidance-of-the-ancestors'
/// slice is modeled: an attachment being played requires context.PlayAttachTarget (no
/// attach-target search/prompt exists, same trust-the-caller convention as every other
/// target) and is attached directly rather than routed through a separate "attach"
/// gameAction. Non-attachment types just move to play area with no attach step - the same
/// one-line ZoneMover call, not separately exercised by any ported card yet.
/// </summary>
public sealed class PlayCardGameActionHandler : IGameActionHandler
{
    public void Execute(AbilityContext context, JsonElement? parameters)
    {
        var card = context.Target ?? context.Source;

        if (context.Game.IsPlayerRestrictedFrom(context.Player, "play", card))
            throw new InvalidOperationException($"'{context.Player.Name}' cannot play '{card.Id}' right now.");

        var cost = context.Game.EffectiveCost(card, context.Player);
        if (context.Player.Fate < cost)
            throw new InvalidOperationException($"'{context.Player.Name}' cannot afford to play '{card.Id}' (cost {cost}, has {context.Player.Fate} fate).");

        context.Player.Fate -= cost;

        if (card.Type == CardType.Attachment)
        {
            var attachTarget = context.PlayAttachTarget
                ?? throw new InvalidOperationException($"Playing attachment '{card.Id}' requires context.PlayAttachTarget to be set.");

            if (context.Game.IsAttachRestricted(card, attachTarget, context.Player))
                throw new InvalidOperationException($"'{card.Id}' cannot be attached to '{attachTarget.Id}' (attachmentMyControlOnly).");

            ZoneMover.MoveTo(card, card.Controller.PlayArea, "play area");
            card.AttachedTo = attachTarget;
        }
        else
        {
            ZoneMover.MoveTo(card, card.Controller.PlayArea, "play area");
        }
    }
}
