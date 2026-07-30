using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Dsl.Costs;
using L5R.Engine.State;

namespace L5R.Engine.Dsl;

/// <summary>
/// Offers the two ported "wouldInterrupt" events (forged-edict, voice-of-honor) a chance to
/// cancel another event's effect before it resolves. Both are event-type cards played from
/// hand, not something an already-in-play card's ability triggers - their whole reason to
/// exist is this interrupt, they have no plain abilities.actions[] entry at all (confirmed:
/// playing either through the normal hand-play path would find no Card.Actions entry and no
/// scriptedFallback, so it would just silently discard - this offerer is the only place their
/// effect can ever actually happen).
///
/// EventResolver.ResolveAndDiscard is the sole caller: it now Prepares the being-played
/// event's own ability instead of fusing Prepare+Resolve, calls OfferInterrupts here, then
/// Resolves (a no-op if cancelled) - the same PendingAbility/Cancel() split
/// TriggeredReactionFirer already uses for shiba-yojimbo, just offered to the *opponent* of
/// the event's own player rather than the reacting card's own controller.
///
/// Hardcodes the two known interrupting card ids rather than a generic "scan hand for any
/// wouldInterrupt-triggered card" search - AbilityContext has no notion of "trigger type"
/// once parsed (Trigger is documentation-only, per TriggeredAbilityDefinition's own doc
/// comment), and these are the only two ported cards shaped this way.
/// </summary>
public static class WouldInterruptOfferer
{
    private static readonly string[] InterruptingEventIds = { "forged-edict", "voice-of-honor" };

    public static void OfferInterrupts(GameState game, Card eventCard, Player player, PendingAbility pending)
    {
        var opponent = game.Opponent(player);

        foreach (var cardId in InterruptingEventIds)
        {
            if (pending.Cancelled)
                return;

            TryPlay(game, cardId, opponent, eventCard, pending);
        }
    }

    private static void TryPlay(GameState game, string cardId, Player opponent, Card eventCard, PendingAbility pending)
    {
        var interruptingCard = opponent.Hand.FirstOrDefault(c => c.Id == cardId);
        if (interruptingCard is null)
            return;

        var ability = interruptingCard.TriggeredAbilities.FirstOrDefault(a => a.WhenEvent == "onInitiateAbilityEffects");
        if (ability is null)
            return;

        var context = new AbilityContext { Game = game, Player = opponent, Source = interruptingCard };
        if (!PredicateEvaluator.Evaluate(ability.WhenCondition, eventCard, context))
            return;

        Card? costTarget = null;
        foreach (var cost in ability.Costs)
        {
            if (cost.Name != "dishonor")
                return; // no other cost shape is needed by either ported wouldInterrupt card

            costTarget = DishonorCostHandler.ResolveLegalCandidates(context, cost.Params).FirstOrDefault();
            if (costTarget is null)
                return;
        }

        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());
        var interruptContext = new AbilityContext { Game = game, Player = opponent, Source = interruptingCard, InterruptedAbility = pending };
        executor.ExecuteTriggered(ability, interruptContext, eventCard, chosenCostTarget: costTarget);

        ZoneMover.MoveTo(interruptingCard, opponent.Discard, "discard");
    }
}
