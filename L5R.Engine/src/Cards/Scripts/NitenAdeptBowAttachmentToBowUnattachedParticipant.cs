using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.State;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// niten-adept: while participating with an attachment, bow one of its own attachments
/// (context.CostTarget, caller-supplied) to bow a participating character with no
/// attachments of its own. "card.parent === context.source" (the cost candidate's parent)
/// and "attachments.size()" (a per-card attachment count) both need direct checks against
/// Card.AttachedTo/a live GameState.AllCards() scan - the same primitives already used for
/// mountain-s-anvil-castle's own attachment count.
/// </summary>
public sealed class NitenAdeptBowAttachmentToBowUnattachedParticipant : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var nitenAdept = context.Source;

        if (context.Game.AllCards().Count(c => c.AttachedTo == nitenAdept) == 0)
            throw new InvalidOperationException($"'{nitenAdept.Id}' has no attachments.");

        if (!IsParticipating(context.Game, nitenAdept))
            throw new InvalidOperationException($"'{nitenAdept.Id}' can only be used while participating.");

        var costTarget = context.CostTarget
            ?? throw new InvalidOperationException($"'{nitenAdept.Id}' requires context.CostTarget to be set.");

        if (costTarget.AttachedTo != nitenAdept)
            throw new InvalidOperationException($"'{costTarget.Id}' is not attached to '{nitenAdept.Id}'.");

        if (costTarget.Bowed)
            throw new InvalidOperationException($"'{costTarget.Id}' is already bowed.");

        var target = context.Target
            ?? throw new InvalidOperationException($"'{nitenAdept.Id}' requires context.Target to be set.");

        if (!IsParticipating(context.Game, target))
            throw new InvalidOperationException($"'{target.Id}' is not participating.");

        if (context.Game.AllCards().Any(c => c.AttachedTo == target))
            throw new InvalidOperationException($"'{target.Id}' has an attachment.");

        costTarget.Bowed = true;

        context.Target = target;
        new BowGameActionHandler().Execute(context, null);
    }

    private static bool IsParticipating(GameState game, Card card) =>
        game.CurrentConflict is { } conflict && (conflict.Attackers.Contains(card) || conflict.Defenders.Contains(card));
}
