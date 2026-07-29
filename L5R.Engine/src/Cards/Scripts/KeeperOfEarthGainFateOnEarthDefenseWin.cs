using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;

namespace L5R.Engine.Cards.Scripts;

/// <summary>keeper-of-earth: same shape as KeeperOfAirGainFateOnDefendedWin, gated on the earth element instead.</summary>
public sealed class KeeperOfEarthGainFateOnEarthDefenseWin : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var conflict = context.Game.CurrentConflict
            ?? throw new InvalidOperationException("keeper-of-earth requires an active conflict.");

        if (!conflict.Elements.Contains("earth"))
            throw new InvalidOperationException("keeper-of-earth can only trigger on a conflict declared with the earth element.");

        if (conflict.Winner != context.Player)
            throw new InvalidOperationException("keeper-of-earth can only trigger when its controller wins the conflict.");

        if (conflict.DefendingPlayer != context.Player)
            throw new InvalidOperationException("keeper-of-earth can only trigger when its controller is defending.");

        new GainFateGameActionHandler().Execute(context, null);
    }
}
