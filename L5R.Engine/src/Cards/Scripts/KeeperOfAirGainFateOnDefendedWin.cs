using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// keeper-of-air: after this player wins a conflict they're defending, declared with the
/// air element, gain 1 fate. Reaction trigger inspects three plain fields on Conflict
/// (Elements, Winner, DefendingPlayer) which are all caller-set facts (no conflict-
/// resolution/ring-declaration pipeline computes them automatically) - see Conflict.Winner's
/// own doc comment. Reuses GainFateGameActionHandler directly (already targets
/// context.Player).
/// </summary>
public sealed class KeeperOfAirGainFateOnDefendedWin : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var conflict = context.Game.CurrentConflict
            ?? throw new InvalidOperationException("keeper-of-air requires an active conflict.");

        if (!conflict.Elements.Contains("air"))
            throw new InvalidOperationException("keeper-of-air can only trigger on a conflict declared with the air element.");

        if (conflict.Winner != context.Player)
            throw new InvalidOperationException("keeper-of-air can only trigger when its controller wins the conflict.");

        if (conflict.DefendingPlayer != context.Player)
            throw new InvalidOperationException("keeper-of-air can only trigger when its controller is defending.");

        new GainFateGameActionHandler().Execute(context, null);
    }
}
