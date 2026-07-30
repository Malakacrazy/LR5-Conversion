using System.Linq;
using System.Text.Json;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl;

namespace L5R.Engine.Dsl.GameActions;

/// <summary>
/// ringteki GameActions.ts discardFromPlay: discard a card from play. Checks
/// IsRestrictedFrom "discardFromPlay" (steadfast-samurai's own cardCannot) - previously
/// unchecked here since no ported card needed to block this action before now. Also cascades
/// to whatever's currently attached to the discarded card (CardGameAction.ts's
/// createContingentEvents/updateLeavesPlayEvent: a character's attachments leave play
/// alongside it) - each attachment goes to its own controller's discard pile, except an
/// "ancestral" one, which returns to its controller's hand instead (this engine has no
/// separate Owner distinct from Controller - see Card.cs). Discarding an attachment
/// *directly* (context.Target is the attachment itself) never hits this cascade, matching
/// ringteki's own isContingent gate: ancestral only saves a card from leaving alongside its
/// parent, not from being discarded on its own. Also fires any eligible vengeful-berserker
/// reactions the discarded character's own controller controls - see
/// VengefulBerserkerFirer's own doc comment.
/// </summary>
public sealed class DiscardFromPlayGameActionHandler : IGameActionHandler
{
    public void Execute(AbilityContext context, JsonElement? parameters)
    {
        if (context.Target is null)
            throw new InvalidOperationException("discardFromPlay requires context.Target to be set.");

        if (context.Game.IsRestrictedFrom(context.Target, "discardFromPlay", context.Source))
            throw new InvalidOperationException($"'{context.Target.Id}' cannot be discarded.");

        var card = context.Target;
        ZoneMover.MoveTo(card, card.Controller.Discard, "discard");

        foreach (var attachment in context.Game.AllCards().Where(a => a.AttachedTo == card).ToList())
        {
            attachment.AttachedTo = null;
            if (context.Game.HasKeyword(attachment, "ancestral"))
                ZoneMover.MoveTo(attachment, attachment.Controller.Hand, "hand");
            else
                ZoneMover.MoveTo(attachment, attachment.Controller.Discard, "discard");
        }

        VengefulBerserkerFirer.FireEligibleReactions(context.Game, card);
    }
}
