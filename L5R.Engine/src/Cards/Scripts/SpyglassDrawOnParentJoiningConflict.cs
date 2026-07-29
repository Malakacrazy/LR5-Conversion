using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.State;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// spyglass: after the attached character commits to the conflict (as an attacker, as a
/// defender, or via moveToConflict - all three of ringteki's alternate "when" clauses
/// collapse to the same underlying fact once resolved: the parent is now participating),
/// draw 1 card. The caller invokes this once that's already true (trust-the-caller, same
/// convention as every other event-shaped reaction in this backlog) rather than needing
/// array-membership checks against event.attackers/event.defenders. "limit: perRound(2)"
/// needs no work, matching every "limit" field's established no-op precedent.
/// </summary>
public sealed class SpyglassDrawOnParentJoiningConflict : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var spyglass = context.Source;
        var parent = spyglass.AttachedTo
            ?? throw new InvalidOperationException($"'{spyglass.Id}' is not currently attached to anything.");

        if (!IsParticipating(context.Game, parent))
            throw new InvalidOperationException($"'{spyglass.Id}' can only trigger once the attached character joins the conflict.");

        new DrawGameActionHandler().Execute(context, null);
    }

    private static bool IsParticipating(GameState game, Card card) =>
        game.CurrentConflict is { } conflict && (conflict.Attackers.Contains(card) || conflict.Defenders.Contains(card));
}
