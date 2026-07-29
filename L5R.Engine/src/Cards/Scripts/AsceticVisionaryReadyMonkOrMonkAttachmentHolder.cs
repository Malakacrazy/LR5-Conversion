using L5R.Engine.Abilities;
using L5R.Engine.Dsl.Costs;
using L5R.Engine.Dsl.GameActions;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// ascetic-visionary: while attacking, pay 1 fate to an unclaimed ring (reuses
/// PayFateToRingCostHandler directly) to ready a character that either has the monk trait
/// itself or has an attached card with the monk trait - an existential check over the
/// candidate's own attachments, beyond any vocabulary scoped to source/player/target
/// (nothing lets a predicate reference "the current candidate's own attachments"). Checked
/// via a live GameState.AllCards() scan, same convention as mountain-s-anvil-castle's own
/// attachment count.
/// </summary>
public sealed class AsceticVisionaryReadyMonkOrMonkAttachmentHolder : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var visionary = context.Source;

        if (context.Game.CurrentConflict?.Attackers.Contains(visionary) != true)
            throw new InvalidOperationException($"'{visionary.Id}' can only be used while attacking.");

        var costHandler = new PayFateToRingCostHandler();
        if (!costHandler.CanPay(context, null))
            throw new InvalidOperationException($"'{visionary.Id}' cannot pay 1 fate to an unclaimed ring.");

        var target = context.Target
            ?? throw new InvalidOperationException($"'{visionary.Id}' requires context.Target to be set.");

        var hasMonkTrait = target.Traits.Contains("monk")
            || context.Game.AllCards().Any(c => c.AttachedTo == target && c.Traits.Contains("monk"));

        if (!hasMonkTrait)
            throw new InvalidOperationException($"'{target.Id}' has no monk trait and no attached card with the monk trait.");

        costHandler.Pay(context, null);

        new ReadyGameActionHandler().Execute(context, null);
    }
}
