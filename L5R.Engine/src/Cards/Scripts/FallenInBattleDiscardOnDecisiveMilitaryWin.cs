using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;
using L5R.Engine.State;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// fallen-in-battle: after winning a military conflict by 5 or more skill, discard a
/// participating character ("event.conflict.winner === context.player &&
/// event.conflict.conflictType === 'military' && event.conflict.skillDifference >= 5").
/// Conflict.SkillDifference is a caller-set fact, same convention as Winner/Loser - no
/// skill-comparison pipeline computes it automatically. "max: perConflict(1)" needs no
/// work, matching every "max"/"limit" field's established no-op precedent.
/// </summary>
public sealed class FallenInBattleDiscardOnDecisiveMilitaryWin : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var source = context.Source;
        var conflict = context.Game.CurrentConflict
            ?? throw new InvalidOperationException($"'{source.Id}' requires an active conflict.");

        if (conflict.Winner != context.Player)
            throw new InvalidOperationException($"'{source.Id}' can only trigger when its controller wins the conflict.");

        if (conflict.ConflictType != "military")
            throw new InvalidOperationException($"'{source.Id}' can only trigger after a military conflict.");

        if (conflict.SkillDifference < 5)
            throw new InvalidOperationException($"'{source.Id}' can only trigger when the conflict was won by 5 or more skill.");

        var target = context.Target
            ?? throw new InvalidOperationException($"'{source.Id}' requires context.Target to be set.");

        if (!IsParticipating(context.Game, target))
            throw new InvalidOperationException($"'{target.Id}' is not participating.");

        new DiscardFromPlayGameActionHandler().Execute(context, null);
    }

    private static bool IsParticipating(GameState game, Card card) =>
        game.CurrentConflict is { } conflict && (conflict.Attackers.Contains(card) || conflict.Defenders.Contains(card));
}
