using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;

namespace L5R.Engine.Cards.Scripts;

/// <summary>keeper-of-fire: same shape as KeeperOfAirGainFateOnDefendedWin, gated on the fire element instead.</summary>
public sealed class KeeperOfFireGainFateOnFireDefenseWin : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var conflict = context.Game.CurrentConflict
            ?? throw new InvalidOperationException("keeper-of-fire requires an active conflict.");

        if (!conflict.Elements.Contains("fire"))
            throw new InvalidOperationException("keeper-of-fire can only trigger on a conflict declared with the fire element.");

        if (conflict.Winner != context.Player)
            throw new InvalidOperationException("keeper-of-fire can only trigger when its controller wins the conflict.");

        if (conflict.DefendingPlayer != context.Player)
            throw new InvalidOperationException("keeper-of-fire can only trigger when its controller is defending.");

        new GainFateGameActionHandler().Execute(context, null);
    }
}
