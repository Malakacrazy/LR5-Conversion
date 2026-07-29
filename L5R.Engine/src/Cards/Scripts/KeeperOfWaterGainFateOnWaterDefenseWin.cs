using L5R.Engine.Abilities;
using L5R.Engine.Dsl.GameActions;

namespace L5R.Engine.Cards.Scripts;

/// <summary>keeper-of-water: same shape as KeeperOfAirGainFateOnDefendedWin, gated on the water element instead.</summary>
public sealed class KeeperOfWaterGainFateOnWaterDefenseWin : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var conflict = context.Game.CurrentConflict
            ?? throw new InvalidOperationException("keeper-of-water requires an active conflict.");

        if (!conflict.Elements.Contains("water"))
            throw new InvalidOperationException("keeper-of-water can only trigger on a conflict declared with the water element.");

        if (conflict.Winner != context.Player)
            throw new InvalidOperationException("keeper-of-water can only trigger when its controller wins the conflict.");

        if (conflict.DefendingPlayer != context.Player)
            throw new InvalidOperationException("keeper-of-water can only trigger when its controller is defending.");

        new GainFateGameActionHandler().Execute(context, null);
    }
}
