using L5R.Engine.Abilities;
using L5R.Engine.State;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// mountain-s-anvil-castle: bow this stronghold during a conflict to give a participating
/// character with attachments +1/+1 (or +2/+2 with 2+ attachments) until the end of the
/// conflict. Card.cs only tracks the reverse relationship (Card.AttachedTo, child to
/// parent), so the attachment count is a live scan (GameState.AllCards().Count(c =>
/// c.AttachedTo == target)) rather than a maintained list - matches how this engine has
/// never needed a materialized "attachments on this card" collection before. The min()-
/// scaled bonus is applied as two ordinary LastingEffect entries (military + political),
/// the exact same primitive CardLastingEffectGameActionHandler's own "modifyBothSkills"
/// case produces - constructed directly here since there's no JSON params to drive that
/// handler's own dispatch.
/// </summary>
public sealed class MountainsAnvilCastleBonusForAttachments : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var stronghold = context.Source;

        if (context.Game.CurrentConflict is null)
            throw new InvalidOperationException($"'{stronghold.Id}' can only be used during a conflict.");

        if (stronghold.Bowed)
            throw new InvalidOperationException($"'{stronghold.Id}' is already bowed.");

        var target = context.Target
            ?? throw new InvalidOperationException($"'{stronghold.Id}' requires context.Target to be set.");

        if (!IsParticipating(context.Game, target))
            throw new InvalidOperationException($"'{target.Id}' is not participating.");

        var attachmentCount = context.Game.AllCards().Count(c => c.AttachedTo == target);
        if (attachmentCount == 0)
            throw new InvalidOperationException($"'{target.Id}' has no attachments.");

        stronghold.Bowed = true;

        var bonus = Math.Min(attachmentCount, 2);
        context.Game.LastingEffects.Add(new LastingEffect { Target = target, Stat = "military", Value = bonus, Duration = "untilEndOfConflict" });
        context.Game.LastingEffects.Add(new LastingEffect { Target = target, Stat = "political", Value = bonus, Duration = "untilEndOfConflict" });
    }

    private static bool IsParticipating(GameState game, Card card) =>
        game.CurrentConflict is { } conflict && (conflict.Attackers.Contains(card) || conflict.Defenders.Contains(card));
}
