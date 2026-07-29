using L5R.Engine.Abilities;
using L5R.Engine.State;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// indomitable-will: after winning a conflict with exactly one participating character on
/// your side, that character does not bow as a result of the conflict's resolution.
/// Reuses the existing CardRestriction model (doesNotBow, untilEndOfConflict) directly -
/// this engine has no explicit "bow participants after a conflict" step to intercept, but
/// the restriction itself is real, queryable state via GameState.IsRestrictedFrom, same as
/// every other CardRestriction-backed effect in this engine.
/// </summary>
public sealed class IndomitableWillPreventBowOnSoloWin : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var conflict = context.Game.CurrentConflict
            ?? throw new InvalidOperationException("indomitable-will requires an active conflict.");

        if (conflict.Winner != context.Player)
            throw new InvalidOperationException("indomitable-will can only be used when its controller wins the conflict.");

        var myParticipants = conflict.Attackers.Concat(conflict.Defenders)
            .Where(c => c.Controller == context.Player)
            .ToList();

        if (myParticipants.Count != 1)
            throw new InvalidOperationException("indomitable-will can only be used when its controller has exactly one participating character.");

        context.Game.Restrictions.Add(new CardRestriction { Target = myParticipants[0], Action = "bow", Duration = "untilEndOfConflict" });
    }
}
