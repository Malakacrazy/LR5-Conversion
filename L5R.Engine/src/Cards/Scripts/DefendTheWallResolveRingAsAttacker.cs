using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// defend-the-wall: after a conflict declared against this province ends, if its
/// controller won it, resolve the ring effect. Needs Conflict.DeclaredProvince/Winner
/// field inspection beyond the closed predicate vocabulary. Reuses
/// ResolveConflictRingGameActionHandler directly, same as akodo-toturi/doji-hotaru.
/// </summary>
public sealed class DefendTheWallResolveRingAsAttacker : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var defendTheWall = context.Source;

        var conflict = context.Game.CurrentConflict
            ?? throw new InvalidOperationException($"'{defendTheWall.Id}' requires an active conflict.");

        if (conflict.DeclaredProvince != defendTheWall)
            throw new InvalidOperationException($"'{defendTheWall.Id}' can only trigger when the conflict is declared against it.");

        if (conflict.Winner != context.Player)
            throw new InvalidOperationException($"'{defendTheWall.Id}' can only trigger when its controller wins the conflict.");

        new ResolveConflictRingGameActionHandler().Execute(context, null);
    }
}
