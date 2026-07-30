using System;
using System.Linq;
using L5R.Engine.Abilities;
using L5R.Engine.Cards.Scripts;
using L5R.Engine.State;

namespace L5R.Engine.Dsl;

/// <summary>
/// Fires a card's own JSON triggeredAbilities[] entry at the exact moment its "when" event
/// already happens in production code (onCharacterEntersPlay in PlayCardGameActionHandler,
/// onCardRevealed/onBreakProvince in ConflictResolver) - not through the general
/// ChooseScriptedAction poll, since "did this specific mutation just happen" isn't a stable,
/// re-queryable state fact the way every Phase B scriptOverride adapter's own precondition
/// is (e.g. honored-general's "isSelf" when-condition is trivially always true - polling it
/// every action window would honor it every turn forever, nothing like the real "once, on
/// entry" trigger).
///
/// Every ported triggeredAbilities[] card reacts to itself (WhenCondition is "isSelf" or an
/// "and" including it) - none targets a *different* card's event - so this only ever checks
/// the one card the hook site already has a direct reference to, never a board-wide scan
/// (which would also need Player.Provinces - GameState.AllCards() deliberately excludes it,
/// and province-type cards are 6 of the 15 ported triggeredAbilities[] cards).
///
/// A plain single-target ability resolves via TargetResolver.ResolveLegalTargets, same
/// "first legal candidate" trivial-bot heuristic used everywhere else this session; per-
/// entry gameAction targets (the-art-of-peace's "every attacker/defender") already resolve
/// automatically inside AbilityExecutor.Resolve, needing no chosenTarget at all. A few
/// gameAction names need one further caller-set fact beyond a plain target (selectRing's
/// ChosenRingElement, chosenDiscard's ChosenDiscardCards) - handled by name here, since it's
/// the uniform JSON shape driving these (not bespoke C#, unlike scriptOverride), so one
/// generic firer covers all of them instead of a hand-written adapter per card.
///
/// Splits Prepare from Resolve (rather than the one-shot ExecuteTriggered) specifically so
/// shiba-yojimbo - the one scriptOverride card shaped as a triggered-ability interrupt - gets
/// a real gap to cancel a pending reaction before it resolves, the same PendingAbility/Cancel
/// mechanics AbilityExecutor already exposes for JSON "cancel" gameActions (forged-edict,
/// voice-of-honor). No general priority window exists for this - just this one, fixed check
/// against a plain chosenTarget, since shiba-yojimbo is the only ported card in this whole
/// backlog that needs it.
/// </summary>
public static class TriggeredReactionFirer
{
    public static void FireIfLegal(GameState game, Card card, string whenEvent)
    {
        var ability = card.TriggeredAbilities.FirstOrDefault(a => a.WhenEvent == whenEvent);
        if (ability is null)
            return;

        var context = new AbilityContext { Game = game, Player = card.Controller, Source = card };

        if (!PredicateEvaluator.Evaluate(ability.WhenCondition, card, context))
            return;

        Card? chosenTarget = null;
        if (ability.Target is { Choices: null, MaxStat: null, UpToNumCards: null } target)
        {
            chosenTarget = TargetResolver.ResolveLegalTargets(target, context).FirstOrDefault();
            if (chosenTarget is null)
                return;
        }

        if (!TryPopulateGameActionFacts(context, ability))
            return;

        var executor = new AbilityExecutor(new CostRegistry(), new GameActionRegistry());
        var pending = executor.PrepareTriggered(ability, context, card, chosenTarget);

        OfferShibaYojimboInterrupt(game, pending);

        executor.Resolve(pending);
    }

    /// <summary>
    /// shiba-yojimbo's own script requires the target to be a shugenja its controller
    /// controls in play - checked here first (cheaply, no side effects) so a shiba-yojimbo
    /// that isn't actually eligible never gets invoked at all, matching every other adapter's
    /// own IsLegal-before-Invoke convention. "Always cancel when legal" - same trivial-bot
    /// heuristic as every other adopted card, not the real "may" choice.
    /// </summary>
    private static void OfferShibaYojimboInterrupt(GameState game, PendingAbility pending)
    {
        var target = pending.ChosenTarget;
        if (target is null || !target.Traits.Contains("shugenja") || target.Location != "play area")
            return;

        var yojimbo = target.Controller.PlayArea.FirstOrDefault(c => c.Id == "shiba-yojimbo" && !game.IsBlanked(c));
        if (yojimbo is null)
            return;

        var interruptContext = new AbilityContext { Game = game, Player = target.Controller, Source = yojimbo, InterruptedAbility = pending };
        new ShibaYojimboCancelShugenjaTargetedAbility().Execute(interruptContext);
    }

    private static bool TryPopulateGameActionFacts(AbilityContext context, TriggeredAbilityDefinition ability)
    {
        foreach (var gameAction in ability.GameActions)
        {
            switch (gameAction.Name)
            {
                case "selectRing":
                {
                    var conflict = context.Game.CurrentConflict;
                    if (conflict is null || conflict.Elements.Count == 0)
                        return false;

                    var ring = context.Game.Rings.FirstOrDefault(r => r.Element != conflict.Elements[0]);
                    if (ring is null)
                        return false;

                    context.ChosenRingElement = ring.Element;
                    break;
                }

                case "chosenDiscard":
                {
                    var amountRef = gameAction.Params?.GetProperty("amount")
                        ?? throw new InvalidOperationException($"'{context.Source.Id}''s chosenDiscard requires params.amount.");

                    var amount = ValueRefResolver.ResolveInt(amountRef, context);
                    var opponent = context.Game.Opponent(context.Player);
                    var actualAmount = Math.Max(0, Math.Min(opponent.Hand.Count, amount));
                    context.ChosenDiscardCards = opponent.Hand.Take(actualAmount).ToList();
                    break;
                }
            }
        }

        return true;
    }
}
