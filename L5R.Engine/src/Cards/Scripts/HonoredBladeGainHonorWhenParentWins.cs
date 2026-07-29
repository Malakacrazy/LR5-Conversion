using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.State;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// honored-blade: after the attached character's controller wins the conflict it's
/// participating in, gain 1 honor (afterConflict reaction, "event.conflict.winner ===
/// context.source.parent.controller"). No conflict-resolution pipeline computes a winner
/// automatically (matches this engine's general lack of one) - Conflict.Winner is a caller-
/// set fact, same convention as ConflictType/Elements. The effect reuses
/// GainHonorGameActionHandler directly (already targets context.Player, the acting player) -
/// the caller sets context.Player to the attachment's own controller, same as its own JSON
/// gameAction would via the ambient ability context.
/// </summary>
public sealed class HonoredBladeGainHonorWhenParentWins : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var honoredBlade = context.Source;
        var parent = honoredBlade.AttachedTo
            ?? throw new InvalidOperationException($"'{honoredBlade.Id}' is not currently attached to anything.");

        if (!IsParticipating(context.Game, parent))
            throw new InvalidOperationException($"'{honoredBlade.Id}' can only trigger while the attached character is participating.");

        if (context.Game.CurrentConflict?.Winner != parent.Controller)
            throw new InvalidOperationException($"'{honoredBlade.Id}' can only trigger when the attached character's controller wins the conflict.");

        new GainHonorGameActionHandler().Execute(context, null);
    }

    private static bool IsParticipating(GameState game, Card card) =>
        game.CurrentConflict is { } conflict && (conflict.Attackers.Contains(card) || conflict.Defenders.Contains(card));
}
