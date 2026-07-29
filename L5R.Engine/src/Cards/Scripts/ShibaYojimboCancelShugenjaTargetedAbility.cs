using L5R.Engine.Abilities;
using L5R.Engine.State;

namespace L5R.Engine.Cards.Scripts;

/// <summary>
/// shiba-yojimbo: cancel a triggered ability whose targets include a shugenja character
/// this player controls in play ("event.context.ability.isTriggeredAbility() &&
/// event.cardTargets.some(...)"). The interrupted ability is context.InterruptedAbility
/// (same convention CancelGameActionHandler already uses) - its own chosen target(s)
/// (PendingAbility.ChosenTarget/ChosenTargets) are inspected directly rather than through
/// an event-shaped predicate.
/// </summary>
public sealed class ShibaYojimboCancelShugenjaTargetedAbility : ICardScript
{
    public void Execute(AbilityContext context)
    {
        var pending = context.InterruptedAbility
            ?? throw new InvalidOperationException("shiba-yojimbo requires context.InterruptedAbility to be set.");

        var targets = new List<Card>();
        if (pending.ChosenTarget is { } single)
            targets.Add(single);
        if (pending.ChosenTargets is { } multiple)
            targets.AddRange(multiple);

        var hasShugenjaTarget = targets.Any(card =>
            card.Traits.Contains("shugenja") && card.Controller == context.Player && card.Location == "play area");

        if (!hasShugenjaTarget)
            throw new InvalidOperationException("shiba-yojimbo can only cancel an ability targeting a shugenja character its controller has in play.");

        pending.Cancel();
    }
}
