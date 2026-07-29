using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// niten-master: after a weapon controlled by this character's controller is attached to
/// it, ready this character (onCardAttached: "event.parent === context.source &&
/// event.card.hasTrait('weapon') && event.card.controller === context.player"). The caller
/// supplies the just-attached card as context.Target (trust-the-caller, same convention as
/// every other event-shaped reaction in this backlog) - checked directly against Card.
/// AttachedTo/Traits/Controller rather than a JSON predicate, since there's no JSON driving
/// this card. "limit: perRound(2)" needs no work, matching every "limit" field's
/// established no-op precedent.
/// </summary>
public sealed class NitenMasterReadyOnWeaponAttached : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var nitenMaster = context.Source;
        var attachedCard = context.Target
            ?? throw new InvalidOperationException($"'{nitenMaster.Id}' requires context.Target to be set.");

        if (attachedCard.AttachedTo != nitenMaster)
            throw new InvalidOperationException($"'{attachedCard.Id}' is not attached to '{nitenMaster.Id}'.");

        if (!attachedCard.Traits.Contains("weapon"))
            throw new InvalidOperationException($"'{attachedCard.Id}' is not a weapon.");

        if (attachedCard.Controller != context.Player)
            throw new InvalidOperationException($"'{attachedCard.Id}' must be controlled by '{nitenMaster.Id}''s controller.");

        context.Target = nitenMaster;
        new ReadyGameActionHandler().Execute(context, null);
    }
}
