using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;

namespace L5R.Engine.Cards.Scripts;

/// <summary>keeper-of-void: same shape as KeeperOfAirGainFateOnDefendedWin, gated on the void element instead.</summary>
public sealed class KeeperOfVoidGainFateOnVoidDefenseWin : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var conflict = context.Game.CurrentConflict
            ?? throw new InvalidOperationException("keeper-of-void requires an active conflict.");

        if (!conflict.Elements.Contains("void"))
            throw new InvalidOperationException("keeper-of-void can only trigger on a conflict declared with the void element.");

        if (conflict.Winner != context.Player)
            throw new InvalidOperationException("keeper-of-void can only trigger when its controller wins the conflict.");

        if (conflict.DefendingPlayer != context.Player)
            throw new InvalidOperationException("keeper-of-void can only trigger when its controller is defending.");

        new GainFateGameActionHandler().Execute(context, null);
    }
}
