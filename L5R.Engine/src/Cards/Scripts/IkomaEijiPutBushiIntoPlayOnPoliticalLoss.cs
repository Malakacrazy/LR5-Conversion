using L5R.Engine.Abilities;
using L5R.Engine.Dsl;
using L5R.Engine.State;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// ikoma-eiji: after losing a political conflict, put a bushi character with printed cost
/// under 4 into play for free from provinces or discard pile. Needs Conflict.Loser/
/// ConflictType field inspection beyond the closed predicate vocabulary's event.card-only
/// convention. Reuses the same free-zone-move shape as keeper-initiate (ZoneMover +
/// explicit Provinces removal, since ZoneMover doesn't clear that zone on its own).
/// </summary>
public sealed class IkomaEijiPutBushiIntoPlayOnPoliticalLoss : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var eiji = context.Source;

        var conflict = context.Game.CurrentConflict
            ?? throw new InvalidOperationException($"'{eiji.Id}' requires an active conflict.");

        if (conflict.Loser != context.Player)
            throw new InvalidOperationException($"'{eiji.Id}' can only trigger when its controller loses the conflict.");

        if (conflict.ConflictType != "political")
            throw new InvalidOperationException($"'{eiji.Id}' can only trigger after a political conflict.");

        var target = context.Target
            ?? throw new InvalidOperationException($"'{eiji.Id}' requires context.Target to be set.");

        if (target.Controller != context.Player)
            throw new InvalidOperationException($"'{target.Id}' must be controlled by '{eiji.Id}''s controller.");

        if (target.Type != CardType.Character || !target.Traits.Contains("bushi"))
            throw new InvalidOperationException($"'{target.Id}' must be a bushi character.");

        if (!(target.PrintedCost < 4))
            throw new InvalidOperationException($"'{target.Id}' must have a printed cost under 4.");

        if (!context.Player.Provinces.Contains(target) && !context.Player.Discard.Contains(target))
            throw new InvalidOperationException($"'{target.Id}' must be in its controller's provinces or discard pile.");

        context.Player.Provinces.Remove(target);
        ZoneMover.MoveTo(target, context.Player.PlayArea, "play area");
    }
}
